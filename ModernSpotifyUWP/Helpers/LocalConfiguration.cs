using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace ModernSpotifyUWP.Helpers
{
    public static class LocalConfiguration
    {
        private static readonly Size compactOverlayDefaultSize = new Size(300, 300);

        public static Size CompactOverlaySize
        {
            get
            {
                var config = GetConfiguration("CompactOverlaySize");

                if (string.IsNullOrEmpty(config))
                    return compactOverlayDefaultSize;

                try
                {
                    var configParts = config.Split(';');
                    return new Size(double.Parse(configParts[0]), double.Parse(configParts[1]));
                }
                catch
                {
                    return compactOverlayDefaultSize;
                }
            }

            set
            {
                SetConfiguration("CompactOverlaySize", $"{value.Width};{value.Height}");
            }
        }

        public static Size WindowMinSize => new Size(500, 500);

        private static string GetConfiguration(string key)
        {
            var completeKey = "LocalConfiguration_" + key;
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(completeKey))
                return null;

            return ApplicationData.Current.LocalSettings.Values[completeKey].ToString();
        }

        private static void SetConfiguration(string key, string value)
        {
            var completeKey = "LocalConfiguration_" + key;
            ApplicationData.Current.LocalSettings.Values[completeKey] = value;
        }
    }
}
