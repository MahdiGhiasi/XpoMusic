using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModernSpotifyUWP.Classes.Model;

namespace ModernSpotifyUWP.Helpers
{
    public static class LanguageHelper
    {
        public static string GetHeaderLanguageString(Language language)
        {
            switch (language)
            {
                case Language.Default:
                    return "";
                case Language.English:
                    return "en";
                default:
                    return "";
            }
        }
    }
}
