using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xpotify.Helpers;
using Xpotify.XpotifyApi.Model;

namespace Xpotify.Controls
{
    public sealed partial class FlyoutContainer : UserControl
    {
        public FlyoutContainer()
        {
            this.InitializeComponent();
        }

        public void OpenSettings()
        {
            OpenSettings(0);
        }

        public void OpenAbout()
        {
            OpenSettings(1);
        }

        public void OpenDonate()
        {
            OpenSettings(2);
        }

        private void OpenSettings(int tabId)
        {
            settingsFlyout.InitFlyout(tabId);
            settingsFlyout.Visibility = Visibility.Visible;
            VisualStateManager.GoToState(this, nameof(OverlayVisibleVisualState), false);
        }

        public async void OpenWhatsNew()
        {
            await Task.Delay(500);

            whatsNewFlyout.InitFlyout();
            whatsNewFlyout.Visibility = Visibility.Visible;
            VisualStateManager.GoToState(this, nameof(OverlayVisibleVisualState), false);

            AnalyticsHelper.Log("flyoutShow", "whatsNew", PackageHelper.GetAppVersionString());
        }

        public async void OpenDeveloperMessage(DeveloperMessage message)
        {
            await Task.Delay(700);

            while (whatsNewFlyout.Visibility == Visibility.Visible)
                await Task.Delay(500);

            developerMessageFlyout.InitFlyout(message);
            developerMessageFlyout.Visibility = Visibility.Visible;
            VisualStateManager.GoToState(this, nameof(OverlayVisibleVisualState), false);

            AnalyticsHelper.Log("flyoutShow", "developerMessage", message.id);
        }

        private void WhatsNewFlyout_FlyoutCloseRequest(object sender, EventArgs e)
        {
            CloseOverlays();
        }

        private void SettingsFlyout_FlyoutCloseRequest(object sender, EventArgs e)
        {
            CloseOverlays();
        }

        private void DeveloperMessageFlyout_FlyoutCloseRequest(object sender, EventArgs e)
        {
            CloseOverlays();
        }

        private async void CloseOverlays()
        {
            VisualStateManager.GoToState(this, nameof(OverlayClosedVisualState), false);

            await Task.Delay(300);

            developerMessageFlyout.Visibility = Visibility.Collapsed;
            whatsNewFlyout.Visibility = Visibility.Collapsed;
            settingsFlyout.Visibility = Visibility.Collapsed;
        }

    }
}
