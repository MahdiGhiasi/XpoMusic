using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.Helpers
{
    public static class StoreEventHelper
    {
        private static StoreServicesCustomEventLogger customEventLogger = null;

        public static void Log(string eventName)
        {
#if !DEBUG
            if (customEventLogger == null)
                customEventLogger = StoreServicesCustomEventLogger.GetDefault();

            customEventLogger.Log(eventName);
#endif
        }
    }
}
