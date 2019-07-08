using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;

namespace Xpotify.Helpers
{
    public class DeviceInfoHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static string GetDeviceName()
        {
            try
            {
                var hostNames = NetworkInformation.GetHostNames();
                var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
                var computerName = localName.DisplayName.Replace(".local", "");

                return computerName;
            }
            catch (Exception ex)
            {
                logger.Info("GetDeviceName via HostName failed: " + ex.ToString() + "\n Will try using EasClientDeviceInformation instead.");
                return (new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation()).FriendlyName;
            }
        }
    }
}
