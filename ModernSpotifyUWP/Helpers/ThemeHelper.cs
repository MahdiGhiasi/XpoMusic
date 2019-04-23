using ModernSpotifyUWP.Classes.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    return "Light (Experimental)";
                default:
                    return "???";
            }
        }
    }
}
