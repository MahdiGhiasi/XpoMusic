using GoogleAnalytics;
using Microsoft.Services.Store.Engagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.Helpers
{
    public static class AnalyticsHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static StoreServicesCustomEventLogger storeCustomEventLoggerObject = null;
        private static StoreServicesCustomEventLogger StoreCustomEventLogger
        {
            get
            {
                if (storeCustomEventLoggerObject == null)
                    storeCustomEventLoggerObject = StoreServicesCustomEventLogger.GetDefault();

                return storeCustomEventLoggerObject;
            }
        }

        private static Tracker googleAnalyticsTrackerObject = null;
        private static Tracker GoogleAnalyticsTracker
        {
            get
            {
                if (googleAnalyticsTrackerObject == null)
                {
                    AnalyticsManager.Current.IsDebug = false; //use only for debugging, returns detailed info on hits sent to analytics servers
                    AnalyticsManager.Current.DispatchPeriod = TimeSpan.Zero; //immediate mode, sends hits immediately
                    AnalyticsManager.Current.ReportUncaughtExceptions = true; //catch unhandled exceptions and send the details
                    AnalyticsManager.Current.AutoAppLifetimeMonitoring = true; //handle suspend/resume and empty hit batched hits on suspend

                    googleAnalyticsTrackerObject = AnalyticsManager.Current.CreateTracker(Secrets.GoogleAnalyticsTrackerId);
                }

                return googleAnalyticsTrackerObject;
            }
        }


        public static void Log(string eventName)
        {
#if !DEBUG
            StoreCustomEventLogger.Log(eventName);
            GoogleAnalyticsTracker.Send(HitBuilder.CreateCustomEvent(eventName, "").Build());

            logger.Info($"Analytics event '{eventName}' fired.");
#endif
        }

        public static void Log(string eventName, string action)
        {
#if !DEBUG
            StoreCustomEventLogger.Log(eventName + " : " + action);
            GoogleAnalyticsTracker.Send(HitBuilder.CreateCustomEvent(eventName, action).Build());

            logger.Info($"Analytics event '{eventName} : {action}' fired.");
#endif
        }

        public static void Log(string eventName, string action, string label)
        {
#if !DEBUG
            StoreCustomEventLogger.Log(eventName + " : " + action + " : " + label);
            GoogleAnalyticsTracker.Send(HitBuilder.CreateCustomEvent(eventName, action, label).Build());

            logger.Info($"Analytics event '{eventName} : {action} : {label}' fired.");
#endif
        }

        internal static void PageView(string pageName)
        {
#if !DEBUG
            GoogleAnalyticsTracker.Send(HitBuilderEx.CreatePageView("/" + pageName));

            logger.Info($"PageView '{pageName}' fired.");
#endif
        }
    }
}
