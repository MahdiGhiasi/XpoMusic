using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Helpers;
using ModernSpotifyUWP.SpotifyApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ModernSpotifyUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string SpotifyPwaUrlBeginsWith = "https://open.spotify.com";

        MediaPlayer silentMediaPlayer;
        bool splashClosed = false;
        private CompactOverlayView compactOverlayView;
        private Uri loadFailedUrl;
        private DispatcherTimer playCheckTimer;
        private string prevCurrentPlaying;

        public MainPage()
        {
            this.InitializeComponent();

            silentMediaPlayer = new MediaPlayer
            {
                Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/Media/silent.wav")),
                IsLoopingEnabled = true,
            };
            silentMediaPlayer.CommandManager.IsEnabled = false;

            loadFailedAppVersionText.Text = PackageHelper.GetAppVersion();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var targetUrl = GetTargetUrl(e);

            if (TokenHelper.HasTokens())
            {
                mainWebView.Navigate(new Uri(targetUrl));
            }
            else
            {
                var authorizationUrl = Authorization.GetAuthorizationUrl(targetUrl);
                mainWebView.Navigate(new Uri(authorizationUrl));
            }
        }

        private string GetTargetUrl(NavigationEventArgs e)
        {
            var parameter = e.Parameter as string;
            if (!string.IsNullOrEmpty(parameter))
            {
                try
                {
                    // Launched from a secondary tile
                    var urlDecoder = new WwwFormUrlDecoder(parameter);
                    var pageUrl = urlDecoder.GetFirstValueByName("pageUrl");

                    return pageUrl;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Parsing input parameter {e.Parameter.ToString()} failed. {ex}");
                }
            }

            //mainWebView.Navigate(new Uri("https://accounts.spotify.com/login?continue=https%3A%2F%2Fopen.spotify.com%2F"));
            return "https://open.spotify.com/";
        }

        public async void NavigateToSecondaryTile(string parameter)
        {
            try
            {
                // Launched from a secondary tile
                var urlDecoder = new WwwFormUrlDecoder(parameter);
                var pageUrl = urlDecoder.GetFirstValueByName("pageUrl");

                await NavigateToSpotifyUrl(pageUrl);

                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Parsing input parameter {parameter} failed. {ex}");
            }
        }

        private async Task NavigateToSpotifyUrl(string url)
        {
            var currentUrl = await mainWebView.InvokeScriptAsync("eval", new String[] { "document.location.href;" });

            if (currentUrl.ToLower().StartsWith(SpotifyPwaUrlBeginsWith.ToLower()))
            {
                var script = File.ReadAllText("InjectedAssets/navigateToPage.js")
                    + $"navigateToPage('{url.Replace("'", "\\'")}');";

                await mainWebView.InvokeScriptAsync("eval", new string[] { script });
            }
            else
            {
                mainWebView.Navigate(new Uri(url));
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            ApplicationView.GetForCurrentView().SetPreferredMinSize(LocalConfiguration.WindowMinSize);

            // Play silent sound to avoid suspending the app when it's minimized.
            silentMediaPlayer.Play();

            // Media controls are necessary for the audio to continue when app is minimized.
            var mediaControls = SystemMediaTransportControls.GetForCurrentView();
            mediaControls.IsEnabled = true;
            mediaControls.IsPreviousEnabled = true;
            mediaControls.IsNextEnabled = true;
            mediaControls.IsPlayEnabled = true;
            mediaControls.IsPauseEnabled = true;
            mediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
            mediaControls.ButtonPressed += SystemControls_ButtonPressed;
            await mediaControls.DisplayUpdater.CopyFromFileAsync(MediaPlaybackType.Music,
                await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Media/silent.wav")));

            PlayStatusTracker.MediaControls = mediaControls;
            PlayStatusTracker.StartRegularRefresh();

            // Show what's new if necessary
            if (WhatsNewHelper.ShouldShowWhatsNew())
            {
                OpenWhatsNew();
            }

            playCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            playCheckTimer.Tick += PlayCheckTimer_Tick;
            playCheckTimer.Start();

            StoreEventHelper.Log("appOpened");
        }

        private async void PlayCheckTimer_Tick(object sender, object e)
        {
            try
            {
                var script = File.ReadAllText("InjectedAssets/checkCurrentPlaying.js");
                var currentPlaying = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

                if (currentPlaying != prevCurrentPlaying)
                {
                    prevCurrentPlaying = currentPlaying;
                    await PlayStatusTracker.RefreshPlayStatus();
                }
            }
            catch { }
        }

        private async void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var mediaControls = SystemMediaTransportControls.GetForCurrentView();

                try
                {
                    switch (e.Button)
                    {
                        case SystemMediaTransportControlsButton.Play:
                            if (await (new Player()).ResumePlaying())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;

                            break;
                        case SystemMediaTransportControlsButton.Pause:
                            if (await (new Player()).Pause())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;

                            break;
                        case SystemMediaTransportControlsButton.Stop:
                            if (await (new Player()).Pause())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;

                            break;
                        case SystemMediaTransportControlsButton.Next:
                            await (new Player()).NextTrack();

                            break;
                        case SystemMediaTransportControlsButton.Previous:
                            await (new Player()).PreviousTrack();

                            break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    UnauthorizedHelper.OnUnauthorizedError();
                }
            });
        }

        private async void MainWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (!splashClosed)
                CloseSplash();

            if (e.Uri.ToString().StartsWith(Authorization.RedirectUri))
            {
                FinalizeAuthorization(e.Uri.ToString());
            }

            if (e.Uri.ToString().ToLower().Contains(SpotifyPwaUrlBeginsWith.ToLower()))
            {
                await InjectInitScript();
            }

            if (!await CheckLoggedIn())
            {
                TokenHelper.ClearTokens();
                var authorizationUrl = Authorization.GetAuthorizationUrl("https://accounts.spotify.com/login?continue=https%3A%2F%2Fopen.spotify.com%2F");
                mainWebView.Navigate(new Uri(authorizationUrl));
                StoreEventHelper.Log("notLoggedIn");
            }
        }

        private async Task<bool> CheckLoggedIn()
        {
            var script = File.ReadAllText("InjectedAssets/isLoggedInCheck.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result != "0");
        }

        private async void CloseSplash()
        {
            splashClosed = true;

            await Task.Delay(500);

            splashHideStoryboard.Begin();
            await Task.Delay(400);
            splashScreen.Visibility = Visibility.Collapsed;
        }

        private async Task InjectInitScript()
        {
            // TODO: Add js code so it does not inject again if already injected

            var checkIfInjected = "((document.getElementsByTagName('body')[0].getAttribute('data-scriptinjection') == null) ? '0' : '1');";
            var injected = await mainWebView.InvokeScriptAsync("eval", new string[] { checkIfInjected });

            if (injected != "1")
            {
                var script = File.ReadAllText("InjectedAssets/initScript.js");
                await mainWebView.InvokeScriptAsync("eval", new string[] { script });
            }
        }

        private async void MainWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Debug.WriteLine("Page: " + args.Uri.ToString());

            if (args.Uri.ToString().EndsWith("#xpotifygoback"))
            {
                args.Cancel = true;

                //var len = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.history.length.toString()" });
                //Debug.WriteLine("Len: " + len);
                //var curLocation = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
                //Debug.WriteLine("curLocation: " + curLocation);

                await mainWebView.InvokeScriptAsync("eval", new string[] { "window.history.go(-1);" });

                //var len2 = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.history.length.toString()" });
                //Debug.WriteLine("Len2: " + len2);
                //var curLocation2 = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
                //Debug.WriteLine("curLocation2: " + curLocation2);
            }
            else if (args.Uri.ToString().EndsWith("#xpotifysettings"))
            {
                args.Cancel = true;
                OpenSettings();
                StoreEventHelper.Log("settingsOpened");
            }
            else if (args.Uri.ToString().EndsWith("#xpotifypintostart"))
            {
                args.Cancel = true;

                await PinPageToStart();
                StoreEventHelper.Log("pinToStart");
            }
            else if (args.Uri.ToString().EndsWith("#xpotifycompactoverlay"))
            {
                args.Cancel = true;

                await GoToCompactOverlayMode();
                StoreEventHelper.Log("compactOverlayOpened");
            }

        }

        private async Task GoToCompactOverlayMode()
        {
            if (!ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                return;

            var viewMode = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            viewMode.ViewSizePreference = ViewSizePreference.Custom;
            viewMode.CustomSize = LocalConfiguration.CompactOverlaySize;

            var modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, viewMode);

            if (modeSwitched)
            {
                compactOverlayView = new CompactOverlayView();
                compactOverlayView.ExitCompactOverlayRequested += CompactOverlayView_ExitCompactOverlayRequested;
                mainGrid.Children.Add(compactOverlayView);
            }
        }

        private async void CompactOverlayView_ExitCompactOverlayRequested(object sender, EventArgs e)
        {
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

            compactOverlayView.ExitCompactOverlayRequested -= CompactOverlayView_ExitCompactOverlayRequested;
            mainGrid.Children.Remove(compactOverlayView);

            StoreEventHelper.Log("compactOverlayClosed");
        }

        private async Task PinPageToStart()
        {
            var findPageTitleScript = File.ReadAllText("InjectedAssets/findPageTitle.js");
            var pageUrl = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
            var pageTitle = await mainWebView.InvokeScriptAsync("eval", new string[] { findPageTitleScript });

            await TileHelper.PinPageToStart(pageUrl, pageTitle);
        }

        private void MainWebView_FrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            Debug.WriteLine("Frame: " + args.Uri.ToString());
        }

        private void RetryConnectButton_Click(object sender, RoutedEventArgs e)
        {
            loadFailedMessage.Visibility = Visibility.Collapsed;
            mainWebView.Navigate(loadFailedUrl);
        }

        private void MainWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(Authorization.RedirectUri))
            {
                FinalizeAuthorization(e.Uri.ToString());
                return;
            }

            loadFailedMessage.Visibility = Visibility.Visible;
            loadFailedUrlText.Text = e.Uri.ToString();
            loadFailedUrl = e.Uri;
            errorMessageText.Text = e.WebErrorStatus.ToString();
        }

        private async void FinalizeAuthorization(string url)
        {
            try
            {
                var urlDecoder = new WwwFormUrlDecoder(url.Substring(url.IndexOf('?') + 1));
                await Authorization.RetrieveAndSaveTokensFromAuthCode(urlDecoder.GetFirstValueByName("code"));
                mainWebView.Navigate(new Uri(urlDecoder.GetFirstValueByName("state")));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Authorization failed. " + ex.ToString());

                var authorizationUrl = Authorization.GetAuthorizationUrl("https://open.spotify.com/");
                mainWebView.Navigate(new Uri(authorizationUrl));
            }
        }

        private void OpenSettings()
        {
            overlay.Visibility = Visibility.Visible;
            settingsFlyout.Visibility = Visibility.Visible;
            overlayShowStoryboard.Begin();
        }

        private void OpenWhatsNew()
        {
            whatsNewFlyout.InitFlyout();
            overlay.Visibility = Visibility.Visible;
            whatsNewFlyout.Visibility = Visibility.Visible;
            overlayShowStoryboard.Begin();
        }

        private void WhatsNewFlyout_FlyoutCloseRequest(object sender, EventArgs e)
        {
            CloseOverlays();
        }

        private void SettingsFlyout_FlyoutCloseRequest(object sender, EventArgs e)
        {
            CloseOverlays();
        }

        private async void CloseOverlays()
        {
            overlayHideStoryboard.Begin();
            await Task.Delay(250);
            overlay.Visibility = Visibility.Collapsed;
            settingsFlyout.Visibility = Visibility.Collapsed;
            whatsNewFlyout.Visibility = Visibility.Collapsed;
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
