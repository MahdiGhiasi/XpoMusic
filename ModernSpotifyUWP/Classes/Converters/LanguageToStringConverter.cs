using ModernSpotifyUWP.Classes.Model;
using ModernSpotifyUWP.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ModernSpotifyUWP.Classes.Converters
{
    public class LanguageToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var lang = (Language)value;

            return LanguageHelper.GetLanguageName(lang);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
