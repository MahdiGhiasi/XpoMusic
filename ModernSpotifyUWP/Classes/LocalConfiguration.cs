using ModernSpotifyUWP.Classes.Model;
using ModernSpotifyUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using static ModernSpotifyUWP.Helpers.ProxyHelper;

namespace ModernSpotifyUWP.Classes
{
    public static class LocalConfiguration
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly Size compactOverlayDefaultSize = new Size(300, 300);

        public static Size CompactOverlaySize
        {
            get
            {
                var config = GetConfiguration("CompactOverlaySize");

                if (string.IsNullOrEmpty(config))
                    return compactOverlayDefaultSize;

                try
                {
                    var configParts = config.Split(';');
                    return new Size(double.Parse(configParts[0]), double.Parse(configParts[1]));
                }
                catch
                {
                    return compactOverlayDefaultSize;
                }
            }

            set
            {
                SetConfiguration("CompactOverlaySize", $"{value.Width};{value.Height}");
            }
        }

        public static Size WindowMinSize => new Size(500, 500);

        public static bool IsLoggedInByFacebook
        {
            get
            {
                return GetConfiguration("IsLoggedInByFacebook") == "1";
            }
            set
            {
                SetConfiguration("IsLoggedInByFacebook", value ? "1" : "0");
            }
        }

        public static bool IsCustomProxyEnabled
        {
            get
            {
                return GetConfiguration("IsCustomProxyEnabled") == "1";
            }
            set
            {
                SetConfiguration("IsCustomProxyEnabled", value ? "1" : "0");
            }
        }

        public static string CustomProxyAddress
        {
            get
            {
                return GetConfiguration("CustomProxyAddress");
            }
            set
            {
                SetConfiguration("CustomProxyAddress", value);
            }
        }

        public static string CustomProxyPort
        {
            get
            {
                return GetConfiguration("CustomProxyPort");
            }
            set
            {
                SetConfiguration("CustomProxyPort", value);
            }
        }

        public static ProxyType CustomProxyType
        {
            get
            {
                if (!int.TryParse(GetConfiguration("CustomProxyType"), out int type))
                    type = (int)ProxyType.HttpHttps;

                return (ProxyType)type;
            }
            set
            {
                SetConfiguration("CustomProxyType", ((int)value).ToString());
            }
        }

        public static Language Language
        {
            get
            {
                if (!int.TryParse(GetConfiguration("Language"), out int type))
                    type = (int)Language.Default;

                return (Language)type;
            }
            set
            {
                SetConfiguration("Language", ((int)value).ToString());
            }
        }

        public static Theme Theme
        {
            get
            {
                if (!int.TryParse(GetConfiguration("Theme"), out int type))
                    type = (int)Theme.Dark;

                return (Theme)type;
            }
            set
            {
                SetConfiguration("Theme", ((int)value).ToString());
            }
        }

        public static string[] DeveloperMessageShownIds
        {
            get
            {
                try
                {
                    var ids = GetConfiguration("DeveloperMessageShownIds");

                    if (string.IsNullOrEmpty(ids))
                        return new string[] { };

                    return ids.Split(';');
                }
                catch (Exception ex)
                {
                    logger.Warn("DeveloperMessageShownIds failed: " + ex.ToString());
                    AnalyticsHelper.Log("exception-DeveloperMessageShownIds", ex.Message, ex.ToString());
                    return new string[] { };
                }
            }
        }

        public static void AddIdToDeveloperMessageShownIds(string id)
        {
            var ids = DeveloperMessageShownIds.ToList();
            ids.Add(id);

            SetConfiguration("DeveloperMessageShownIds", string.Join(';', ids.ToArray()));
        }

        public static int LatestAssetUpdateVersion
        {
            get
            {
                if (int.TryParse(GetConfiguration("LatestAssetUpdateVersion"), out int latestVersion))
                    return latestVersion;

                return 0;
            }
            set
            {
                SetConfiguration("LatestAssetUpdateVersion", value.ToString());
            }
        }

        public static Version LatestAppVersionOnAssetUpdate
        {
            get
            {
                if (Version.TryParse(GetConfiguration("LatestAppVersionOnAssetUpdate"), out Version latestVersion))
                    return latestVersion;

                return PackageHelper.GetAppVersion();
            }
            set
            {
                SetConfiguration("LatestAppVersionOnAssetUpdate", value.ToString());
            }
        }

        private static string GetConfiguration(string key)
        {
            var completeKey = "LocalConfiguration_" + key;
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(completeKey))
                return null;

            return ApplicationData.Current.LocalSettings.Values[completeKey].ToString();
        }

        private static void SetConfiguration(string key, string value)
        {
            var completeKey = "LocalConfiguration_" + key;
            ApplicationData.Current.LocalSettings.Values[completeKey] = value;
        }
    }
}
