using XpoMusic.Classes;
using XpoMusic.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace XpoMusic.Flyouts
{
    public sealed partial class WhatsNewFlyout : UserControl, IFlyout<EventArgs>
    {
        public event EventHandler<EventArgs> FlyoutCloseRequest;

        public WhatsNewFlyout()
        {
            this.InitializeComponent();

            if (ThemeHelper.GetCurrentTheme() == Classes.Model.Theme.Dark)
                RequestedTheme = ElementTheme.Dark;
            else
                RequestedTheme = ElementTheme.Light;

            foreach (var item in Content.Children)
            {
                var sp = (item as StackPanel);
                if (sp != null)
                    sp.Visibility = Visibility.Collapsed;
            }

            if (WhatsNewHelper.testMode)
                VersionText.Text = "Next";
            else
                VersionText.Text = PackageHelper.GetAppVersionString();

            navigationView.SelectedItem = navigationView.MenuItems.First();
        }

        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            FlyoutCloseRequest?.Invoke(this, new EventArgs());
        }

        public void InitFlyout()
        {
            bool changelogPresent = false;
            var ids = WhatsNewHelper.GetWhatsNewContentIdAndMarkAsRead();

            foreach (var item in Content.Children)
            {
                var sp = item as StackPanel;
                if (sp == null)
                    continue;

                if (ids.Contains(sp.Tag.ToString()))
                {
                    changelogPresent = true;
                    sp.Visibility = Visibility.Visible;
                }
                else
                {
                    sp.Visibility = Visibility.Collapsed;
                }
            }

            if (!changelogPresent)
                FlyoutCloseRequest?.Invoke(this, new EventArgs());
#if !DEBUG
            //App.Tracker.Send(HitBuilder.CreateCustomEvent("What's new", "Show", DeviceInfo.ApplicationVersionString).Build());
#endif
        }

        private async void GetXpotifyPro_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(PackageHelper.ProStoreUri);
            AnalyticsHelper.Log("whatsNew", "getXpotifyPro");
        }
    }
}
