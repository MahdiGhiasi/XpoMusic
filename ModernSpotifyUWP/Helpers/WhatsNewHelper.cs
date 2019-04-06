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

        public static bool ShouldShowWhatsNew()
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(latestWhatsNewVersionKey))
            {
                MarkThisWhatsNewAsRead();
                return false;
            }

            if (Version.TryParse(ApplicationData.Current.LocalSettings.Values[latestWhatsNewVersionKey].ToString(), out Version v))
            {
                if (v < Version.Parse(PackageHelper.GetAppVersion()))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<string> GetWhatsNewContentId()
        {
            List<string> output = new List<string>();

            if (!ShouldShowWhatsNew())
                return output;

            Version prevVersion = new Version(0, 0, 0, 0);

            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(latestWhatsNewVersionKey))
                Version.TryParse(ApplicationData.Current.LocalSettings.Values[latestWhatsNewVersionKey].ToString(), out prevVersion);

            //if (prevVersion < new Version("1.1.0.0"))
            //    output.Add("1");

            MarkThisWhatsNewAsRead();

            return output;
        }

        private static void MarkThisWhatsNewAsRead()
        {
            ApplicationData.Current.LocalSettings.Values[latestWhatsNewVersionKey] = PackageHelper.GetAppVersion();
        }
    }
}
