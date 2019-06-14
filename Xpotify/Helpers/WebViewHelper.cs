using Xpotify.Classes;
using Xpotify.Classes.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Xpotify.Helpers
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
            Navigate(targetUri, LocalConfiguration.Language); 
        }

        private static void Navigate(Uri targetUri, Language language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, targetUri);

            var languageString = LanguageHelper.GetHeaderLanguageString(language);
            if (!string.IsNullOrWhiteSpace(languageString))
                request.Headers.Add("Accept-Language", languageString);

            mainWebView.NavigateWithHttpRequestMessage(request);
        }

        /// <returns>true if just injected the script, false if it was already injected.</returns>
        public static async Task<bool> InjectInitScript(bool lightTheme)
        {
            var checkIfInjected = "((document.getElementsByTagName('body')[0].getAttribute('data-scriptinjection') == null) ? '0' : '1');";
            var injected = await mainWebView.InvokeScriptAsync("eval", new string[] { checkIfInjected });

            if (injected != "1")
            {
                var script = await AssetManager.LoadAssetString(lightTheme ? "init-light.js" : "init-dark.js");
                var styleCss = await AssetManager.LoadAssetString(lightTheme ? "style-light.min.css" : "style-dark.min.css");
                script = script
                    .Replace("{{XPOTIFYCSSBASE64CONTENT}}", Convert.ToBase64String(Encoding.UTF8.GetBytes(styleCss)))
                    .Replace("{{XPOTIFYISPROVERSION}}", PackageHelper.IsProVersion ? "1" : "0");

                await mainWebView.InvokeScriptAsync("eval", new string[] { script });

                return true;
            }

            return false;
        }

        public static async Task<bool> CheckLoggedIn()
        {
            var script = await AssetManager.LoadAssetString("isLoggedInCheck.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result != "0");
        }

        public static async Task<bool> TryPushingFacebookLoginButton()
        {
            var script = await AssetManager.LoadAssetString("clickOnFacebookLogin.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task<bool> GoBackIfPossible()
        {
            var script = await AssetManager.LoadAssetString("goBackIfPossible.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task<bool> IsBackPossible()
        {
            var script = await AssetManager.LoadAssetString("isBackPossible.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static void ClearCookies()
        {
            // WebView.ClearTemporaryWebDataAsync() causes cookies (that will be added later) to not 
            // be saved upon app exit during current session. It seems to be a bug. 
            // So we can't use that to clear cookies.

            ClearCookies(new Uri("https://www.spotify.com"));
            ClearCookies(new Uri("https://spotify.com"));
            ClearCookies(new Uri("https://accounts.spotify.com"));
            ClearCookies(new Uri("https://open.spotify.com"));
            ClearCookies(new Uri("https://facebook.com"));
            ClearCookies(new Uri("https://www.facebook.com"));
        }

        private static void ClearCookies(Uri uri)
        {
            HttpBaseProtocolFilter baseFilter = new HttpBaseProtocolFilter();
            foreach (var cookie in baseFilter.CookieManager.GetCookies(uri))
            {
                baseFilter.CookieManager.DeleteCookie(cookie);
            }
        }

        public static async Task<string> GetPageUrl()
        {
            return await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
        }

        public static async Task<string> GetPageTitle()
        {
            var findPageTitleScript = await AssetManager.LoadAssetString("findPageTitle.js");
            var pageTitle = await mainWebView.InvokeScriptAsync("eval", new string[] { findPageTitleScript });

            return pageTitle;
        }

        public static async Task NavigateToSpotifyUrl(string url)
        {
            var currentUrl = await mainWebView.InvokeScriptAsync("eval", new String[] { "document.location.href;" });

            if (currentUrl.ToLower().StartsWith(SpotifyPwaUrlBeginsWith.ToLower()))
            {
                var script = await AssetManager.LoadAssetString("navigateToPage.js")
                    + $"navigateToPage('{url.Replace("'", "\\'")}');";

                await mainWebView.InvokeScriptAsync("eval", new string[] { script });
            }
            else
            {
                Navigate(new Uri(url));
            }
        }

        internal static async Task AutoPlay(AutoPlayAction action)
        {
            var script = await AssetManager.LoadAssetString(action == AutoPlayAction.Track ? "autoplayTrack.js" : "autoplayPlaylist.js");
            var currentPlaying = await mainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        public static async Task<string> GetCurrentPlaying()
        {
            var script = await AssetManager.LoadAssetString("checkCurrentPlaying.js");
            var currentPlaying = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return currentPlaying;
        }

        public static async Task<string> GetCurrentSongPlayTime()
        {
            var script = await AssetManager.LoadAssetString("checkCurrentSongPlayTime.js");
            var currentPlayTime = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return currentPlayTime;
        }

        public static async Task<string> ClearPlaybackLocalStorage()
        {
            var script = await AssetManager.LoadAssetString("clearPlaybackLocalStorage.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return result;
        }

        public static async Task<bool> Play()
        {
            var script = await AssetManager.LoadAssetString("actionPlay.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task<bool> Pause()
        {
            var script = await AssetManager.LoadAssetString("actionPause.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task<bool> NextTrack()
        {
            var script = await AssetManager.LoadAssetString("actionNextTrack.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public static async Task<bool> PreviousTrack()
        {
            var script = await AssetManager.LoadAssetString("actionPrevTrack.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        internal static async Task<bool> IsPlayingOnThisApp()
        {
            var script = await AssetManager.LoadAssetString("isPlayingOnThisApp.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }
    }
}
