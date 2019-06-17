using Xpotify.Classes;
using Xpotify.Helpers;
using Xpotify.XpotifyApi.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Xpotify.Flyouts
{
    public sealed partial class DeveloperMessageFlyout : UserControl, IFlyout<EventArgs>
    {
        public event EventHandler<EventArgs> FlyoutCloseRequest;

        public DeveloperMessageFlyout()
        {
            this.InitializeComponent();

            if (ThemeHelper.GetCurrentTheme() == Classes.Model.Theme.Dark)
                RequestedTheme = ElementTheme.Dark;
            else
                RequestedTheme = ElementTheme.Light;

            navigationView.SelectedItem = navigationView.MenuItems.First();
        }

        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            FlyoutCloseRequest?.Invoke(this, new EventArgs());
        }

        public void InitFlyout(DeveloperMessage message)
        {
            messageTitle.Text = message.title;
            messageContent.Text = message.content;
            messageDate.Text = TimeAgo(message.messageDate);
        }

        /// <summary>
        /// Taken from https://dotnetthoughts.net/time-ago-function-for-c/
        /// </summary>
        public static string TimeAgo(DateTime dateTime)
        {
            string result = string.Empty;
            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromSeconds(60))
            {
                result = "just now";
            }
            else if (timeSpan <= TimeSpan.FromMinutes(60))
            {
                result = timeSpan.Minutes > 1 ?
                    String.Format("{0} minutes ago", timeSpan.Minutes) :
                    "a minute ago";
            }
            else if (timeSpan <= TimeSpan.FromHours(24))
            {
                result = timeSpan.Hours > 1 ?
                    String.Format("{0} hours ago", timeSpan.Hours) :
                    "an hour ago";
            }
            else if (timeSpan <= TimeSpan.FromDays(30))
            {
                result = timeSpan.Days > 1 ?
                    String.Format("{0} days ago", timeSpan.Days) :
                    "yesterday";
            }
            else if (timeSpan <= TimeSpan.FromDays(365))
            {
                result = timeSpan.Days > 30 ?
                    String.Format("{0} months ago", timeSpan.Days / 30) :
                    "a month ago";
            }
            else
            {
                result = "";
            }

            return result;
        }
    }
}
