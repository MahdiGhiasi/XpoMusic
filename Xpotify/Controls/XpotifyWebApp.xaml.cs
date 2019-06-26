using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Xpotify.Classes;
using Xpotify.Classes.Model;
using Xpotify.Helpers;
using Xpotify.SpotifyApi;
using XpotifyWebAgent;
using XpotifyWebAgent.Model;

namespace Xpotify.Controls
{
    public sealed partial class XpotifyWebApp : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum XpotifyWebAppActionRequest
        {
            OpenSettingsFlyout,
            OpenAboutFlyout,
            OpenDonateFlyout,
            GoToCompactOverlay,
            GoToNowPlaying,
            ShowSplashScreen,
        }

        public event EventHandler<EventArgs> PageLoaded;
        public event EventHandler<EventArgs> WebAppLoaded;
        public event EventHandler<XpotifyWebAppActionRequest> ActionRequested;

        public AutoPlayAction AutoPlayAction { get; set; } = AutoPlayAction.None;
        public WebViewController Controller { get; }
        public bool BackEnabled { get; private set; } = false;

        #region Custom Properties
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof(bool), typeof(XpotifyWebApp), new PropertyMetadata(defaultValue: false,
                propertyChangedCallback: new PropertyChangedCallback(OnIsOpenPropertyChanged)));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set
            {
                if (IsOpen != value)
                    SetValue(IsOpenProperty, value);

                this.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                mainWebView.Visibility = this.Visibility;
                
                // TODO: Hide html content from DOM when possible via javascript to reduce CPU usage
            }
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as XpotifyWebApp).IsOpen = (bool)e.NewValue;
        }
        #endregion

        private Uri loadFailedUrl;
        private string webViewPreviousUri = "";
        private LocalStoragePlayback initialPlaybackState;
        private XpotifyWebAgent.XpotifyWebAgent xpotifyWebAgent;

        public XpotifyWebApp()
        {
            this.InitializeComponent();

            Controller = new WebViewController(this.mainWebView);
            PlaybackActionHelper.SetController(Controller);

            loadFailedAppVersionText.Text = PackageHelper.GetAppVersionString();

            xpotifyWebAgent = new XpotifyWebAgent.XpotifyWebAgent();
            xpotifyWebAgent.ProgressBarCommandReceived += XpotifyWebAgent_ProgressBarCommandReceived;
            xpotifyWebAgent.StatusReportReceived += XpotifyWebAgent_StatusReportReceived;
            xpotifyWebAgent.ActionRequested += XpotifyWebAgent_ActionRequested;
            xpotifyWebAgent.InitializationFailed += XpotifyWebAgent_InitializationFailed;

            VisualStateManager.GoToState(this, nameof(DefaultVisualState), false);
        }

        private void XpotifyWebAgent_InitializationFailed(object sender, InitFailedEventArgs e)
        {
            Controller.LastInitErrors = e.Errors;
            logger.Warn("WebAgent: InitFailed :: " + e.Errors);
            AnalyticsHelper.Log("webAgentInitFailed", e.Errors);
        }

        private async void XpotifyWebAgent_ActionRequested(object sender, XpotifyWebAgent.Model.ActionRequestedEventArgs e)
        {
            switch (e.Action)
            {
                case XpotifyWebAgent.Model.Action.PinToStart:
                    await PinPageToStart();
                    break;
                case XpotifyWebAgent.Model.Action.OpenSettings:
                    ActionRequested?.Invoke(this, XpotifyWebAppActionRequest.OpenSettingsFlyout);
                    break;
                case XpotifyWebAgent.Model.Action.OpenDonate:
                    ActionRequested?.Invoke(this, XpotifyWebAppActionRequest.OpenDonateFlyout);
                    break;
                case XpotifyWebAgent.Model.Action.OpenAbout:
                    ActionRequested?.Invoke(this, XpotifyWebAppActionRequest.OpenAboutFlyout);
                    break;
                case XpotifyWebAgent.Model.Action.OpenMiniView:
                    ActionRequested?.Invoke(this, XpotifyWebAppActionRequest.GoToCompactOverlay);
                    break;
                case XpotifyWebAgent.Model.Action.OpenNowPlaying:
                    ActionRequested?.Invoke(this, XpotifyWebAppActionRequest.GoToNowPlaying);
                    break;
                case XpotifyWebAgent.Model.Action.NavigateToClipboardUri:
                    if (await ClipboardHelper.IsSpotifyUriPresent())
                    {
                        var uri = await ClipboardHelper.GetSpotifyUri();
                        await Controller.NavigateToSpotifyUrl(uri);
                    }
                    break;
                default:
                    logger.Warn($"Action {e.Action} is unknown.");
                    break;
            }
        }

        private void XpotifyWebAgent_StatusReportReceived(object sender, StatusReportReceivedEventArgs e)
        {
            logger.Trace("StatusReport Received.");

            BackEnabled = e.Status.BackButtonEnabled;
            PlayStatusTracker.LocalPlaybackDataReceived(e.Status.NowPlaying);
            StuckResolveHelper.StatusReportReceived(e.Status.NowPlaying, Controller);
        }

        private void XpotifyWebAgent_ProgressBarCommandReceived(object sender, ProgressBarCommandEventArgs e)
        {
            if (e.Command == ProgressBarCommand.Show)
            {
                playbackBarProgressBar.Margin = new Thickness(e.Left * mainWebView.ActualWidth, e.Top * mainWebView.ActualHeight, 0, 0);
                playbackBarProgressBar.Width = e.Width * mainWebView.ActualWidth;
                playbackBarProgressBar.Visibility = Visibility.Visible;
            }
            else
            {
                playbackBarProgressBar.Visibility = Visibility.Collapsed;
            }
        }

        private void MainWebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(Authorization.RedirectUri))
            {
                FinalizeAuthorization(e.Uri.ToString());
                return;
            }

            VisualStateManager.GoToState(this, nameof(LoadFailedVisualState), false);
            PageLoaded?.Invoke(this, new EventArgs());
            loadFailedUrlText.Text = e.Uri.ToString();
            loadFailedUrl = e.Uri;
            errorMessageText.Text = e.WebErrorStatus.ToString();
        }

        private async void MainWebView_LoadCompleted(object sender, NavigationEventArgs e)
        {
            VisualStateManager.GoToState(this, nameof(DefaultVisualState), false);

            if (e.Uri.ToString().StartsWith(Authorization.SpotifyLoginUri) && LocalConfiguration.IsLoggedInByFacebook)
            {
                if (await Controller.TryPushingFacebookLoginButton())
                {
                    logger.Info("Pushed the facebook login button.");
                    return;
                }
            }

            if (e.Uri.ToString().StartsWith("https://open.spotify.com/static/offline.html?redirectUrl="))
            {
                var url = e.Uri.ToString();

                logger.Info("Clearing local storage and redirecting...");
                var result = await Controller.ClearPlaybackLocalStorage();

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
                Controller.Navigate(new Uri(urlDecoder.GetFirstValueByName("redirectUrl")));

                return;
            }

            if (e.Uri.ToString().ToLower().Contains(WebViewController.SpotifyPwaUrlBeginsWith.ToLower()))
            {
                var justInjected = await Controller.InjectInitScript(ThemeHelper.GetCurrentTheme() == Theme.Light);
                if (justInjected)
                {
                    SetInitialPlaybackState();
                    PlayStatusTracker.StartRegularRefresh();
                    SetFocusToWebView();
                }

                if (AutoPlayAction != AutoPlayAction.None)
                {
                    AutoPlayOnStartup(AutoPlayAction);
                    AutoPlayAction = AutoPlayAction.None;
                }
            }

            if (e.Uri.ToString().StartsWith(Authorization.RedirectUri))
                FinalizeAuthorization(e.Uri.ToString());
            else if (e.Uri.ToString().ToLower().Contains(WebViewController.SpotifyPwaUrlBeginsWith.ToLower()))
                WebAppLoaded?.Invoke(this, new EventArgs());
            else
                PageLoaded?.Invoke(this, new EventArgs());

            if (!await Controller.CheckLoggedIn())
            {
                Authorize("https://accounts.spotify.com/login?continue=https%3A%2F%2Fopen.spotify.com%2F", clearExisting: true);
                AnalyticsHelper.Log("mainEvent", "notLoggedIn");
            }
        }

        public async void SetFocusToWebView()
        {
            await FocusManager.TryFocusAsync(mainWebView, FocusState.Programmatic);
        }

        private async void AutoPlayOnStartup(AutoPlayAction autoPlayAction)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            logger.Info("AutoPlay " + autoPlayAction);
            await Controller.AutoPlay(autoPlayAction);
        }

        private async void MainWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs e)
        {
            logger.Info("Page: " + e.Uri.ToString());

            if (e.Uri.ToString().EndsWith("#xpotifyInitialPage"))
            {
            }
            else if (e.Uri.ToString().ToLower().Contains(WebViewController.SpotifyPwaUrlBeginsWith.ToLower()))
            {
                mainWebView.AddWebAllowedObject("Xpotify", xpotifyWebAgent);
            }
            else
            {
                if (!webViewPreviousUri.ToLower().StartsWith(WebViewController.SpotifyPwaUrlBeginsWith.ToLower())
                    || !e.Uri.ToString().ToLower().StartsWith(WebViewController.SpotifyPwaUrlBeginsWith.ToLower()))
                {
                    // Open splash screen, unless both new and old uris are in open.spotify.com itself.
                    ActionRequested?.Invoke(this, XpotifyWebAppActionRequest.ShowSplashScreen);
                }
            }

            if (e.Uri.ToString().StartsWith(Authorization.FacebookLoginFinishRedirectUri))
            {
                logger.Info("Logged in by Facebook.");
                LocalConfiguration.IsLoggedInByFacebook = true;
            }

            webViewPreviousUri = e.Uri.ToString();
        }

        private async Task PinPageToStart()
        {
            VisualStateManager.GoToState(this, nameof(WaitingVisualState), false);

            var pageUrl = await Controller.GetPageUrl();
            var pageTitle = await Controller.GetPageTitle();

            if (await TileHelper.PinPageToStart(pageUrl, pageTitle))
                AnalyticsHelper.Log("mainEvent", "pinToStart");

            VisualStateManager.GoToState(this, nameof(DefaultVisualState), false);
        }

        private void RetryConnectButton_Click(object sender, RoutedEventArgs e)
        {
            ActionRequested?.Invoke(this, XpotifyWebAppActionRequest.ShowSplashScreen);
            Controller.Navigate(loadFailedUrl);
        }

        private async void FinalizeAuthorization(string url)
        {
            try
            {
                var urlDecoder = new WwwFormUrlDecoder(url.Substring(url.IndexOf('?') + 1));
                await Authorization.RetrieveAndSaveTokensFromAuthCode(urlDecoder.GetFirstValueByName("code"));
                Controller.Navigate(new Uri(urlDecoder.GetFirstValueByName("state")));
            }
            catch (Exception ex)
            {
                logger.Info("Authorization failed. " + ex.ToString());

                Authorize("https://open.spotify.com/", clearExisting: false);
            }
        }

        public void Authorize(string targetUrl, bool clearExisting)
        {
            if (clearExisting)
            {
                Controller.ClearCookies();
                TokenHelper.ClearTokens();
                LocalConfiguration.IsLoggedInByFacebook = false;
            }

            var authorizationUrl = Authorization.GetAuthorizationUrl(targetUrl);
            Controller.Navigate(new Uri(authorizationUrl));
        }

        private void LoadFailedProxySettingsLink_Click(object sender, RoutedEventArgs e)
        {
            loadFailedProxySettingsLink.Visibility = Visibility.Collapsed;
            loadFailedProxySettings.Visibility = Visibility.Visible;
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
    }
}
