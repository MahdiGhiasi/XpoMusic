﻿using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Helpers;
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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ModernSpotifyUWP.Flyouts
{
    public sealed partial class SettingsFlyout : UserControl, IFlyout<EventArgs>
    {
        public event EventHandler<EventArgs> FlyoutCloseRequest;

        public SettingsFlyout()
        {
            this.InitializeComponent();

            if (LocalConfiguration.Theme == Classes.Model.Theme.Dark)
                RequestedTheme = ElementTheme.Dark;
            else
                RequestedTheme = ElementTheme.Light;

            navigationView.SelectedItem = navigationView.MenuItems.First();
        }

        private void NavigationView_BackRequested(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewBackRequestedEventArgs args)
        {
            FlyoutCloseRequest?.Invoke(this, new EventArgs());
        }

        private void NavigationView_SelectionChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navOptions = new FrameNavigationOptions
            {                 
                IsNavigationStackEnabled = false,
                TransitionInfoOverride = new EntranceNavigationTransitionInfo(),
            };

            if (args.SelectedItem == settingsMenuItem)
                contentFrame.NavigateToType(typeof(SettingsPage), null, navOptions);
            else if (args.SelectedItem == aboutMenuItem)
                contentFrame.NavigateToType(typeof(AboutPage), null, navOptions);
            else if (args.SelectedItem == donateMenuItem)
                contentFrame.NavigateToType(typeof(DonatePage), null, navOptions);
        }
    }
}
