using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace XpoMusicInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string appInstallerUri = "ms-appinstaller:?source=https://ghiasi.net/XpoMusic/install/XpoMusic.appinstaller";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            (this.FindResource("mainLogoStoryboard") as Storyboard).Begin();
            StartInstallation();
        }

        private async void StartInstallation()
        {
            if (!VerifyOSVersion())
                return;
            if (!InstallCertificate())
                return;
            OpenAppInstaller();
            (this.FindResource("mainLogoExitStoryboard") as Storyboard).Begin();
            await Task.Delay(1500);
            Application.Current.Shutdown();
        }

        private static void ShowOSVersionAlertAndExit()
        {
            MessageBox.Show("To install Xpo Music, you must first update your operating system to Windows 10 version 1803 or above.", "Xpo Music Installer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Application.Current.Shutdown();
        }

        private static void ShowPermissionAlertAndExit()
        {
            MessageBox.Show("Please run Xpo Music Installer as an administrator.", "Xpo Music Installer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            Application.Current.Shutdown();
        }

        private bool VerifyOSVersion()
        {
            var osVersion = Environment.OSVersion.Version;
            var minSupportedVersion = new Version("10.0.17134.0");
            return osVersion >= minSupportedVersion;
        }

        private void OpenAppInstaller()
        {
            Process.Start(appInstallerUri);
        }

        private bool InstallCertificate()
        {
            try
            {
                var cert = new X509Certificate2(Properties.Resources.XpoMusic);
                X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadWrite);
                store.Add(cert);
                store.Close();
                return true;
            }
            catch (CryptographicException)
            {
                ShowPermissionAlertAndExit();
                return false;
            }
        }
    }
}
