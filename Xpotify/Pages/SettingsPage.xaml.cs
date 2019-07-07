using Xpotify.Helpers;
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

namespace Xpotify.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void RestartApp_Click(object sender, RoutedEventArgs e)
        {
            PackageHelper.RestartApp();
        }

        private async void PinTileToStart_Click(object sender, RoutedEventArgs e)
        {
            await LiveTileHelper.PinToStart();
            ViewModel.CheckPrimaryTileStatus();
        }
    }
}
