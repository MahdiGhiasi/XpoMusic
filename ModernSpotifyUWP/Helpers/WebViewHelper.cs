using ModernSpotifyUWP.Classes.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace ModernSpotifyUWP.Helpers
{
    public static class WebViewHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public const string SpotifyPwaUrlBeginsWith = "https://open.spotify.com";

        private static WebView mainWebView;

        public static void Init(WebView webView)
        {
            mainWebView = webView;
        }

        public static void Navigate(Uri targetUri)
        {
            // TODO: Read language preference from configuration
            Navigate(targetUri, Language.Default); 
        }

        public static void Navigate(Uri targetUri, Language language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, targetUri);

            var languageString = LanguageHelper.GetHeaderLanguageString(language);
            if (!string.IsNullOrWhiteSpace(languageString))
                request.Headers.Add("Accept-Language", languageString);

            mainWebView.NavigateWithHttpRequestMessage(request);
        }

        public static async Task InjectInitScript()
        {
            var checkIfInjected = "((document.getElementsByTagName('body')[0].getAttribute('data-scriptinjection') == null) ? '0' : '1');";
            var injected = await mainWebView.InvokeScriptAsync("eval", new string[] { checkIfInjected });

            if (injected != "1")
            {
                var script = File.ReadAllText("InjectedAssets/initScript.js");
                await mainWebView.InvokeScriptAsync("eval", new string[] { script });
            }
        }

        public static async Task<bool> CheckLoggedIn()
        {
            var script = File.ReadAllText("InjectedAssets/isLoggedInCheck.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result != "0");
        }

        public static async Task<bool> TryPushingFacebookLoginButton()
        {
            var script = File.ReadAllText("InjectedAssets/clickOnFacebookLogin.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task GoBack()
        {
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

        public static async Task<string> GetPageUrl()
        {
            return await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
        }

        public static async Task<string> GetPageTitle()
        {
            var findPageTitleScript = File.ReadAllText("InjectedAssets/findPageTitle.js");
            var pageTitle = await mainWebView.InvokeScriptAsync("eval", new string[] { findPageTitleScript });

            return pageTitle;
        }

        public static async Task NavigateToSpotifyUrl(string url)
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
                Navigate(new Uri(url));
            }
        }

        public static async Task<string> GetCurrentPlaying()
        {
            var script = File.ReadAllText("InjectedAssets/checkCurrentPlaying.js");
            var currentPlaying = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return currentPlaying;
        }

        public static async Task<string> GetCurrentSongPlayTime()
        {
            var script = File.ReadAllText("InjectedAssets/checkCurrentSongPlayTime.js");
            var currentPlayTime = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return currentPlayTime;
        }

        public static async Task<string> ClearPlaybackLocalStorage()
        {
            var script = File.ReadAllText("InjectedAssets/clearPlaybackLocalStorage.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return result;
        }

        public static async Task<bool> Play()
        {
            var script = File.ReadAllText("InjectedAssets/actionPlay.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task<bool> Pause()
        {
            var script = File.ReadAllText("InjectedAssets/actionPause.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task<bool> NextTrack()
        {
            var script = File.ReadAllText("InjectedAssets/actionNextTrack.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task<bool> PreviousTrack()
        {
            var script = File.ReadAllText("InjectedAssets/actionPrevTrack.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }
    }
}
