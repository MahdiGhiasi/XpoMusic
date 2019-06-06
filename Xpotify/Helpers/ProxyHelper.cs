using Xpotify.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.Helpers
{
    public static class ProxyHelper
    {
        public enum ProxyType
        {
            HttpHttps = 1,
            Socks = 2,
        }

        public static bool IsCustomProxyEverEnabledInThisSession { get; private set; } = false;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal static void SetSocksProxyInProcess(string address, string port)
        {
            SetProxyInProcess($"socks={address}:{port}", "local");
        }

        internal static void SetHttpHttpsProxyInProcess(string address, string port)
        {
            SetProxyInProcess($"http={address}:{port};https={address}:{port}", "local");
        }

        public static void SetProxyInProcess(string proxy, string proxyBypass)
        {
            IsCustomProxyEverEnabledInThisSession = true;

            var proxyInfo = new INTERNET_PROXY_INFO
            {
                dwAccessType = INTERNET_OPEN_TYPE.INTERNET_OPEN_TYPE_PROXY,
                lpszProxy = proxy,
                lpszProxyBypass = proxyBypass,
            };
            var dwBufferLength = (uint)Marshal.SizeOf(proxyInfo);
            var result = UrlMkSetSessionOption(INTERNET_OPTION.INTERNET_OPTION_PROXY, proxyInfo, dwBufferLength, 0U);

            if (result != 0)
            {
                var errorCode = Marshal.GetLastWin32Error();
                logger.Warn($"UrlMkSetSessionOption failed with error code {errorCode}.");
            }
        }

        internal static void ApplyProxySettings()
        {
            if (!LocalConfiguration.IsCustomProxyEnabled)
                return;

            if (string.IsNullOrWhiteSpace(LocalConfiguration.CustomProxyAddress))
                return;
            if (string.IsNullOrWhiteSpace(LocalConfiguration.CustomProxyPort))
                return;

            if (LocalConfiguration.CustomProxyType == ProxyType.HttpHttps)
                SetHttpHttpsProxyInProcess(LocalConfiguration.CustomProxyAddress, LocalConfiguration.CustomProxyPort);
            else
                SetSocksProxyInProcess(LocalConfiguration.CustomProxyAddress, LocalConfiguration.CustomProxyPort);
        }

        internal enum INTERNET_OPTION
        {
            INTERNET_OPTION_REFRESH = 37,
            INTERNET_OPTION_PROXY = 38,
            INTERNET_OPTION_SETTINGS_CHANGED = 39,
            INTERNET_OPTION_PER_CONNECTION_OPTION = 75,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class INTERNET_PROXY_INFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public INTERNET_OPEN_TYPE dwAccessType;

            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszProxy;

            [MarshalAs(UnmanagedType.LPStr)]
            public string lpszProxyBypass;
        }

        internal enum INTERNET_OPEN_TYPE
        {
            INTERNET_OPEN_TYPE_PRECONFIG = 0,
            INTERNET_OPEN_TYPE_DIRECT = 1,
            INTERNET_OPEN_TYPE_PROXY = 3,
            INTERNET_OPEN_TYPE_PRECONFIG_WITH_NO_AUTOPROXY = 4,
        }

        [DllImport("urlmon.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int UrlMkSetSessionOption(
            INTERNET_OPTION dwOption,
            INTERNET_PROXY_INFO pBuffer,
            uint dwBufferLength,
            uint dwReserved);
    }
}
