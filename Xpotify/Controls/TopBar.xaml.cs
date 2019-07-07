using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xpotify.Classes.Model;
using Xpotify.Helpers;
using Xpotify.SpotifyApi;

namespace Xpotify.Controls
{
    public sealed partial class TopBar : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public event EventHandler<string> OpenSpotifyUriRequested;

        #region Custom Properties
        public static readonly DependencyProperty ShowExtraButtonsProperty = DependencyProperty.Register(
            "ShowExtraButtons", typeof(bool), typeof(TopBar), new PropertyMetadata(defaultValue: false, 
                propertyChangedCallback: new PropertyChangedCallback(OnShowExtraButtonsPropertyChanged)));

        public bool ShowExtraButtons
        {
            get => (bool)GetValue(ShowExtraButtonsProperty);
            set
            {
                SetValue(ShowExtraButtonsProperty, value);

                ViewModel.ShowExtraButtons = value;

                if (ShowExtraButtons && !clipboardCheckTimer.IsEnabled)
                    clipboardCheckTimer.Start();
                else if (!ShowExtraButtons && clipboardCheckTimer.IsEnabled)
                    clipboardCheckTimer.Stop();
            }
        }

        private static void OnShowExtraButtonsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TopBar).ShowExtraButtons = (bool)e.NewValue;
        }
        #endregion

        private DispatcherTimer clipboardCheckTimer;

        public TopBar()
        {
            this.InitializeComponent();

            clipboardCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4),
            };
            clipboardCheckTimer.Tick += ClipboardCheckTimer_Tick;

            ShowExtraButtons = false;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            Window.Current.CoreWindow.Activated += Window_Activated;

            InitTitleBar();
        }


        private void ClipboardCheckTimer_Tick(object sender, object e)
        {
            // Ignore if not logged in
            if (!TokenHelper.HasTokens())
                return;

            CheckForSpotifyUriInClipboard();
        }

        private async void OpenLinkFromClipboard_Click(object sender, RoutedEventArgs e)
        {
            var uri = await ClipboardHelper.GetSpotifyUri();

            if (!string.IsNullOrWhiteSpace(uri))
            {
                OpenSpotifyUriRequested?.Invoke(this, uri);
            }
        }

        private void Window_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState != CoreWindowActivationState.Deactivated)
            {
                CheckForSpotifyUriInClipboard();
            }
        }

        private async void CheckForSpotifyUriInClipboard()
        {
            try
            {
                var isSpotifyUriPresent = await ClipboardHelper.IsSpotifyUriPresent();

                if (isSpotifyUriPresent && TokenHelper.HasTokens())
                {
                    ViewModel.OpenLinkFromClipboardVisibility = Visibility.Visible;
                }
                else
                {
                    ViewModel.OpenLinkFromClipboardVisibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                logger.Error("CheckForSpotifyUriInClipboard failed: " + ex.ToString());
            }
        }


        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarSize();
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarSize();
        }

        private void UpdateTitleBarSize()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;

            if (!coreTitleBar.IsVisible)
            {
                ViewModel.TopBarButtonWidth = 0.0;
                ViewModel.TopBarButtonHeight = 0.0;
                return;
            }

            var isTabletMode = (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch);

            ViewModel.TopBarButtonHeight = coreTitleBar.Height;
            ViewModel.TopBarButtonWidth = coreTitleBar.SystemOverlayRightInset / (isTabletMode ? 2.0 : 4.0);
        }

        public void InitTitleBar()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.SetTitleBar(topBarBackground);
            
            UpdateTitleBarColors(ThemeHelper.GetCurrentTheme());
        }

        public void UpdateTitleBarColors(Theme theme)
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (theme == Theme.Dark)
            {
                // Top bar buttons background color
                topBarButtonsAcrylicBrush.TintColor = Colors.Black;
                topBarButtonsAcrylicBrush.FallbackColor = Colors.Black;

                // Clipboard button style
                openLinkFromClipboard.Style = Application.Current.Resources["TopBarDarkButtonStyle"] as Style;

                // Set active window colors
                titleBar.ForegroundColor = Colors.White;
                titleBar.BackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.White;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Colors.White;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 100, 100, 100);
                titleBar.ButtonPressedForegroundColor = Colors.White;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 140, 140, 140);

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Color.FromArgb(255, 200, 200, 200);
                titleBar.InactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 200, 200, 200);
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            else
            {
                // Top bar buttons background color
                topBarButtonsAcrylicBrush.TintColor = Colors.White;
                topBarButtonsAcrylicBrush.FallbackColor = Colors.White;

                // Clipboard button style
                openLinkFromClipboard.Style = Application.Current.Resources["TopBarLightButtonStyle"] as Style;

                // Set active window colors
                titleBar.ForegroundColor = Colors.Black;
                titleBar.BackgroundColor = Colors.Transparent;
                titleBar.ButtonForegroundColor = Colors.Black;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonHoverForegroundColor = Colors.Black;
                titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 200, 200, 200);
                titleBar.ButtonPressedForegroundColor = Colors.Black;
                titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255, 160, 160, 160);

                // Set inactive window colors
                titleBar.InactiveForegroundColor = Color.FromArgb(255, 255 - 200, 255 - 200, 255 - 200);
                titleBar.InactiveBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, 255 - 200, 255 - 200, 255 - 200);
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
        }

    }
}
