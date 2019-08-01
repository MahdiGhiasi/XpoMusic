using XpoMusic.Classes;
using XpoMusic.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using XpoMusic.Pages;

namespace XpoMusic.Flyouts
{
    public sealed partial class SettingsFlyout : UserControl, IFlyout<EventArgs>
    {
        public event EventHandler<EventArgs> FlyoutCloseRequest;

        public SettingsFlyout()
        {
            this.InitializeComponent();

            if (ThemeHelper.GetCurrentTheme() == Classes.Model.Theme.Dark)
                RequestedTheme = ElementTheme.Dark;
            else
                RequestedTheme = ElementTheme.Light;
        }

        public void InitFlyout(int tabId)
        {
            navigationView.SelectedItem = navigationView.MenuItems[tabId];
        }

        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            FlyoutCloseRequest?.Invoke(this, new EventArgs());
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem == settingsMenuItem)
            {
                contentFrame.Navigate(typeof(SettingsPage), null, new EntranceNavigationTransitionInfo());
                AnalyticsHelper.PageView("SettingsPage");
            }
            else if (args.SelectedItem == helpMenuItem)
            {
                contentFrame.Navigate(typeof(HelpPage), null, new EntranceNavigationTransitionInfo());
                AnalyticsHelper.PageView("HelpPage");
            }
            else if (args.SelectedItem == aboutMenuItem)
            {
                contentFrame.Navigate(typeof(AboutPage), null, new EntranceNavigationTransitionInfo());
                AnalyticsHelper.PageView("AboutPage");
            }
            else if (args.SelectedItem == donateMenuItem)
            {
                contentFrame.Navigate(typeof(DonatePage), null, new EntranceNavigationTransitionInfo());
                AnalyticsHelper.PageView("DonatePage");
            }
        }
    }
}
