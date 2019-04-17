using ModernSpotifyUWP.Helpers;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ModernSpotifyUWP.Flyouts
{
    public sealed partial class WhatsNewFlyout : UserControl, IFlyout<EventArgs>
    {
        public event EventHandler<EventArgs> FlyoutCloseRequest;

        public WhatsNewFlyout()
        {
            this.InitializeComponent();

            foreach (var item in Content.Children)
            {
                var sp = (item as StackPanel);
                if (sp != null)
                    sp.Visibility = Visibility.Collapsed;
            }

            VersionText.Text = PackageHelper.GetAppVersionString();
        }

        private void OKButton_Tapped(object sender, TappedRoutedEventArgs e)
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
    }
}
