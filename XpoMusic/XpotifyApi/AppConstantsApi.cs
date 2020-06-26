using XpoMusic.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.XpotifyApi
{
    public static class AppConstantsApi
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static string UpdateUri
        {
            get
            {
                var endpoint = "https://xpomusic.com/constants";
                var version = PackageHelper.GetAppVersion().ToString(3);
                var fileName = "constants.json";
                var query = "?date=" + DateTime.Now.ToString("yyyyMMddHHmmss");
                var proSuffix = PackageHelper.IsProVersion ? "pro" : "";

                return $"{endpoint}/{version}{proSuffix}/{fileName}{query}";
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
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            // If this happens, first session will contain the previous updated values since it's cached. 
                            // But the next sessions will have the values defined in code.
                            logger.Info("GetAppConstantsString could not find a constants file on the server. Will return an empty json.");
                            return "{}";
                        }
                        else
                        {
                            logger.Warn($"GetAppConstantsString failed because request failed with status code {response.StatusCode}.");
                            return null;
                        }
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetAppConstantsString failed: " + ex.ToString());
                return null;
            }
        }
    }
}
