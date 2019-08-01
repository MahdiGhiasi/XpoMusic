using XpoMusic.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using XpoMusic.Pages;

namespace XpoMusic
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;
        }

        private async void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            AnalyticsHelper.Log("unhandledException", e.Message, e.Exception.ToString());
            await new MessageDialog(e.Message + "\r\n---\r\n" + e.Exception.ToString(), "An exception has been occured in Xpo Music.").ShowAsync();
            logger.Error("Unhandled exception: \r\n" + e.Message + "\r\n---\r\n" + e.Exception.ToString());
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = InitRootFrame(e.PreviousExecutionState);

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                else if (!string.IsNullOrEmpty(e.Arguments))
                {
                    // If app is already opened but a secondary tile is clicked,

                    var currentMainPage = (rootFrame.Content as MainPage);
                    if (currentMainPage == null)
                    {
                        // if MainPage is not opened, navigate to MainPage with the necessary argument

                        rootFrame.Navigate(typeof(MainPage), e.Arguments);
                    }
                    else
                    {
                        // if MainPage is opened already, send a signal to it so it can navigate to the new url.

                        currentMainPage.NavigateToSecondaryTile(e.Arguments);
                    }
                }

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs e)
        {
            Frame rootFrame = InitRootFrame(e.PreviousExecutionState);

            // Handle toast activation
            if (e.Kind == ActivationKind.ToastNotification)
            {
                var toastActivationArgs = e as ToastNotificationActivatedEventArgs;

                logger.Info($"ToastActivation with argument '{toastActivationArgs.Argument}'.");

                // Parse the query string (using QueryString.NET)
                var args = new WwwFormUrlDecoder(toastActivationArgs.Argument);

                // See what action is being requested 
                switch (args.GetFirstValueByName("action"))
                {
                    // Open the image
                    case "reopenApp":
                        if (rootFrame.Content == null)
                        {
                            rootFrame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
                        }
                        break;
                }
            }
            else if (e.Kind == ActivationKind.Protocol)
            {
                var args = e as ProtocolActivatedEventArgs;
                var uri = args.Uri.ToString();
                var currentMainPage = (rootFrame.Content as MainPage);

                logger.Info($"ProtocolActivation with uri '{uri}'.");

                var targetPwaUri = SpotifyShareUriHelper.GetPwaUri(uri);

                if (string.IsNullOrWhiteSpace(targetPwaUri))
                {
                    // Invalid uri, will launch default page

                    if (currentMainPage == null)
                    {
                        // if MainPage is not opened, navigate to MainPage with the necessary argument
                        rootFrame.Navigate(typeof(MainPage), null, new SuppressNavigationTransitionInfo());
                    }
                }
                else
                {
                    var argument = "pageUrl=" + WebUtility.UrlEncode(targetPwaUri);

                    if (uri.ToLower().StartsWith("spotify:nl:"))
                    {
                        var autoplay = targetPwaUri.ToLower().Contains("/track/") ? "track" : "playlist";
                        argument += "&autoplay=" + autoplay;
                        argument += "&source=cortana";
                    }

                    if (currentMainPage == null)
                    {
                        // if MainPage is not opened, navigate to MainPage with the necessary argument
                        rootFrame.Navigate(typeof(MainPage), argument);
                    }
                    else
                    {
                        // if MainPage is opened already, send a signal to it so it can navigate to the new url.
                        currentMainPage.NavigateWithConfig(argument);
                    }
                }
            }

            // TODO: Handle other types of activation

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private Frame InitRootFrame(ApplicationExecutionState previousExecutionState)
        {
            // Get the root frame
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (previousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            return rootFrame;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
