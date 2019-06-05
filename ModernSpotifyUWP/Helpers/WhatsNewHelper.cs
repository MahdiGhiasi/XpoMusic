using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ModernSpotifyUWP.Helpers
{
    public static class WhatsNewHelper
    {
        const string latestWhatsNewVersionKey = "LatestWhatsNewVersion";
        public static readonly bool testMode = false;

        public static bool ShouldShowWhatsNew()
        {
            if (testMode)
                return true;

            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(latestWhatsNewVersionKey))
            {
                MarkThisWhatsNewAsRead();
                return false;
            }

            if (Version.TryParse(ApplicationData.Current.LocalSettings.Values[latestWhatsNewVersionKey].ToString(), out Version v))
            {
                if (v < Version.Parse(PackageHelper.GetAppVersionString()))
                {
                    if (GetWhatsNewContentId().Count > 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static List<string> GetWhatsNewContentIdAndMarkAsRead()
        {
            if (!ShouldShowWhatsNew())
                return new List<string>();

            List<string> output = GetWhatsNewContentId();

            MarkThisWhatsNewAsRead();

            return output;
        }

        private static List<string> GetWhatsNewContentId()
        {
            Version prevVersion = new Version(0, 0, 0, 0);

            if (!testMode)
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey(latestWhatsNewVersionKey))
                    Version.TryParse(ApplicationData.Current.LocalSettings.Values[latestWhatsNewVersionKey].ToString(), out prevVersion);

            List<string> output = new List<string>();

            if (prevVersion < new Version("1.6.0.0"))
                output.Add("4");

            if (prevVersion < new Version("1.5.0.0"))
                output.Add("3");

            if (prevVersion < new Version("1.4.2.0"))
                output.Add("2");

            if (prevVersion < new Version("1.1.0.0"))
                output.Add("1");

            return output;
        }

        private static void MarkThisWhatsNewAsRead()
        {
            ApplicationData.Current.LocalSettings.Values[latestWhatsNewVersionKey] = PackageHelper.GetAppVersionString();
        }
    }
}
