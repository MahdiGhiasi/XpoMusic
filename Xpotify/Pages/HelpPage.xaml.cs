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
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xpotify.Helpers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Xpotify.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HelpPage : Page
    {
        private const string supportEmailAddress = "xpotifyapp@gmail.com";
        private readonly Uri gitHubIssuesHelpPage = new Uri("https://github.com/MahdiGhiasi/Xpotify/issues");
        private readonly Uri localProxyHelpPage = new Uri("https://ghiasi.net/xpotify/loopback");
        private readonly Uri keyboardShortuctsHelpPage = new Uri("https://ghiasi.net/xpotify/keyboardShortcuts");
        private readonly Uri discordServerUrl = new Uri("https://discord.gg/4RQCctB");

        public HelpPage()
        {
            this.InitializeComponent();
        }

        private async void Contact_Click(Hyperlink sender, HyperlinkClickEventArgs args)
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
            emailMessage.Subject = $"{PackageHelper.GetAppNameString()} v{PackageHelper.GetAppVersionString()}";
            if (logFileReference != null)
            {
                emailMessage.Attachments.Add(new EmailAttachment("xpotify-email.log", logFileReference));
            }

            await EmailManager.ShowComposeNewEmailAsync(emailMessage);
            AnalyticsHelper.Log("helpLink", "email");
        }

        private async void GitHubIssue_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(gitHubIssuesHelpPage);
            AnalyticsHelper.Log("helpLink", "gitHubIssue");
        }

        private async void LocalProxy_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(localProxyHelpPage);
            AnalyticsHelper.Log("helpLink", "localProxy");
        }

        private async void KeyboardShortcuts_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(keyboardShortuctsHelpPage);
            AnalyticsHelper.Log("helpLink", "keyboardShortcuts");
        }

        private async void DiscordGroup_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(discordServerUrl);
            AnalyticsHelper.Log("helpLink", "joinDiscordServer");
        }
    }
}
