using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Helpers;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ModernSpotifyUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        private const string supportEmailAddress = "xpotifyapp@gmail.com";
        private readonly Uri twitterPageUri = new Uri("https://twitter.com/Xpotify");
        private readonly Uri privacyPolicyPageUri = new Uri("https://ghiasi.net/xpotify/privacy.html");

        public AboutPage()
        {
            this.InitializeComponent();

            if (LocalConfiguration.Theme == Classes.Model.Theme.Dark)
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
        }

        private async void RateAndReviewButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(string.Format("ms-windows-store:REVIEW?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName)));
        }

        private async void SendFeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            RandomAccessStreamReference logFileReference = null;
            var emailMessage = new EmailMessage
            {
                Body = "",
            };

            try
            {
                if (await ApplicationData.Current.LocalFolder.TryGetItemAsync("logs") is StorageFolder logFolder)
                {
                    if (await logFolder.TryGetItemAsync("xpotify-email.log") is StorageFile oldLogFile)
                        await oldLogFile.DeleteAsync();

                    if (await logFolder.TryGetItemAsync("xpotify.log") is StorageFile logFile)
                    {
                        var newFile = await logFile.CopyAsync(logFolder, "xpotify-email.log");
                        logFileReference = RandomAccessStreamReference.CreateFromFile(newFile);
                    }
                }
            }
            catch (Exception ex)
            {
                emailMessage.Body = "\r\n\r\n\r\n\r\nCould not attach log file\r\n" + ex.ToString();
            }

            emailMessage.To.Add(new EmailRecipient(supportEmailAddress));
            emailMessage.Subject = $"Xpotify v{PackageHelper.GetAppVersionString()}";
            if (logFileReference != null)
            {
                emailMessage.Attachments.Add(new EmailAttachment("xpotify-email.log", logFileReference));
            }

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);

        }

        private async void PrivacyPolicyButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(privacyPolicyPageUri);
        }

        private async void TwitterButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(twitterPageUri);
        }
    }
}
