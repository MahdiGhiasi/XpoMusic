using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Media.Playback;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using XpoMusic.Classes;
using XpoMusic.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.UI.Popups;
using Windows.Media.Core;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using XpoMusic.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace XpoMusic.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        MediaPlayer silentMediaPlayer;
        public MainPage()
        {
            this.InitializeComponent();
            VisualStateManager.GoToState(this, nameof(SplashScreenVisualState), false);

            silentMediaPlayer = new MediaPlayer
            {
                Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Media/silent.wav")),
                IsLoopingEnabled = true,
            };
            silentMediaPlayer.CommandManager.IsEnabled = false;
        }

        private async void XpoWebView_ActionRequested(object sender, Controls.XpoMusicWebApp.XpoMusicWebAppActionRequest e)
        {
            try
            {
                switch (e)
                {
                    case Controls.XpoMusicWebApp.XpoMusicWebAppActionRequest.OpenSettingsFlyout:
                        break;
                    case Controls.XpoMusicWebApp.XpoMusicWebAppActionRequest.OpenAboutFlyout:
                        break;
                    case Controls.XpoMusicWebApp.XpoMusicWebAppActionRequest.OpenDonateFlyout:
                        break;
                    case Controls.XpoMusicWebApp.XpoMusicWebAppActionRequest.GoToCompactOverlay:
                        await GoToCompactOverlayMode();
                        break;
                    case Controls.XpoMusicWebApp.XpoMusicWebAppActionRequest.GoToNowPlaying:
                        GoToNowPlayingMode();
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"exception in XpoWebView_ActionRequested({e}): {ex}");
            }
        }

        private async void GoToNowPlayingMode()
        {
            VisualStateManager.GoToState(this, nameof(NowPlayingVisualState), false);
            //nowPlaying.ActionRequested += NowPlaying_ActionRequested;
            //topBar.UpdateTitleBarColors(Theme.Dark);

            SetExtendToTitleBar(extend: true);

            AnalyticsHelper.PageView("NowPlaying");
        }

        private async Task GoToCompactOverlayMode()
        {
            if (!ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                return;

            VisualStateManager.GoToState(this, nameof(CompactOverlayVisualState), false);

            var viewMode = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            viewMode.ViewSizePreference = ViewSizePreference.Custom;
            viewMode.CustomSize = LocalConfiguration.CompactOverlaySize;

            var modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, viewMode);
            
            if (modeSwitched)
            {
                // TODO
                //nowPlaying.ActionRequested += NowPlaying_ActionRequested;
                //topBar.UpdateTitleBarColors(Theme.Dark);

                SetExtendToTitleBar(extend: true);

                AnalyticsHelper.PageView("CompactOverlay");
            }
            else
            {
                GoToWebAppViewMode();
            }
        }

        private async void GoToWebAppViewMode()
        {
            if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay)
            {
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            }

            SetExtendToTitleBar(extend: false);
            VisualStateManager.GoToState(this, nameof(MainScreenVisualState), false);
        }

        private void SetExtendToTitleBar(bool extend)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = extend;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplicationView.GetForCurrentView().SetPreferredMinSize(LocalConfiguration.WindowMinSize);

                var currentViewNavManager = SystemNavigationManager.GetForCurrentView();
                currentViewNavManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                currentViewNavManager.BackRequested += App_BackRequested;

                SetExtendToTitleBar(false);

                // Update app constants from server
                AppConstants.Update();

                // Update assets from server
                AssetManager.UpdateAssets();

                // Play silent sound to avoid suspending the app when it's minimized.
                silentMediaPlayer.Play();

                // Media controls are necessary for the audio to continue when app is minimized.
                MediaControlsHelper.Init(Dispatcher);
                MediaControlsHelper.TrackChanged += (ss, trackChangedArgs) =>
                {
                    //if (nowPlaying.IsOpen)
                    //    nowPlaying.PlayChangeTrackAnimation(
                    //        reverse: (trackChangedArgs.Direction == TrackChangeDirection.Backward));
                };

                // Show what's new if necessary
                //if (WhatsNewHelper.ShouldShowWhatsNew())
                //    shouldShowWhatsNew = true;

                // LiveTileHelper.InitLiveTileUpdates();
                JumpListHelper.DeleteRecentJumplistEntries();

                AnalyticsHelper.PageView("MainPage", setNewSession: true);
                AnalyticsHelper.Log("mainEvent", "appOpened", SystemInformation.OperatingSystemVersion.ToString());

                //developerMessage = await DeveloperMessageHelper.GetNextDeveloperMessage();

                // Window.Current.CoreWindow.KeyDown does not capture Alt events, but AcceleratorKeyActivated does.
                // NOTE: This event captures all key events, even when WebView is focused.
                // CoreWindow.GetForCurrentThread().Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
            }
            catch (Exception ex)
            {
                AnalyticsHelper.Log("mainPageLoadedException", ex.Message, ex.ToString());
                await new MessageDialog(ex.ToString(), "MainPage:Loaded unhandled exception").ShowAsync();
            }
        }

        private async void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (nowPlaying.IsOpen)
            {
                e.Handled = true;
                NowPlaying_ActionRequested(nowPlaying, new ActionRequestedEventArgs
                {
                    Action = NowPlayingView.Action.Back,
                });
            }
            else if (xpoWebView.BackEnabled)
            {
                e.Handled = true;

                var result = await xpoWebView.Controller.GoBackIfPossible();
                logger.Info($"GoBackIfPossible() result = {result}");
            }
        }

        private void XpoWebView_PageLoadBegin(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(SplashScreenVisualState), false);
        }

        private void XpoWebView_PageLoadFinished(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(MainScreenVisualState), false);
        }

        private void XpoWebView_WebAppLoaded(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(MainScreenVisualState), false);
        }

        private void NowPlaying_ActionRequested(object sender, Controls.ActionRequestedEventArgs e)
        {
            switch (e.Action)
            {
                case Controls.NowPlayingView.Action.Back:
                    GoToWebAppViewMode();
                    break;
                case Controls.NowPlayingView.Action.PlayQueue:
                    break;
                case Controls.NowPlayingView.Action.SeekPlayback:
                    break;
                case Controls.NowPlayingView.Action.SeekVolume:
                    break;
                default:
                    break;
            }
        }
    }
}
