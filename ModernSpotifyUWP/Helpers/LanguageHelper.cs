using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xpotify.Classes.Model;

namespace Xpotify.Helpers
{
    public static class LanguageHelper
    {
        public static List<Language> GetLanguages()
        {
            var output = new List<Language>();

            var items = Enum.GetValues(typeof(Language));
            foreach (var item in items)
            {
                if ((Language)item == Language.Default)
                    continue;
                output.Add((Language)item);
            }

            output = output.OrderBy(x => GetLanguageName(x)).ToList();
            output.Insert(0, Language.Default);

            return output;
        }

        public static string GetLanguageName(Language language)
        {
            switch (language)
            {
                case Language.English:
                    return "English";
                case Language.Arabic:
                    return "Arabic";
                case Language.Hungarian:
                    return "Hungarian";
                case Language.Czech:
                    return "Czech";
                case Language.German:
                    return "German";
                case Language.Spanish:
                    return "Spanish";
                case Language.Finnish:
                    return "Finnish";
                case Language.French:
                    return "French";
                case Language.CanadianFrench:
                    return "French (Canadian)";
                case Language.Greek:
                    return "Greek";
                case Language.Indonesian:
                    return "Indonesian";
                case Language.Italian:
                    return "Italian";
                case Language.Japanese:
                    return "Japanese";
                case Language.Malay:
                    return "Malay";
                case Language.Dutch:
                    return "Dutch";
                case Language.Polish:
                    return "Polish";
                case Language.BrazillianPortuguese:
                    return "Portuguese (Brazil)";
                case Language.Swedish:
                    return "Swedish";
                case Language.Thai:
                    return "Thai";
                case Language.Turkish:
                    return "Turkish";
                case Language.Vietnamese:
                    return "Vietnamese";
                case Language.Default:
                default:
                    return "Auto detect";
            }
        }

        public static string GetHeaderLanguageString(Language language)
        {
            switch (language)
            {
                case Language.English:
                    return "en";
                case Language.Arabic:
                    return "ar";
                case Language.Hungarian:
                    return "hu";
                case Language.Czech:
                    return "cs";
                case Language.German:
                    return "de";
                case Language.Spanish:
                    return "es";
                case Language.Finnish:
                    return "fi";
                case Language.French:
                    return "fr";
                case Language.CanadianFrench:
                    return "fr-ca";
                case Language.Greek:
                    return "el";
                case Language.Indonesian:
                    return "id";
                case Language.Italian:
                    return "it";
                case Language.Japanese:
                    return "ja";
                case Language.Malay:
                    return "ms";
                case Language.Dutch:
                    return "nl";
                case Language.Polish:
                    return "pl";
                case Language.BrazillianPortuguese:
                    return "pt-br";
                case Language.Swedish:
                    return "sv";
                case Language.Thai:
                    return "th";
                case Language.Turkish:
                    return "tr";
                case Language.Vietnamese:
                    return "vi";
                case Language.Default:
                default:
                    return "";
            }
        }
    }
}
