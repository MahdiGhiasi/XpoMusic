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
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private const string SpotifyPwaUrlBeginsWith = "https://open.spotify.com";

        MediaPlayer silentMediaPlayer;
        bool splashClosed = false;
        private CompactOverlayView compactOverlayView;
        private Uri loadFailedUrl;
        private DispatcherTimer playCheckTimer, stuckDetectTimer;
        private string prevCurrentPlaying;
        private LocalStoragePlayback initialPlaybackState = null;
        private bool stuckDetectSecondChance = false;

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
                    mainWebView.Navigate(new Uri(loginUrl));
                }
                else
                {
                    mainWebView.Navigate(new Uri(targetUrl));
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
                TokenHelper.ClearTokens();
                LocalConfiguration.IsLoggedInByFacebook = false;
            }

            var authorizationUrl = Authorization.GetAuthorizationUrl(targetUrl);
            mainWebView.Navigate(new Uri(authorizationUrl));
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

                await NavigateToSpotifyUrl(pageUrl);

                return;
            }
            catch (Exception ex)
            {
                logger.Info($"Parsing input parameter {parameter} failed. {ex}");
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

            stuckDetectTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5),
            };
            stuckDetectTimer.Tick += StuckDetectTimer_Tick;
            stuckDetectTimer.Start();

            AnalyticsHelper.PageView("MainPage");
            AnalyticsHelper.Log("appOpened", SystemInformation.OperatingSystemVersion.ToString());
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
            try
            {
                var script = File.ReadAllText("InjectedAssets/checkCurrentPlaying.js");
                var currentPlaying = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

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

        private async void StuckDetectTimer_Tick(object sender, object e)
        {
            try
            {
                var script = File.ReadAllText("InjectedAssets/checkCurrentSongPlayTime.js");
                var currentPlayTime = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

                if (currentPlayTime == "0:00" 
                    && PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds > 5000
                    && PlayStatusTracker.LastPlayStatus.IsPlaying)
                {
                    if (stuckDetectSecondChance == false)
                    {
                        stuckDetectSecondChance = true;
                    }
                    else
                    {
                        stuckDetectSecondChance = false;
                        logger.Warn("Playback seems to have stuck. Will issue a Previous Track command.");

                        var player = new Player();
                        await player.PreviousTrack();

                        AnalyticsHelper.Log("playbackStuck1");

                        ToastHelper.SendDebugToast("PlaybackStuck1", "PrevTrack issued.");
                    }
                }
                else
                {
                    stuckDetectSecondChance = false;
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
                            if (await (new Player()).NextTrack())
                                compactOverlayView?.PlayChangeTrackAnimation(reverse: false);

                            break;
                        case SystemMediaTransportControlsButton.Previous:
                            if (await (new Player()).PreviousTrack())
                                compactOverlayView?.PlayChangeTrackAnimation(reverse: true);

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
                if (await TryPushingFacebookLoginButton())
                {
                    logger.Info("Pushed the facebook login button.");
                    return;
                }
            }

            if (e.Uri.ToString().StartsWith("https://open.spotify.com/static/offline.html?redirectUrl="))
            {
                var url = e.Uri.ToString();

                logger.Info("Clearing local storage and redirecting...");
                var script = File.ReadAllText("InjectedAssets/clearPlaybackLocalStorage.js");
                var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

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
                mainWebView.Navigate(new Uri(urlDecoder.GetFirstValueByName("redirectUrl")));

                return;
            }

            var currentStateName = VisualStateManager.GetVisualStateGroups(mainGrid).FirstOrDefault().CurrentState.Name;
            if (currentStateName == "SplashScreen" || currentStateName == "LoadFailedScreen")
                VisualStateManager.GoToState(this, "MainScreen", false);

            if (e.Uri.ToString().StartsWith(Authorization.RedirectUri))
            {
                FinalizeAuthorization(e.Uri.ToString());
            }

            if (e.Uri.ToString().ToLower().Contains(SpotifyPwaUrlBeginsWith.ToLower()))
            {
                await InjectInitScript();
                SetInitialPlaybackState();
            }

            if (!await CheckLoggedIn())
            {
                Authorize("https://accounts.spotify.com/login?continue=https%3A%2F%2Fopen.spotify.com%2F", clearExisting: true);
                AnalyticsHelper.Log("notLoggedIn");
            }
        }

        private async Task<bool> TryPushingFacebookLoginButton()
        {
            var script = File.ReadAllText("InjectedAssets/clickOnFacebookLogin.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        private async Task<bool> CheckLoggedIn()
        {
            var script = File.ReadAllText("InjectedAssets/isLoggedInCheck.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result != "0");
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

        private async void MainWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs e)
        {
            logger.Info("Page: " + e.Uri.ToString());

            if (e.Uri.ToString().StartsWith("https://open.spotify.com/static/offline.html?redirectUrl="))
            {
                VisualStateManager.GoToState(this, "SplashScreen", false);
            }


            if (e.Uri.ToString().EndsWith("#xpotifygoback"))
            {
                e.Cancel = true;

                //var len = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.history.length.toString()" });
                //logger.Info("Len: " + len);
                //var curLocation = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
                //logger.Info("curLocation: " + curLocation);

                await mainWebView.InvokeScriptAsync("eval", new string[] { "window.history.go(-1);" });

                //var len2 = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.history.length.toString()" });
                //logger.Info("Len2: " + len2);
                //var curLocation2 = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
                //logger.Info("curLocation2: " + curLocation2);
            }
            else if (e.Uri.ToString().EndsWith("#xpotifysettings"))
            {
                e.Cancel = true;
                OpenSettings();
                AnalyticsHelper.Log("settingsOpened");
            }
            else if (e.Uri.ToString().EndsWith("#xpotifypintostart"))
            {
                e.Cancel = true;

                await PinPageToStart();
                AnalyticsHelper.Log("pinToStart");
            }
            else if (e.Uri.ToString().EndsWith("#xpotifycompactoverlay"))
            {
                e.Cancel = true;

                await GoToCompactOverlayMode();
                AnalyticsHelper.Log("compactOverlayOpened");
            }


            if (e.Uri.ToString().StartsWith(Authorization.FacebookLoginFinishRedirectUri))
            {
                logger.Info("Logged in by Facebook.");
                LocalConfiguration.IsLoggedInByFacebook = true;
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

            AnalyticsHelper.Log("compactOverlayClosed");
        }

        private async Task PinPageToStart()
        {
            VisualStateManager.GoToState(this, "MainScreenWaiting", false);

            var findPageTitleScript = File.ReadAllText("InjectedAssets/findPageTitle.js");
            var pageUrl = await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
            var pageTitle = await mainWebView.InvokeScriptAsync("eval", new string[] { findPageTitleScript });

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
            mainWebView.Navigate(loadFailedUrl);
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
                mainWebView.Navigate(new Uri(urlDecoder.GetFirstValueByName("state")));
            }
            catch (Exception ex)
            {
                logger.Info("Authorization failed. " + ex.ToString());

                Authorize("https://open.spotify.com/", clearExisting: false);
            }
        }

        private void OpenSettings()
        {
            settingsFlyout.Visibility = Visibility.Visible;
            VisualStateManager.GoToState(this, "OverlayScreen", false);
        }

        private void OpenWhatsNew()
        {
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
