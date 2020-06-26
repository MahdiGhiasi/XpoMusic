using XpoMusic.Classes;
using XpoMusic.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace XpoMusic.Pages
{
    public sealed partial class AboutPage : Page
    {
        private readonly Uri twitterPageUri = new Uri("https://twitter.com/XpoMusic");
        private readonly Uri githubPageUri = new Uri("https://github.com/MahdiGhiasi/Xpotify");
        private readonly Uri privacyPolicyPageUri = new Uri("https://xpomusic.com/privacy.html");

        public AboutPage()
        {
            this.InitializeComponent();

            if (ThemeHelper.GetCurrentTheme() == Classes.Model.Theme.Dark)
            {
                coloredLogo.Visibility = Visibility.Collapsed;
                whiteLogo.Visibility = Visibility.Visible;
            }
            else
            {
                coloredLogo.Visibility = Visibility.Visible;
                whiteLogo.Visibility = Visibility.Collapsed;
            }

            appNameText.Text = PackageHelper.GetAppNameString();
            appVersionText.Text = PackageHelper.GetAppVersionString();

            if (LocalConfiguration.LatestAssetUpdateVersion == 0)
            {
                assetUpdatePackSection.Visibility = Visibility.Collapsed;
            }
            else
            {
                assetUpdatePackSection.Visibility = Visibility.Visible;
                assetUpdatePackText.Text = LocalConfiguration.LatestAssetUpdateVersion.ToString();
            }
        }

        private async void RateAndReviewButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
            AnalyticsHelper.Log("aboutLink", "rateAndReview");
        }

        private async void PrivacyPolicyButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(privacyPolicyPageUri);
            AnalyticsHelper.Log("aboutLink", "privacyPolicy");
        }

        private async void TwitterButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(twitterPageUri);
            AnalyticsHelper.Log("aboutLink", "twitter");
        }

        private async void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(githubPageUri);
            AnalyticsHelper.Log("aboutLink", "gitHub");
        }
    }
}
