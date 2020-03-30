using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using XpoMusic.Classes.Model.WebResourceModifications;
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
#if DEBUG
            var initialStateJson = JsonConvert.SerializeObject(this, Formatting.Indented);
#endif

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
        public int PlayStatePollIntervalMilliseconds { get; private set; } = 20000;

        [JsonProperty]
        public int PlayStatePollIntervalMillisecondsWithCompactOverlayOpen { get; private set; } = 15000;

        [JsonIgnore]
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
        public int HeartbeatIntervalSeconds { get; private set; } = 60 * 10;

        [JsonIgnore]
        public TimeSpan HeartbeatInterval => TimeSpan.FromSeconds(HeartbeatIntervalSeconds);

        [JsonProperty]
        public int StuckDetectSameElapsedCount { get; private set; } = 10;

        [JsonProperty]
        public int StuckDetectSameElapsedExtraCount { get; private set; } = 5;

        [JsonProperty]
        public int MaxStuckResolveTryCount { get; private set; } = 1;

        [JsonProperty]
        public WebResourceModificationRule[] ModificationRules { get; private set; } = new[]
            {
                new WebResourceModificationRule
                {
                    UriRegexMatch = @"web-player\.[a-zA-Z0-9]*\.js",
                    Type = WebResourceModificationRuleType.ModifyString,
                    StringModificationRules = new[]
                    {
                        new WebResourceStringModificationRule
                        {
                            RegexMatch = Regex.Escape(@"window.matchMedia(""(display-mode: standalone)"").addEventListener(""change"",function(t){e(t.matches)})"),
                            ReplaceTo = @"window.matchMedia(""(display-mode: standalone)"").addListener(""change"",function(t){e(t.matches)})",
                        },
                        new WebResourceStringModificationRule
                        {
                            RegexMatch = Regex.Escape(@"var t=window.matchMedia(""(display-mode: standalone)"");(t.addEventListener||t.addListener)(""change"",function(t){return e(t.matches)})"),
                            ReplaceTo = @"var t=window.matchMedia(""(display-mode: standalone)"");t.addListener(""change"",function(t){return e(t.matches)})",
                        },
                        new WebResourceStringModificationRule
                        {
                            RegexMatch = @"""Web Player \("".concat\([a-zA-Z]+.getBrowserName\(\),""\)""\)",
                            ReplaceTo = @"""Xpo Music""",
                        },
                        new WebResourceStringModificationRule
                        {
                            RegexMatch = @"\{\.\.\.r\._propagators,\.\.\.e\.propagators\}",
                            ReplaceTo = @"Object.assign({}, r._propagators, e.propagators)",
                        },
                        new WebResourceStringModificationRule
                        {
                            RegexMatch = @"\{\.\.\.p\}",
                            ReplaceTo = @"Object.assign({}, p)",
                        },
                    }
                },
                new WebResourceModificationRule
                {
                    UriRegexMatch = @"https\:\/\/accounts\.spotify\.com(\/[a-z]+)?\/status.*",
                    Type = WebResourceModificationRuleType.ModifyString,
                    StringModificationRules = new[]
                    {
                        new WebResourceStringModificationRule
                        {
                            RegexMatch = @"^(.*\s*)*$",
                            ReplaceTo = @"<script>window.location.href='https://open.spotify.com'</script><meta http-equiv='refresh' content='0;URL=https://open.spotify.com/'>",
                        },
                    }
                }
            };

        [JsonProperty]
        public bool WebPlayerBackupEnabled { get; private set; } = true;

        [JsonProperty]
        public bool XamlInvertFilterForLightThemeEnabled { get; private set; } = true;

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
