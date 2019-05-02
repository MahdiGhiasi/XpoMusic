using ModernSpotifyUWP.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.XpotifyApi
{
    public static class AppConstantsApi
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static string UpdateUri
        {
            get
            {
                var endpoint = "https://ghiasi.net/xpotify/constants";
                var version = PackageHelper.GetAppVersion().ToString(3);
                var fileName = "constants.json";
                var query = "?date=" + DateTime.Now.ToString("yyyyMMddHHmmss");

                return $"{endpoint}/{version}/{fileName}{query}";
            }
        }

        public static async Task<string> GetAppConstantsString()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(UpdateUri);
                    if (response.IsSuccessStatusCode == false)
                    {
                        logger.Warn($"UpdateAppConstants failed because request failed with status code {response.StatusCode}.");
                        return null;
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("UpdateAppConstants failed: " + ex.ToString());
                return null;
            }
        }
    }
}
