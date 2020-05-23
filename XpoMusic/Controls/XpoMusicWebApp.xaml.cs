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
using System.Threading.Tasks;
using XpoMusic.Classes;
using XpoMusic.Helpers;
using XpoMusic.Classes.Model;
using XpoMusic.WebAgent.Model;
using XpoMusic.SpotifyApi;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace XpoMusic.Controls
{
    public sealed partial class XpoMusicWebApp : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public enum XpoMusicWebAppActionRequest
        {
            OpenSettingsFlyout,
            OpenAboutFlyout,
            OpenDonateFlyout,
            GoToCompactOverlay,
            GoToNowPlaying,
        }

        public event EventHandler<EventArgs> PageLoadBegin;
        public event EventHandler<EventArgs> PageLoadFinished;
        public event EventHandler<EventArgs> WebAppLoaded;
        public event EventHandler<XpoMusicWebAppActionRequest> ActionRequested;

        public bool BackEnabled { get; private set; } = false;
        public WebViewController Controller { get; }


        #region Custom Properties
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof(bool), typeof(XpoMusicWebApp), new PropertyMetadata(defaultValue: false,
                propertyChangedCallback: new PropertyChangedCallback(OnIsOpenPropertyChanged)));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set
            {
                if (IsOpen != value)
                    SetValue(IsOpenProperty, value);

                // Currently WebView2 Visibility does not work, so we set its width to 0 instead when we want to hide it.

                //this.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                //mainWebView.Visibility = this.Visibility;
                if (value)
                {
                    mainWebView.HorizontalAlignment = HorizontalAlignment.Stretch;
                    mainWebView.Width = double.NaN;
                }
                else
                {
                    mainWebView.HorizontalAlignment = HorizontalAlignment.Left;
                    mainWebView.Width = 0;
                }

                // TODO: Hide html content from DOM when possible via javascript to reduce CPU usage
            }
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as XpoMusicWebApp).IsOpen = (bool)e.NewValue;
        }
        #endregion


        public XpoMusicWebApp()
        {
            this.InitializeComponent();
            //mainWebView.Visibility = Visibility.Collapsed;

            Controller = new WebViewController(this.mainWebView);
            Controller.WebAgent.StatusReportReceived += WebAgent_StatusReportReceived;
            Controller.WebAgent.ActionRequested += WebAgent_ActionRequested;
            Controller.WebAgent.InitializationFailed += WebAgent_InitializationFailed;
            Controller.WebAgent.NewAccessTokenRequested += WebAgent_NewAccessTokenRequested;
            Controller.WebAgent.LogMessageReceived += WebAgent_LogMessageReceived;
            PlaybackActionHelper.SetController(Controller);

            OpenWebApp(WebViewController.SpotifyPwaUrlHome);
        }


        private void WebAgent_LogMessageReceived(object sender, LogMessageReceivedEventArgs e)
        {
            logger.Info($"js::{e.Message}");
        }

        private async void WebAgent_NewAccessTokenRequested(object sender, object e)
        {
            try
            {
                await TokenHelper.GetAndSaveNewTokenAsync();
                Controller.WebAgent.SetNewAccessToken(TokenHelper.GetTokens().AccessToken);
            }
            catch (Exception ex)
            {
                logger.Error($"Exception in WebAgent_NewAccessTokenRequested: {ex}");
            }
        }

        private void WebAgent_InitializationFailed(object sender, InitFailedEventArgs e)
        {
            Controller.LastInitErrors = e.Errors;
            logger.Warn("WebAgent: InitFailed :: " + e.Errors);
            AnalyticsHelper.Log("webAgentInitFailed", e.Errors);
        }

        private async void WebAgent_ActionRequested(object sender, WebAgent.Model.ActionRequestedEventArgs e)
        {
            switch (e.Action)
            {
                case WebAgent.Model.Action.PinToStart:
                    // await PinPageToStart();
                    break;
                case WebAgent.Model.Action.OpenSettings:
                    ActionRequested?.Invoke(this, XpoMusicWebAppActionRequest.OpenSettingsFlyout);
                    break;
                case WebAgent.Model.Action.OpenDonate:
                    ActionRequested?.Invoke(this, XpoMusicWebAppActionRequest.OpenDonateFlyout);
                    break;
                case WebAgent.Model.Action.OpenAbout:
                    ActionRequested?.Invoke(this, XpoMusicWebAppActionRequest.OpenAboutFlyout);
                    break;
                case WebAgent.Model.Action.OpenMiniView:
                    ActionRequested?.Invoke(this, XpoMusicWebAppActionRequest.GoToCompactOverlay);
                    break;
                case WebAgent.Model.Action.OpenNowPlaying:
                    ActionRequested?.Invoke(this, XpoMusicWebAppActionRequest.GoToNowPlaying);
                    break;
                case WebAgent.Model.Action.NavigateToClipboardUri:
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

        private void WebAgent_StatusReportReceived(object sender, StatusReportReceivedEventArgs e)
        {
            logger.Trace("StatusReport Received.");

            BackEnabled = e.Status.BackButtonEnabled;
            PlayStatusTracker.LocalPlaybackDataReceived(e.Status.NowPlaying);
            StuckResolveHelper.StatusReportReceived(e.Status.NowPlaying, Controller);
        }

        public void OpenWebApp(string targetUrl)
        {
            if (TokenHelper.HasTokens())
            {
                if (LocalConfiguration.ApiTokenVersion < 2)
                {
                    AuthorizationHelper.Authorize(Controller, clearExisting: false);
                }
                else
                {
                    if (LocalConfiguration.IsLoggedInByFacebook)
                    {
                        Controller.Navigate(new Uri(targetUrl));
                        // TODO

                        // We need to open the login page and click on facebook button
                        //logger.Info("Logging in via Facebook...");
                        //var loginUrl = "https://accounts.spotify.com/login?continue=" + System.Net.WebUtility.UrlEncode(targetUrl);
                        //Controller.Navigate(new Uri(loginUrl));
                    }
                    else
                    {
                        Controller.Navigate(new Uri(targetUrl));
                    }
                }
            }
            else
            {
                AuthorizationHelper.Authorize(Controller, clearExisting: false);
            }
        }

        private async void mainWebView_NavigationCompleted(WebView2 sender, WebView2NavigationCompletedEventArgs e)
        {
            logger.Info($"XpoMusicWebApp.MainWebView: NavigationCompleted: IsSuccess={e.IsSuccess}, WebErrorStatus={e.WebErrorStatus}, Url={sender.Source}");

            if (sender.Source.ToString().ToLower().Contains(WebViewController.SpotifyPwaUrlHome.ToLower()))
            {
                var justInjected = await Controller.InjectInitScript(ThemeHelper.GetCurrentTheme() == Theme.Light);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    if (justInjected)
                    {
                        WebAppLoaded?.Invoke(this, null);
                        // SetInitialPlaybackState();
                        PlayStatusTracker.StartRegularRefresh();
                        // SetFocusToWebView();
                        // windowTitle.Text = PackageHelper.GetAppNameString();
                    }

                    //if (AutoPlayAction != AutoPlayAction.None)
                    //{
                    //    AutoPlayOnStartup(AutoPlayAction);
                    //    AutoPlayAction = AutoPlayAction.None;
                    //}
                });

                if (!await Controller.CheckLoggedIn())
                {
                    AuthorizationHelper.Authorize(Controller, clearExisting: true);
                    AnalyticsHelper.Log("mainEvent", "notLoggedIn");
                }
            }
            else
            {
                PageLoadFinished?.Invoke(this, null);
            }
        }

        private void mainWebView_NavigationStarting(WebView2 sender, WebView2NavigationStartingEventArgs e)
        {
            logger.Info($"XpoMusicWebApp.MainWebView: NavigationStarting to {e.Uri}");

            PageLoadBegin?.Invoke(this, null);
            
            if (e.Uri.ToString().StartsWith(Authorization.RedirectUri))
            {
                AuthorizationHelper.FinalizeAuthorization(Controller, e.Uri.ToString());
                return;
            }
        }
    }
}
