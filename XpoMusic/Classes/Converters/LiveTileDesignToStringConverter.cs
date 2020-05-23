using XpoMusic.Classes.Model;
using XpoMusic.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;
using static XpoMusic.Helpers.LiveTileHelper;

namespace XpoMusic.Classes.Converters
{
    public class LiveTileDesignToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var design = (LiveTileDesign)value;

            switch (design)
            {
                case LiveTileDesign.AlbumArtOnly:
                    return "Album art only";
                case LiveTileDesign.ArtistArtOnly:
                    return "Artist art only";
                case LiveTileDesign.Disabled:
                    return "Disabled";
                case LiveTileDesign.AlbumAndArtistArt:
                    return "Album art and artist art";
                default:
                    return "???";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
