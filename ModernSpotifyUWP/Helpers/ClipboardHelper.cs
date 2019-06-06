using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Xpotify.Helpers
{
    public static class ClipboardHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static async Task<bool> IsSpotifyUriPresent()
        {
            try
            {
                var content = Clipboard.GetContent();

                if (!content.Contains(StandardDataFormats.Text))
                    return false;

                var data = await content.GetTextAsync();
                var uri = SpotifyShareUriHelper.GetPwaUri(data);

                return !string.IsNullOrWhiteSpace(uri);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return false;
            }
        }

        public static async Task<string> GetSpotifyUri()
        {
            try
            {
                var content = Clipboard.GetContent();

                if (!content.Contains(StandardDataFormats.Text))
                    return "";

                var data = await content.GetTextAsync();
                var uri = SpotifyShareUriHelper.GetPwaUri(data);

                return uri;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return "";
            }
        }
    }
}
