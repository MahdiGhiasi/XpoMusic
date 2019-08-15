using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using XpoMusic.Helpers;
using XpoMusic.XpotifyApi;

namespace XpoMusic.Classes
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

        private AppConstants()
        {
            var latestSavedAppConstantsString = LocalConfiguration.LatestAppConstants;
            try
            {
                if (!string.IsNullOrWhiteSpace(latestSavedAppConstantsString))
                {
                    JsonConvert.PopulateObject(latestSavedAppConstantsString, this);
                    this.ConstantsUpdated?.Invoke(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Updating AppConstants from latest saved app constants string failed. The content was: '{latestSavedAppConstantsString}'");
                logger.Error($"Updating AppConstants from latest saved app constants string failed: {ex}");
            }
        }

        public event EventHandler ConstantsUpdated;

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

        [JsonProperty]
        public bool HeartbeatEnabled { get; private set; } = true;

        [JsonProperty]
        public int HeartbeatIntervalSeconds { get; private set; } = 60 * 15;

        public TimeSpan HeartbeatInterval => TimeSpan.FromSeconds(HeartbeatIntervalSeconds);

        [JsonProperty]
        public int StuckDetectSameElapsedCount { get; private set; } = 10;

        [JsonProperty]
        public int StuckDetectSameElapsedExtraCount { get; private set; } = 5;

        [JsonProperty]
        public int MaxStuckResolveTryCount { get; private set; } = 1;

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
                Instance.ConstantsUpdated?.Invoke(Instance, new EventArgs());

                LocalConfiguration.LatestAppConstants = updateString;
            }
            catch (Exception ex)
            {
                logger.Warn("AppConstants.Update failed: " + ex.ToString());
                AnalyticsHelper.Log("constantsFetchException", ex.Message, ex.ToString());
            }
        }
    }
}
