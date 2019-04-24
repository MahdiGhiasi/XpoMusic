using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ModernSpotifyUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DonatePage : Page
    {
        private string bitcoinWalletAddress = "38TmLnUjix9NiPpiFoKV7qAjNeqaSi1EJH";
        private string ethereumWalletAddress = "0x8c8d6a7f0c4e2da49bf9e249fdb349ffde884a00";

        public DonatePage()
        {
            this.InitializeComponent();

            bitcoinWallet.Text = bitcoinWalletAddress;
            ethereumWallet.Text = ethereumWalletAddress;
        }

        private async void CopyBitcoinAddressToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CopyTextToClipboard(bitcoinWalletAddress);

            bitcoinAddressCopiedCheckMark.Visibility = Visibility.Visible;
            await Task.Delay(TimeSpan.FromSeconds(3));
            bitcoinAddressCopiedCheckMark.Visibility = Visibility.Collapsed;
        }

        private async void CopyEthereumAddressToClipboard_Click(object sender, RoutedEventArgs e)
        {
            CopyTextToClipboard(ethereumWalletAddress);

            ethereumAddressCopiedCheckMark.Visibility = Visibility.Visible;
            await Task.Delay(TimeSpan.FromSeconds(3));
            ethereumAddressCopiedCheckMark.Visibility = Visibility.Collapsed;
        }

        private void CopyTextToClipboard(string text)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
            Clipboard.Flush();
        }
    }
}
