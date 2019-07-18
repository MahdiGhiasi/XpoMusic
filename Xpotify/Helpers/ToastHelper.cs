using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.Helpers
{
    public static class ToastHelper
    {
        public static void SendDebugToast(string title, string text)
        {
#if DEBUG
            // template to load for showing Toast Notification
            var xmlToastTemplate = "<toast launch=\"app-defined-string\">" +
                                     "<visual>" +
                                       "<binding template =\"ToastGeneric\">" +
                                         "<text>" + WebUtility.HtmlEncode(title) + "</text>" +
                                         "<text>" +
                                           WebUtility.HtmlEncode(text) +
                                         "</text>" +
                                       "</binding>" +
                                     "</visual>" +
                                   "</toast>";

            // load the template as XML document
            var xmlDocument = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDocument.LoadXml(xmlToastTemplate);

            // create the toast notification and show to user
            var toastNotification = new Windows.UI.Notifications.ToastNotification(xmlDocument);
            var notification = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notification.Show(toastNotification);
#endif
        }

        public static void SendReopenAppToast()
        {
            // template to load for showing Toast Notification
            var xmlToastTemplate = "<toast launch=\"action=reopenApp\">" +
                                     "<visual>" +
                                       "<binding template =\"ToastGeneric\">" +
                                         "<text>Tap here to open Xpo Music again</text>" +
                                       "</binding>" +
                                     "</visual>" +
                                   "</toast>";

            // load the template as XML document
            var xmlDocument = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDocument.LoadXml(xmlToastTemplate);

            // create the toast notification and show to user
            var toastNotification = new Windows.UI.Notifications.ToastNotification(xmlDocument);
            var notification = Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier();
            notification.Show(toastNotification);
        }
    }
}
