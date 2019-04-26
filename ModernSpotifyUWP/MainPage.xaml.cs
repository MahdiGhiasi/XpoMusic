using Microsoft.Toolkit.Uwp.Helpers;
using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Classes.Model;
using ModernSpotifyUWP.Helpers;
using ModernSpotifyUWP.SpotifyApi;
using Newtonsoft.Json;
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
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ModernSpotifyUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        MediaPlayer silentMediaPlayer;
        private CompactOverlayView compactOverlayView;
        private Uri loadFailedUrl;
        private DispatcherTimer playCheckTimer, stuckDetectTimer, clipboardCheckTimer;
        private string prevCurrentPlaying;
        private LocalStoragePlayback initialPlaybackState = null;
        private int stuckDetectCounter = 0;
        private DateTime lastStuckFixApiCall;
        private bool isWebViewGoingBack = false;
        private string webViewPreviousUri = "";
        private bool shouldShowWhatsNew = false;

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

            WebViewHelper.Init(this.mainWebView);

            loadFailedAppVersionText.Text = PackageHelper.GetAppVersionString();
            VisualStateManager.GoToState(this, "SplashScreen", false);
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
                    WebViewHelper.Navigate(new Uri(loginUrl));
                }
                else
                {
                    WebViewHelper.Navigate(new Uri(targetUrl));
                }
            }
            else
            {
                Authorize(targetUrl, clearExisting: false);
            }
        }

        private void Authorize(string targetUrl, bool clearExisting)
        {
            if (clearExisting)
            {
                WebViewHelper.ClearCookies();
                TokenHelper.ClearTokens();
                LocalConfiguration.IsLoggedInByFacebook = false;
            }

            var authorizationUrl = Authorization.GetAuthorizationUrl(targetUrl);
            WebViewHelper.Navigate(new Uri(authorizationUrl));
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

                    destinationUrl = pageUrl;
                }
                catch (Exception ex)
                {
                    logger.Info($"Parsing input parameter {e.Parameter.ToString()} failed. {ex}");
                }
            }

            return "https://open.spotify.com/static/offline.html?redirectUrl=" + System.Net.WebUtility.UrlEncode(destinationUrl);
        }

        public async void NavigateToSecondaryTile(string parameter)
        {
            try
            {
                // Launched from a secondary tile

                if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay)
                    CloseCompactOverlay();

                var urlDecoder = new WwwFormUrlDecoder(parameter);
                var pageUrl = urlDecoder.GetFirstValueByName("pageUrl");

                await WebViewHelper.NavigateToSpotifyUrl(pageUrl);

                return;
            }
            catch (Exception ex)
            {
                logger.Info($"Parsing input parameter {parameter} failed. {ex}");
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitTitleBar();
            ApplicationView.GetForCurrentView().SetPreferredMinSize(LocalConfiguration.WindowMinSize);
            Window.Current.CoreWindow.Activated += Window_Activated;

            // Update app constants from server
            AppConstants.Instance.Update();

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
                shouldShowWhatsNew = true;
            }

            playCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            playCheckTimer.Tick += PlayCheckTimer_Tick;
            playCheckTimer.Start();

            clipboardCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3),
            };
            clipboardCheckTimer.Tick += ClipboardCheckTimer_Tick;
            clipboardCheckTimer.Start();

            stuckDetectTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4),
            };
            stuckDetectTimer.Tick += StuckDetectTimer_Tick;
            stuckDetectTimer.Start();

            AnalyticsHelper.PageView("MainPage");
            AnalyticsHelper.Log("mainEvent", "appOpened", SystemInformation.OperatingSystemVersion.ToString());
            
            if (LocalConfiguration.Theme == Theme.Light)
                splashScreenToLightStoryboard.Begin();
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                CheckForSpotifyUriInClipboard();
            }
        }

        private async void CheckForSpotifyUriInClipboard()
        {
            try
            {
                var isSpotifyUriPresent = await ClipboardHelper.IsSpotifyUriPresent();

                if (isSpotifyUriPresent && TokenHelper.HasTokens())
                {
                    topBarButtonSeparator.Visibility = Visibility.Visible;
                    openLinkFromClipboard.Visibility = Visibility.Visible;
                }
                else
                {
                    topBarButtonSeparator.Visibility = Visibility.Collapsed;
                    openLinkFromClipboard.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                logger.Error("CheckForSpotifyUriInClipboard failed: " + ex.ToString());
            }
        }

        private void InitTitleBar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            Window.Current.SetTitleBar(topBarBackground);

            UpdateTitleBarSize();

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;

            if (LocalConfiguration.Theme == Theme.Dark)
            {
                // Top bar buttons background color
                topBarButtonsAcrylicBrush.TintColor = Colors.Black;
                topBarButtonsAcrylicBrush.FallbackColor = Colors.Black;

                // Top bar extra buttons style
                openLinkFromClipboard.Style = Application.Current.Resources["TopBarDarkButtonStyle"] as Style;

                // Set active window colors
                titleBar.ForegroundColor = Colors.White;
                titleBar.BackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 100, 100, 100);
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 140, 140, 140);

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Color.FromArgb(255, 200, 200, 200);
                titleBar.InactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 200, 200, 200);
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else
            {
                // Top bar buttons background color
                topBarButtonsAcrylicBrush.TintColor = Colors.White;
                topBarButtonsAcrylicBrush.FallbackColor = Colors.White;

                // Top bar extra buttons style
                openLinkFromClipboard.Style = Application.Current.Resources["TopBarLightButtonStyle"] as Style;

                // Set active window colors
                titleBar.ForegroundColor = Colors.Black;
                titleBar.BackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 200, 200, 200);
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 160, 160, 160);

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Color.FromArgb(255, 255 - 200, 255 - 200, 255 - 200);
                titleBar.InactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 255 - 200, 255 - 200, 255 - 200);
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarSize();
        }

        private void UpdateTitleBarSize()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            ViewModel.TopBarButtonHeight = coreTitleBar.Height;
            ViewModel.TopBarButtonWidth = coreTitleBar.SystemOverlayRightInset / 4.0;
        }

        private async void SetInitialPlaybackState()
        {
            // Restore initial playback state
            // (We removed the localStorage entry, because of PWA's bug with Edge. See clearPlaybackLocalStorage.js for more info)

            if (initialPlaybackState == null)
                return;

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3));

                var player = new Player();

                SpotifyApi.Model.Device thisDevice = null;

                for (int i = 0; i < 10; i++)
                {
                    var devices = await player.GetDevices();
                    thisDevice = devices.devices.FirstOrDefault(x => x.name.Contains("Edge") && x.name.Contains("Web"));

                    if (thisDevice != null)
                        break;
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }

                if (thisDevice != null)
                {
                    await player.SetVolume(thisDevice.id, initialPlaybackState.volume);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("SetInitialPlaybackState failed: " + ex.ToString());
            }
        }

        private async void PlayCheckTimer_Tick(object sender, object e)
        {
            // Ignore if not logged in
            if (!TokenHelper.HasTokens())
                return;

            try
            {
                var currentPlaying = await WebViewHelper.GetCurrentPlaying();
                if (currentPlaying != prevCurrentPlaying)
                {
                    prevCurrentPlaying = currentPlaying;
                    logger.Info($"CurrentPlaying text extracted from web page changed to '{currentPlaying}'.");

                    await PlayStatusTracker.RefreshPlayStatus();
                }
            }
            catch (Exception ex)
            {
                logger.Warn("checkCurrentPlaying failed: " + ex.ToString());
            }
        }

        private void ClipboardCheckTimer_Tick(object sender, object e)
        {
            // Ignore if not logged in
            if (!TokenHelper.HasTokens())
                return;

            CheckForSpotifyUriInClipboard();
        }

        private async void StuckDetectTimer_Tick(object sender, object e)
        {
            // Ignore if not logged in
            if (!TokenHelper.HasTokens())
                return;

            try
            {                 
                var currentPlayTime = await WebViewHelper.GetCurrentSongPlayTime();

                if (currentPlayTime == "0:00" 
                    && PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds > 5000
                    && PlayStatusTracker.LastPlayStatus.IsPlaying)
                {
                    if (stuckDetectCounter < 2)
                    {
                        stuckDetectCounter++;
                    }
                    else
                    {
                        stuckDetectCounter = 0;
                        logger.Warn("Playback seems to have stuck.");

                        var result = false;

                        if ((DateTime.UtcNow - lastStuckFixApiCall) > TimeSpan.FromMinutes(1))
                        {
                            lastStuckFixApiCall = DateTime.UtcNow;

                            var player = new Player();
                            result = await player.PreviousTrack();
                        }

                        if (result)
                        {
                            AnalyticsHelper.Log("playbackStuck", "1");
                            ToastHelper.SendDebugToast("PlaybackStuck1", "PrevTrack issued.");
                            logger.Info("playbackStuck1");
                        }
                        else
                        {
                            await WebViewHelper.NextTrack();
                            await Task.Delay(1500);
                            await WebViewHelper.PreviousTrack();

                            AnalyticsHelper.Log("playbackStuck", "2");
                            ToastHelper.SendDebugToast("PlaybackStuck2", "NextAndPrevTrack issued.");
                            logger.Info("playbackStuck2");
                        }
                    }
                }
                else
                {
                    stuckDetectCounter = 0;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("checkCurrentSongPlayTime failed: " + ex.ToString());
            }
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
                            if (await PlaybackActionHelper.Play())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;

                            break;
                        case SystemMediaTransportControlsButton.Pause:
                            if (await PlaybackActionHelper.Pause())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;

                            break;
                        case SystemMediaTransportControlsButton.Stop:
                            if (await PlaybackActionHelper.Pause())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;

                            break;
                        case SystemMediaTransportControlsButton.Next:
                            if (await PlaybackActionHelper.NextTrack())
                                compactOverlayView?.PlayChangeTrackAnimation(reverse: false);

                            break;
                        case SystemMediaTransportControlsButton.Previous:
                            if (await PlaybackActionHelper.PreviousTrack())
                                compactOverlayView?.PlayChangeTrackAnimation(reverse: true);

                            // Necessary for progress bar update, in case 'previous' command goes to 
                            // the beginning of the same track.
                            await Task.Delay(500);
                            await PlayStatusTracker.RefreshPlayStatus(); 

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
            if (e.Uri.ToString().StartsWith(Authorization.SpotifyLoginUri) && LocalConfiguration.IsLoggedInByFacebook)
            {
                if (await WebViewHelper.TryPushingFacebookLoginButton())
                {
                    logger.Info("Pushed the facebook login button.");
                    return;
                }
            }

            if (e.Uri.ToString().StartsWith("https://open.spotify.com/static/offline.html?redirectUrl="))
            {
                var url = e.Uri.ToString();

                logger.Info("Clearing local storage and redirecting...");
                var result = await WebViewHelper.ClearPlaybackLocalStorage();

                try
                {
                    if (result.Length > 0)
                    {
                        initialPlaybackState = JsonConvert.DeserializeObject<LocalStoragePlayback>(result);
                        logger.Info("initial playback volume = " + initialPlaybackState.volume);
                    }
                    else
                    {
                        logger.Info("localStorage.playback was undefined.");
                    }
                }
                catch
                {
                    logger.Warn("Decoding localStorage.playback failed.");
                    logger.Info("localStorage.playback content was: " + result);
                }

                var urlDecoder = new WwwFormUrlDecoder(url.Substring(url.IndexOf('?') + 1));
                WebViewHelper.Navigate(new Uri(urlDecoder.GetFirstValueByName("redirectUrl")));

                return;
            }

            var currentStateName = VisualStateManager.GetVisualStateGroups(mainGrid).FirstOrDefault().CurrentState.Name;
            if (currentStateName == "SplashScreen" || currentStateName == "LoadFailedScreen") {

                if (e.Uri.ToString().ToLower().Contains(WebViewHelper.SpotifyPwaUrlBeginsWith.ToLower()))
                    VisualStateManager.GoToState(this, "MainScreen", false);
                else
                    VisualStateManager.GoToState(this, "MainScreenQuick", false);

                if (shouldShowWhatsNew)
                {
                    OpenWhatsNew();
                }
            }

            if (e.Uri.ToString().StartsWith(Authorization.RedirectUri))
            {
                FinalizeAuthorization(e.Uri.ToString());
            }

            if (e.Uri.ToString().ToLower().Contains(WebViewHelper.SpotifyPwaUrlBeginsWith.ToLower()))
            {
                await WebViewHelper.InjectInitScript();
                if (LocalConfiguration.Theme == Theme.Dark)
                    mainWebViewInvertFilter.Visibility = Visibility.Visible;
                else
                    await WebViewHelper.InjectLightThemeScript();
                SetInitialPlaybackState();
            }

            if (!await WebViewHelper.CheckLoggedIn())
            {
                Authorize("https://accounts.spotify.com/login?continue=https%3A%2F%2Fopen.spotify.com%2F", clearExisting: true);
                AnalyticsHelper.Log("mainEvent", "notLoggedIn");
            }
        }

        private async void MainWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs e)
        {
            logger.Info("Page: " + e.Uri.ToString());

            if (e.Uri.ToString().EndsWith("#xpotifygoback"))
            {
                e.Cancel = true;
                isWebViewGoingBack = true;

                await WebViewHelper.GoBack();
            }
            else if (e.Uri.ToString().EndsWith("#xpotifysettings"))
            {
                e.Cancel = true;
                OpenSettings();
            }
            else if (e.Uri.ToString().EndsWith("#xpotifypintostart"))
            {
                e.Cancel = true;

                await PinPageToStart();
                AnalyticsHelper.Log("mainEvent", "pinToStart");
            }
            else if (e.Uri.ToString().EndsWith("#xpotifycompactoverlay"))
            {
                e.Cancel = true;

                await GoToCompactOverlayMode();
                AnalyticsHelper.Log("mainEvent", "compactOverlayOpened");
            }
            else if (e.Uri.ToString().EndsWith("#xpotifyInitialPage"))
            {
            }
            else if (!isWebViewGoingBack)
            {
                if (!webViewPreviousUri.ToLower().StartsWith(WebViewHelper.SpotifyPwaUrlBeginsWith.ToLower())
                    || !e.Uri.ToString().ToLower().StartsWith(WebViewHelper.SpotifyPwaUrlBeginsWith.ToLower()))
                {
                    // Open splash screen, unless #xpotifygoback is happening or
                    // both new and old uris are in open.spotify.com itself.
                    VisualStateManager.GoToState(this, "SplashScreen", false);
                }
            }

            if (!e.Uri.ToString().ToLower().StartsWith(WebViewHelper.SpotifyPwaUrlBeginsWith.ToLower()))
                mainWebViewInvertFilter.Visibility = Visibility.Collapsed;

            if (e.Uri.ToString().StartsWith(Authorization.FacebookLoginFinishRedirectUri))
            {
                logger.Info("Logged in by Facebook.");
                LocalConfiguration.IsLoggedInByFacebook = true;
            }

            isWebViewGoingBack = false;
            webViewPreviousUri = e.Uri.ToString();
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

        private void CompactOverlayView_ExitCompactOverlayRequested(object sender, EventArgs e)
        {
            CloseCompactOverlay();
        }

        private async void CloseCompactOverlay()
        {
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

            compactOverlayView.ExitCompactOverlayRequested -= CompactOverlayView_ExitCompactOverlayRequested;
            mainGrid.Children.Remove(compactOverlayView);

            compactOverlayView.PrepareToExit();
            compactOverlayView = null;

            AnalyticsHelper.Log("mainEvent", "compactOverlayClosed");
        }

        private async Task PinPageToStart()
        {
            VisualStateManager.GoToState(this, "MainScreenWaiting", false);

            var pageUrl = await WebViewHelper.GetPageUrl();
            var pageTitle = await WebViewHelper.GetPageTitle();

            await TileHelper.PinPageToStart(pageUrl, pageTitle);

            VisualStateManager.GoToState(this, "MainScreen", false);
        }

        private void MainWebView_FrameNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            logger.Info("Frame: " + args.Uri.ToString());
        }

        private void RetryConnectButton_Click(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "SplashScreen", false);
            WebViewHelper.Navigate(loadFailedUrl);
        }

        private void MainWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(Authorization.RedirectUri))
            {
                FinalizeAuthorization(e.Uri.ToString());
                return;
            }

            VisualStateManager.GoToState(this, "LoadFailedScreen", false);
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
                WebViewHelper.Navigate(new Uri(urlDecoder.GetFirstValueByName("state")));
            }
            catch (Exception ex)
            {
                logger.Info("Authorization failed. " + ex.ToString());

                Authorize("https://open.spotify.com/", clearExisting: false);
            }
        }

        private void OpenSettings()
        {
            settingsFlyout.InitFlyout();
            settingsFlyout.Visibility = Visibility.Visible;
            VisualStateManager.GoToState(this, "OverlayScreen", false);
        }

        private async void OpenWhatsNew()
        {
            await Task.Delay(700);

            whatsNewFlyout.InitFlyout();
            whatsNewFlyout.Visibility = Visibility.Visible;
            VisualStateManager.GoToState(this, "OverlayScreen", false);
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
            VisualStateManager.GoToState(this, "MainScreen", false);

            await Task.Delay(300);

            settingsFlyout.Visibility = Visibility.Collapsed;
            whatsNewFlyout.Visibility = Visibility.Collapsed;
        }

        private async void OpenLinkFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            var uri = await ClipboardHelper.GetSpotifyUri();

            if (!string.IsNullOrWhiteSpace(uri))
            {
                await WebViewHelper.NavigateToSpotifyUrl(uri);
            }
        }

        private void LoadFailedProxySettingsLink_Click(object sender, RoutedEventArgs e)
        {
            loadFailedProxySettingsLink.Visibility = Visibility.Collapsed;
            loadFailedProxySettings.Visibility = Visibility.Visible;
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
