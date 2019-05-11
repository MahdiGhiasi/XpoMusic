using ModernSpotifyUWP.Helpers;
using ModernSpotifyUWP.XpotifyApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.XpotifyApi
{
    public static class DeveloperMessageApi
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static string DevMessagesUri
        {
            get
            {
                var endpoint = "https://ghiasi.net/xpotify/devmessages";
                var version = PackageHelper.GetAppVersion().ToString(3);
                var fileName = "messages.json";
                var query = "?date=" + DateTime.Now.ToString("yyyyMMddHHmmss");
                var proSuffix = PackageHelper.IsProVersion ? "pro" : "";

                return $"{endpoint}/{version}{proSuffix}/{fileName}{query}";
            }
        }

        public static async Task<DeveloperMessageCollection> GetDeveloperMessages()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(DevMessagesUri);
                    if (response.IsSuccessStatusCode == false)
                    {
                        logger.Warn($"GetDeveloperMessages failed because request failed with status code {response.StatusCode}.");
                        return null;
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<DeveloperMessageCollection>(result);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetDeveloperMessages failed: " + ex.ToString());
                return null;
            }
        }
    }
}
