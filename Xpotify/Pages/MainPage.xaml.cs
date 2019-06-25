using Microsoft.Toolkit.Uwp.Helpers;
using Xpotify.Classes;
using Xpotify.Classes.Model;
using Xpotify.Helpers;
using Xpotify.Helpers.Integration;
using Xpotify.SpotifyApi;
using Xpotify.XpotifyApi.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Xpotify.Controls;
using static Xpotify.Helpers.MediaControlsHelper.TrackChangedEventArgs;
using Windows.System;
using Windows.UI.Xaml.Input;

namespace Xpotify.Pages
{
    public sealed partial class MainPage : Page
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        MediaPlayer silentMediaPlayer;
        private bool shouldShowWhatsNew = false;
        private DeveloperMessage developerMessage = null;
        private bool isNowPlayingEnabled = false;

        private DispatcherTimer analyticsHeartbeatTimer;

        private readonly string _playQueueUri = "https://open.spotify.com/queue";

        public MainPage()
        {
            ProxyHelper.ApplyProxySettings();
            this.InitializeComponent();

            silentMediaPlayer = new MediaPlayer
            {
                Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Media/silent.wav")),
                IsLoopingEnabled = true,
            };
            silentMediaPlayer.CommandManager.IsEnabled = false;

            analyticsHeartbeatTimer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMinutes(15),
            };
            analyticsHeartbeatTimer.Tick += AnalyticsHeartbeatTimer_Tick;

            xpotifyWebView.RequestedTheme = (ThemeHelper.GetCurrentTheme() == Theme.Light) ? ElementTheme.Light : ElementTheme.Dark;

