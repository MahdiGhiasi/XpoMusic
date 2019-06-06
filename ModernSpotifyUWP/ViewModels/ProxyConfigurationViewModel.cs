using Xpotify.Classes;
using Xpotify.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Xpotify.ViewModels
{
    public class ProxyConfigurationViewModel : ViewModelBase
    {
        private bool customProxyEnabled = LocalConfiguration.IsCustomProxyEnabled;
        public bool IsCustomProxyEnabled
        {
            get => customProxyEnabled;
            set
            {
                customProxyEnabled = value;

                // But if true, it'll be set on Apply Settings button click.
                if (value == false)
                    LocalConfiguration.IsCustomProxyEnabled = false;

                FirePropertyChangedEvent(nameof(IsCustomProxyEnabled));
                FirePropertyChangedEvent(nameof(RestartNeededNoticeVisibility));
                FirePropertyChangedEvent(nameof(ProxyConfigurationVisibility));
                FirePropertyChangedEvent(nameof(SystemProxyNoticeVisibility));
            }
        }

        public Visibility RestartNeededNoticeVisibility => (ProxyHelper.IsCustomProxyEverEnabledInThisSession && !IsCustomProxyEnabled) ? Visibility.Visible : Visibility.Collapsed;
        public Visibility ProxyConfigurationVisibility => IsCustomProxyEnabled ? Visibility.Visible : Visibility.Collapsed;
        public Visibility SystemProxyNoticeVisibility => IsCustomProxyEnabled ? Visibility.Collapsed : Visibility.Visible;

        public ProxyHelper.ProxyType ProxyType { get; private set; } = LocalConfiguration.CustomProxyType;

        public bool IsSocksProxy
        {
            get => ProxyType == ProxyHelper.ProxyType.Socks;
            set
            {
                if (value)
                {
                    ProxyType = ProxyHelper.ProxyType.Socks;
                }
            }
        }

        public bool IsHttpProxy
        {
            get => ProxyType == ProxyHelper.ProxyType.HttpHttps;
            set
            {
                if (value)
                {
                    ProxyType = ProxyHelper.ProxyType.HttpHttps;
                }
            }
        }

        private string proxyAddress = LocalConfiguration.CustomProxyAddress;
        public string ProxyAddress
        {
            get => proxyAddress;
            set
            {
                if (value != ProxyAddress)
                {
                    LocalConfiguration.CustomProxyAddress = value;
                    FirePropertyChangedEvent(nameof(ProxyAddress));
                }
            }
        }

        private string proxyPort = LocalConfiguration.CustomProxyPort;
        public string ProxyPort
        {
            get => proxyPort;
            set
            {
                if (value != ProxyPort)
                {
                    proxyPort = value;
                    FirePropertyChangedEvent(nameof(ProxyPort));
                }
            }
        }
    }
}
