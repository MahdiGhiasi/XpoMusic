using ModernSpotifyUWP.Helpers;
using ModernSpotifyUWP.XpotifyApi;
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
        public int PlayStatePollIntervalMilliseconds { get; private set; } = 10000;

        [JsonProperty]
        public int PlayStatePollIntervalMillisecondsWithCompactOverlayOpen { get; private set; } = 8000;

        public TimeSpan PlayStatePollInterval
        {
            get
            {
                if (ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay)
                    return TimeSpan.FromMilliseconds(PlayStatePollIntervalMillisecondsWithCompactOverlayOpen);

                return TimeSpan.FromMilliseconds(PlayStatePollIntervalMilliseconds);
            }
        }

        public static async void Update()
        {
            try
            {
                var updateString = await AppConstantsApi.GetAppConstantsString();

                if (string.IsNullOrWhiteSpace(updateString))
                {
                    logger.Info("No AppConstants update available.");
                    return;
                }

                JsonConvert.PopulateObject(updateString, Instance);
            }
            catch (Exception ex)
            {
                logger.Warn("AppConstants.Update failed: " + ex.ToString());
            }
        }
    }
}
