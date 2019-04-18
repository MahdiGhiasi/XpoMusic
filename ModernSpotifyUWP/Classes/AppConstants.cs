using ModernSpotifyUWP.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace ModernSpotifyUWP.Classes
{
    public class AppConstants
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static AppConstants instance;
        public static AppConstants Instance
        {
            get
            {
                if (instance == null)
                    instance = new AppConstants();

                return instance;
            }
        }

        private AppConstants() { }

        [JsonProperty]
        public int PlayStatePollIntervalMilliseconds { get; private set; } = 20000;

        [JsonProperty]
        public int PlayStatePollIntervalMillisecondsWithCompactOverlayOpen { get; private set; } = 10000;

        public TimeSpan PlayStatePollInterval
        {
            get
            {
                if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay)
                    return TimeSpan.FromMilliseconds(PlayStatePollIntervalMillisecondsWithCompactOverlayOpen);

                return TimeSpan.FromMilliseconds(PlayStatePollIntervalMilliseconds);
            }
        }

        private string UpdateUri
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

        public async void Update()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(UpdateUri);
                    if (response.IsSuccessStatusCode == false)
                    {
                        logger.Warn($"AppConstants update failed because request failed with status code {response.StatusCode}.");
                        return;
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    JsonConvert.PopulateObject(result, this);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("AppConstants update failed: " + ex.ToString());
            }
        }
    }
}
