using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Classes.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ModernSpotifyUWP.Helpers
{
    public static class ThemeHelper
    {
        public static List<Theme> GetThemes()
        {
            var output = new List<Theme>();

            var items = Enum.GetValues(typeof(Theme));
            foreach (var item in items)
            {
                output.Add((Theme)item);
            }

            output = output.OrderBy(x => GetThemeName(x)).ToList();

            return output;
        }

        public static string GetThemeName(Theme theme)
        {
            switch (theme)
            {
                case Theme.Dark:
                    return "Dark";
                case Theme.Light:
                    return "Light";
                case Theme.System:
                    return "Use my Windows mode";
                default:
                    return "???";
            }
        }

        internal static Theme GetCurrentTheme()
        {
            if (LocalConfiguration.Theme != Theme.System)
                return LocalConfiguration.Theme;

            return (Application.Current.RequestedTheme == ApplicationTheme.Dark) ? Theme.Dark : Theme.Light;
        }
    }
}
