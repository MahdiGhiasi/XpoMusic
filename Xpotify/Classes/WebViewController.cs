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
using Newtonsoft.Json;
using Windows.UI.Popups;
using Xpotify.SpotifyApi;

namespace Xpotify.Helpers
{
    public class WebViewController
    {
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public const string SpotifyPwaUrlBeginsWith = "https://open.spotify.com";

        public string LastInitErrors { get; internal set; } = "";

        private WebView mainWebView;

        public WebViewController(WebView webView)
        {
            mainWebView = webView;
        }

        public void Navigate(Uri targetUri)
        {
            Navigate(targetUri, LocalConfiguration.Language); 
        }

        private void Navigate(Uri targetUri, Language language)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, targetUri);

            var languageString = LanguageHelper.GetHeaderLanguageString(language);
            if (!string.IsNullOrWhiteSpace(languageString))
                request.Headers.Add("Accept-Language", languageString);

            mainWebView.NavigateWithHttpRequestMessage(request);
        }

        /// <returns>true if just injected the script, false if it was already injected.</returns>
        public async Task<bool> InjectInitScript(bool lightTheme)
        {
            var checkIfInjected = "((document.getElementsByTagName('body')[0].getAttribute('data-scriptinjection') == null) ? '0' : '1');";
            var injected = await mainWebView.InvokeScriptAsync("eval", new string[] { checkIfInjected });

            if (injected != "1")
            {
                var script = await AssetManager.LoadAssetString(lightTheme ? "init-light.js" : "init-dark.js");
                var styleCss = await AssetManager.LoadAssetString(lightTheme ? "style-light.min.css" : "style-dark.min.css");
                var tokens = TokenHelper.GetTokens();
                script = script
                    .Replace("{{XPOTIFYCSSBASE64CONTENT}}", Convert.ToBase64String(Encoding.UTF8.GetBytes(styleCss)))
                    .Replace("{{XPOTIFYISPROVERSION}}", PackageHelper.IsProVersion ? "1" : "0")
                    .Replace("{{SPOTIFYACCESSTOKEN}}", tokens.AccessToken);

                try
                {
                    await mainWebView.InvokeScriptAsync("eval", new string[] { script });
                }
                catch (Exception ex)
                {
                    logger.Warn("InjectInitScript failed: " + ex.ToString());
                    AnalyticsHelper.Log("injectException", ex.Message, $"{LastInitErrors}\n{ex}");

                    if (ex.ToString().Contains("80020101"))
                    {
                        MessageDialog md = new MessageDialog(
                            content: $"Error details:\r\n\r\n{LastInitErrors}\r\n{ex}", 
                            title: "There was a problem while initializing Xpotify. Some features might not work properly.");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        md.ShowAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                    {
                        MessageDialog md = new MessageDialog(
                            content: ex.ToString(),
                            title: "Xpotify initialization failed. Some features might not work properly.");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        md.ShowAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }

                return true;
            }

            return false;
        }

        public async Task<bool> CheckLoggedIn()
        {
            var script = await AssetManager.LoadAssetString("isLoggedInCheck.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result != "0");
        }

        public async Task<bool> TryPushingFacebookLoginButton()
        {
            var script = await AssetManager.LoadAssetString("clickOnFacebookLogin.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public async Task EnableNowPlaying()
        {
            var script = "window.XpotifyScript.Common.Action.enableNowPlaying();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        public async Task<bool> GoBackIfPossible()
        {
            var script = "window.XpotifyScript.Common.Action.goBackIfPossible();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public void ClearCookies()
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

        private void ClearCookies(Uri uri)
        {
            HttpBaseProtocolFilter baseFilter = new HttpBaseProtocolFilter();
            foreach (var cookie in baseFilter.CookieManager.GetCookies(uri))
            {
                baseFilter.CookieManager.DeleteCookie(cookie);
            }
        }

        public async Task<string> GetPageUrl()
        {
            return await mainWebView.InvokeScriptAsync("eval", new string[] { "window.location.href" });
        }

        public async Task<string> GetPageTitle()
        {
            var findPageTitleScript = "window.XpotifyScript.Common.PageTitleFinder.getTitle();";
            var pageTitle = await mainWebView.InvokeScriptAsync("eval", new string[] { findPageTitleScript });

            return pageTitle;
        }

        public async Task NavigateToSpotifyUrl(string url)
        {
            var currentUrl = await mainWebView.InvokeScriptAsync("eval", new String[] { "document.location.href;" });

            if (currentUrl.ToLower().StartsWith(SpotifyPwaUrlBeginsWith.ToLower()))
            {
                var script = $"window.XpotifyScript.Common.Action.navigateToPage('{url.Replace("'", "\\'")}');";
                await mainWebView.InvokeScriptAsync("eval", new string[] { script });
            }
            else
            {
                Navigate(new Uri(url));
            }
        }

        internal async Task AutoPlay(AutoPlayAction action)
        {
            string script;
            if (action == AutoPlayAction.Track)
                script = "window.XpotifyScript.Common.Action.autoPlayTrack();";
            else
                script = "window.XpotifyScript.Common.Action.autoPlayPlaylist();";
            
            var currentPlaying = await mainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        public async Task<string> ClearPlaybackLocalStorage()
        {
            var script = await AssetManager.LoadAssetString("clearPlaybackLocalStorage.js");
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return result;
        }

        public async Task<bool> PlayPause()
        {
            var script = "window.XpotifyScript.Common.Action.playPause();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public async Task<bool> Play()
        {
            var script = "window.XpotifyScript.Common.Action.play();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public async Task<bool> Pause()
        {
            var script = "window.XpotifyScript.Common.Action.pause();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public async Task<bool> NextTrack()
        {
            var script = "window.XpotifyScript.Common.Action.nextTrack();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        public async Task<bool> PreviousTrack(bool canGoToBeginningOfCurrentSong = true)
        {
            string script;
            if (canGoToBeginningOfCurrentSong)
                script = "window.XpotifyScript.Common.Action.prevTrack();";
            else
                script = "window.XpotifyScript.Common.Action.prevTrackForce();";

            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        internal async Task<bool> IsPlayingOnThisApp()
        {
            var script = "window.XpotifyScript.Common.Action.isPlayingOnThisApp();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });

            return (result == "1");
        }

        internal async Task SeekPlayback(double percentage)
        {
            var script = $"window.XpotifyScript.Common.Action.seekPlayback({percentage});";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        internal async Task SeekVolume(double percentage)
        {
            var script = $"window.XpotifyScript.Common.Action.seekVolume({percentage});";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        internal async Task<bool> OnKeyDown(int charCode, bool shiftPressed, bool ctrlPressed, bool altPressed)
        {
            var script = $"window.XpotifyScript.Common.KeyboardShortcutListener.keyDownExternalCall" +
                $"({charCode}, {shiftPressed.ToString().ToLower()}, {ctrlPressed.ToString().ToLower()}, " +
                $"{altPressed.ToString().ToLower()});";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });
            logger.Info(result);
            return result == "1";
        }

        internal async Task TryResolvePlaybackStartStuck()
        {
            var script = $"window.XpotifyScript.Common.PlaybackStuckHelper.tryResolveStart();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });
        }

        internal async Task TryResolvePlaybackMiddleStuck()
        {
            var script = $"window.XpotifyScript.Common.PlaybackStuckHelper.tryResolveMiddle();";
            var result = await mainWebView.InvokeScriptAsync("eval", new string[] { script });
        }
    }
}
