using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using XpoMusic.Classes;
using XpoMusic.SpotifyApi;

namespace XpoMusic.Helpers
{
    public static class AuthorizationHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal static void Authorize(WebViewController controller, bool clearExisting)
        {
            Authorize(controller, WebViewController.SpotifyPwaUrlHome, clearExisting);
        }

        internal static void Authorize(WebViewController controller, string targetUrl, bool clearExisting)
        {
            if (clearExisting)
            {
                controller.ClearCookies();
                TokenHelper.ClearTokens();
                LocalConfiguration.IsLoggedInByFacebook = false;
            }

            var authorizationUrl = Authorization.GetAuthorizationUrl(targetUrl);
            controller.Navigate(new Uri(authorizationUrl));
        }

        internal static async void FinalizeAuthorization(WebViewController controller, string url)
        {
            try
            {
                var urlDecoder = new WwwFormUrlDecoder(url.Substring(url.IndexOf('?') + 1));
                await Authorization.RetrieveAndSaveTokensFromAuthCode(urlDecoder.GetFirstValueByName("code"));
                controller.Navigate(new Uri(urlDecoder.GetFirstValueByName("state")));
            }
            catch (Exception ex)
            {
                logger.Info($"Authorization failed: {ex}");

                Authorize(controller, clearExisting: false);
            }
        }
    }
}