            VisualStateManager.GoToState(this, nameof(SplashScreenVisualState), false);
        }

        private void AnalyticsHeartbeatTimer_Tick(object sender, object e)
        {
            if (!AppConstants.Instance.HeartbeatEnabled)
                return;

            // Keep the session alive in analytics
            AnalyticsHelper.Log("Heartbeat", "Heartbeat");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var targetUrl = GetTargetUrl(e);

            if (TokenHelper.HasTokens())
            {
                if (LocalConfiguration.IsLoggedInByFacebook)
                {
                    // We need to open the login page and click on facebook button
                    logger.Info("Logging in via Facebook...");
                    var loginUrl = "https://accounts.spotify.com/login?continue=" + System.Net.WebUtility.UrlEncode(targetUrl);
                    xpotifyWebView.Controller.Navigate(new Uri(loginUrl));
                }
                else
                {
                    xpotifyWebView.Controller.Navigate(new Uri(targetUrl));
                }
            }
            else
            {
                xpotifyWebView.Authorize(targetUrl, clearExisting: false);
            }
        }

        private string GetTargetUrl(NavigationEventArgs e)
        {
            var destinationUrl = "https://open.spotify.com";

            var parameter = e.Parameter as string;
            if (!string.IsNullOrEmpty(parameter))
            {
                try
                {
                    // Launched from a secondary tile
                    var urlDecoder = new WwwFormUrlDecoder(parameter);
                    var pageUrl = urlDecoder.GetFirstValueByName("pageUrl");

                    var autoplayEntry = urlDecoder.FirstOrDefault(x => x.Name == "autoplay");
                    if (autoplayEntry != null)
                    {
                        xpotifyWebView.AutoPlayAction = autoplayEntry.Value == "track" ? AutoPlayAction.Track : AutoPlayAction.Playlist;
                    }
                    else
                    {
                        xpotifyWebView.AutoPlayAction = AutoPlayAction.None;
                    }

                    var sourceEntry = urlDecoder.FirstOrDefault(x => x.Name == "source");
                    if (sourceEntry != null && sourceEntry.Value == "cortana" && LocalConfiguration.OpenInMiniViewByCortana)
                    {
                        OpenCompactOverlayForAutoPlay();
                    }

                    destinationUrl = pageUrl;
                }
                catch (Exception ex)
                {
                    logger.Info($"Parsing input parameter {e.Parameter.ToString()} failed. {ex}");
                }
            }

            return "https://open.spotify.com/static/offline.html?redirectUrl=" + System.Net.WebUtility.UrlEncode(destinationUrl);
        }

        private async void OpenCompactOverlayForAutoPlay()
        {
            await GoToCompactOverlayMode();
            nowPlaying.ActivateProgressRing();
        }

        public void NavigateToSecondaryTile(string parameter)
        {
            // Launched from a secondary tile
            if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay)
                CloseCompactOverlay();

            NavigateWithConfig(parameter);
        }

        public async void NavigateWithConfig(string parameter)
        {
            try
            {
                var urlDecoder = new WwwFormUrlDecoder(parameter);
                var pageUrl = urlDecoder.GetFirstValueByName("pageUrl");

                await xpotifyWebView.Controller.NavigateToSpotifyUrl(pageUrl);

                var autoplayEntry = urlDecoder.FirstOrDefault(x => x.Name == "autoplay");
                AutoPlayAction action = AutoPlayAction.None;
                if (autoplayEntry != null)
                    action = autoplayEntry.Value == "track" ? AutoPlayAction.Track : AutoPlayAction.Playlist;

                if (action != AutoPlayAction.None)
                    await xpotifyWebView.Controller.AutoPlay(action);

                return;
            }
            catch (Exception ex)
            {
                logger.Info($"Parsing input parameter {parameter} failed. {ex}");
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(LocalConfiguration.WindowMinSize);

            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;

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
                if (nowPlaying.IsOpen)
                    nowPlaying.PlayChangeTrackAnimation(
                        reverse: (trackChangedArgs.Direction == TrackChangeDirection.Backward));
            };

            // Show what's new if necessary
            if (WhatsNewHelper.ShouldShowWhatsNew())
                shouldShowWhatsNew = true;

            AnalyticsHelper.PageView("MainPage");
            AnalyticsHelper.Log("mainEvent", "appOpened", SystemInformation.OperatingSystemVersion.ToString());
            
            developerMessage = await DeveloperMessageHelper.GetNextDeveloperMessage();

            LyricsViewerIntegrationHelper.InitIntegration();
            LiveTileHelper.InitLiveTileUpdates();
            JumpListHelper.DeleteRecentJumplistEntries();

            // Window.Current.CoreWindow.KeyDown does not capture Alt events, but AcceleratorKeyActivated does.
            // NOTE: This event captures all key events, even when WebView is focused.
            CoreWindow.GetForCurrentThread().Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        private async void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        {
            if (e.EventType == CoreAcceleratorKeyEventType.KeyDown || e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown)
            {
                // We won't process the event if the currently focused element is WebView,
                // as the web app can handle keyboard shortcuts itself.
                if (FocusManager.GetFocusedElement().GetType() == typeof(WebView))
                    return;

                var shiftState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
                var shiftPressed = (shiftState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);
                var ctrlPressed = (ctrlState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                var altState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Menu);
                var altPressed = (altState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                logger.Info(e.VirtualKey);
                var isHandled = await KeyboardShortcutHelper.KeyDown(e.VirtualKey, shiftPressed, ctrlPressed, altPressed, xpotifyWebView.Controller, nowPlaying);
                if (isHandled == KeyboardShortcutHelper.KeyDownProcessResult.AskJs)
                {
                    logger.Info("Sending it to js...");
                    int charCode = (int)e.VirtualKey;
                    var handledByJs = await xpotifyWebView.Controller.OnKeyDown(charCode, shiftPressed, ctrlPressed, altPressed);
                    if (handledByJs)
                    {
                        if (nowPlaying.IsOpen)
                        {
                            if (nowPlaying.ViewMode == NowPlayingView.NowPlayingViewMode.CompactOverlay)
                                CloseCompactOverlay();
                            else
                                CloseNowPlaying();
                        }
                    }
                }
                else if (isHandled == KeyboardShortcutHelper.KeyDownProcessResult.GoBack)
                {
                    if (nowPlaying.IsOpen)
                    {
                        if (nowPlaying.ViewMode == NowPlayingView.NowPlayingViewMode.CompactOverlay)
                            CloseCompactOverlay();
                        else
                            CloseNowPlaying();
                    }
                }
            }
            
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= App_BackRequested;

            base.OnNavigatingFrom(e);
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
            else if (xpotifyWebView.BackEnabled)
            {
                e.Handled = true;

                var result = await xpotifyWebView.Controller.GoBackIfPossible();
                logger.Info($"GoBackIfPossible() result = {result}");
            }
        }
        
        private void GoToNowPlayingMode()
        {
            VisualStateManager.GoToState(this, nameof(NowPlayingVisualState), false);
            nowPlaying.ActionRequested += NowPlaying_ActionRequested;
            topBar.UpdateTitleBarColors(Theme.Dark);

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
                nowPlaying.ActionRequested += NowPlaying_ActionRequested;
                topBar.UpdateTitleBarColors(Theme.Dark);

                AnalyticsHelper.PageView("CompactOverlay");
            }
            else
            {
                VisualStateManager.GoToState(this, nameof(MainScreenVisualState), false);
            }
        }

        private async void NowPlaying_ActionRequested(object sender, ActionRequestedEventArgs e)
        {
            if ((sender as NowPlayingView).ViewMode == NowPlayingView.NowPlayingViewMode.CompactOverlay)
            {
                if (e.Action == NowPlayingView.Action.Back)
                {
                    CloseCompactOverlay();
                }
                else if (e.Action == NowPlayingView.Action.SeekPlayback)
                {
                    SeekPlayback((double)e.AdditionalData);
                }
                else if (e.Action == NowPlayingView.Action.SeekVolume)
                {
                    SeekVolume((double)e.AdditionalData);
                }
            }
            else
            {
                if (e.Action == NowPlayingView.Action.Back)
                {
                    CloseNowPlaying();
                }
                else if (e.Action == NowPlayingView.Action.PlayQueue)
                {
                    await xpotifyWebView.Controller.NavigateToSpotifyUrl(_playQueueUri);
                    CloseNowPlaying();
                }
                else if (e.Action == NowPlayingView.Action.SeekPlayback)
                {
                    SeekPlayback((double)e.AdditionalData);
                }
                else if (e.Action == NowPlayingView.Action.SeekVolume)
                {
                    SeekVolume((double)e.AdditionalData);
                }
            }
        }

        private async void SeekPlayback(double percentage)
        {
            try
            {
                PlayStatusTracker.SeekPlayback(percentage);
                await xpotifyWebView.Controller.SeekPlayback(percentage);
            }
            catch (Exception ex)
            {
                logger.Warn("SeekPlayback failed: " + ex.ToString());
            }
        }

        private async void SeekVolume(double percentage)
        {
            try
            {
                PlayStatusTracker.SeekVolume(percentage);
                await xpotifyWebView.Controller.SeekVolume(percentage);
            }
            catch (Exception ex)
            {
                logger.Warn("SeekVolume failed: " + ex.ToString());
            }
        }

        private async void CloseCompactOverlay()
        {
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            nowPlaying.ActionRequested -= NowPlaying_ActionRequested;
            VisualStateManager.GoToState(this, nameof(MainScreenVisualState), false);
            topBar.InitTitleBar();

            AnalyticsHelper.PageView("MainPage");

            GetBackFocusToWebView();
        }

        private async void CloseNowPlaying()
        {
            nowPlaying.ActionRequested -= NowPlaying_ActionRequested;
            topBar.InitTitleBar();

            VisualStateManager.GoToState(this, nameof(NowPlayingClosingVisualState), false);
            await Task.Delay(200);
            VisualStateManager.GoToState(this, nameof(MainScreenVisualState), false);

            AnalyticsHelper.PageView("MainPage");

            GetBackFocusToWebView();
        }

        private async void GetBackFocusToWebView()
        {
            await Task.Delay(200);
            xpotifyWebView.SetFocusToWebView();
        }

        private void XpotifyWebView_PageLoaded(object sender, EventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(MainScreenQuickVisualState), false);

            OnWebViewLoadCompleted();

            isNowPlayingEnabled = false;
            PlayStatusTracker.LastPlayStatus.Updated -= LastPlayStatus_Updated;
        }

        private void XpotifyWebView_WebAppLoaded(object sender, EventArgs e)
        {
            if (splashScreen.SplashState == Controls.SplashScreen.SplashScreenShowState.Visible)
                VisualStateManager.GoToState(this, nameof(MainScreenVisualState), false);

            OnWebViewLoadCompleted();

            if (!isNowPlayingEnabled)
            {
                EnableNowPlayingWhenReady();
            }
        }

        private void OnWebViewLoadCompleted()
        {
            if (shouldShowWhatsNew)
            {
                flyoutContainer.OpenWhatsNew();
                shouldShowWhatsNew = false;
            }

            if (developerMessage != null)
            {
                flyoutContainer.OpenDeveloperMessage(developerMessage);
                developerMessage = null;
            }
        }

        private async void EnableNowPlayingWhenReady()
        {
            try
            {
                PlayStatusTracker.LastPlayStatus.Updated += LastPlayStatus_Updated;
                var result = await EnableNowPlayingIfReady();

                if (result)
                    PlayStatusTracker.LastPlayStatus.Updated -= LastPlayStatus_Updated;

                logger.Info($"EnableNowPlaying (initial) result = {result}");
            }
            catch (Exception ex)
            {
                logger.Warn("EnableNowPlaying (initial) failed: " + ex.ToString());
            }
        }

        private async void LastPlayStatus_Updated(object sender, EventArgs e)
        {
            try
            {
                var result = await EnableNowPlayingIfReady();

                logger.Info($"EnableNowPlaying (event) result = {result}");
            }
            catch (Exception ex)
            {
                logger.Warn("EnableNowPlaying (event) failed: " + ex.ToString());
            }
        }

        private async Task<bool> EnableNowPlayingIfReady()
        {
            var nowPlayingShouldBeEnabled = !string.IsNullOrWhiteSpace(PlayStatusTracker.LastPlayStatus.SongId);

            if (!isNowPlayingEnabled && nowPlayingShouldBeEnabled)
            {
                PlayStatusTracker.LastPlayStatus.Updated -= LastPlayStatus_Updated;
                await xpotifyWebView.Controller.EnableNowPlaying();
                isNowPlayingEnabled = true;
                return true;
            }

            return false;
        }

        private async void XpotifyWebView_ActionRequested(object sender, XpotifyWebApp.XpotifyWebAppActionRequest request)
        {
            switch (request)
            {
                case XpotifyWebApp.XpotifyWebAppActionRequest.OpenSettingsFlyout:
                    flyoutContainer.OpenSettings();
                    break;
                case XpotifyWebApp.XpotifyWebAppActionRequest.OpenAboutFlyout:
                    flyoutContainer.OpenAbout();
                    break;
                case XpotifyWebApp.XpotifyWebAppActionRequest.OpenDonateFlyout:
                    flyoutContainer.OpenDonate();
                    break;
                case XpotifyWebApp.XpotifyWebAppActionRequest.GoToCompactOverlay:
                    if (isNowPlayingEnabled)
                        await GoToCompactOverlayMode();
                    break;
                case XpotifyWebApp.XpotifyWebAppActionRequest.GoToNowPlaying:
                    if (isNowPlayingEnabled)
                        GoToNowPlayingMode();
                    break;
                case XpotifyWebApp.XpotifyWebAppActionRequest.ShowSplashScreen:
                    VisualStateManager.GoToState(this, nameof(SplashScreenVisualState), false);
                    break;
                default:
                    break;
            }
        }

        private async void TopBar_OpenSpotifyUriRequested(object sender, string uri)
        {
            await xpotifyWebView.Controller.NavigateToSpotifyUrl(uri);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay)
            {
                LocalConfiguration.CompactOverlaySize = new Size(this.ActualWidth, this.ActualHeight);
            }
        }
    }
}
