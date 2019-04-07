using ModernSpotifyUWP.SpotifyApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace ModernSpotifyUWP.Helpers
{
    public static class UnauthorizedHelper
    {
        static readonly TimeSpan minimumNotifyDelay = TimeSpan.FromMinutes(3);

        static DateTime lastNotifyToUser = DateTime.MinValue;

        public static async void OnUnauthorizedError()
        {
            if (DateTime.UtcNow - lastNotifyToUser < minimumNotifyDelay)
                return;

            lastNotifyToUser = DateTime.UtcNow;

            TokenHelper.ClearTokens();

            var md = new MessageDialog("Please close and reopen Xpotify, and enter your credentials again. In " +
                "the meantime, some features might not work correctly", "Authorization Error");
            await md.ShowAsync();
        }
    }
}
