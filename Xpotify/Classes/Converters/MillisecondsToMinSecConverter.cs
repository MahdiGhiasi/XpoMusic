using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace XpoMusic.Classes.Converters
{
    public class MillisecondsToMinSecConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var ms = (double)value;
            var t = TimeSpan.FromMilliseconds(ms);

            return $"{(int)Math.Floor(t.TotalMinutes)}:{t.Seconds.ToString("00")}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
