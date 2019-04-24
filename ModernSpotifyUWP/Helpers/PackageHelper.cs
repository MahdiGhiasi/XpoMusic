using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;

namespace ModernSpotifyUWP.Helpers
{
    public static class PackageHelper
    {
        public static string GetAppVersionString()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        public static Version GetAppVersion()
        {
            return Version.Parse(GetAppVersionString());
        }

        public static void RestartApp()
        {
            ToastHelper.SendReopenAppToast();
            Application.Current.Exit();
        }
    }
}
