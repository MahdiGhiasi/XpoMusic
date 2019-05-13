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
        private static readonly string proPackageName = "36835MahdiGhiasi.XpotifyPro";
        internal static readonly Uri ProStoreUri = new Uri("ms-windows-store://pdp/?productid=9PC9VV8KTXPL");

        public static string GetAppVersionString()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        public static string GetAppNameString()
        {
            return Package.Current.DisplayName;
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

        public static bool IsProVersion => Package.Current.Id.FamilyName.Contains(proPackageName);
    }
}
