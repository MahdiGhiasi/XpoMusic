using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.Classes.Model.LyricsViewer
{
    public class CurrentPlayingSongInfo
    {
        public string SongName { get; set; }
        public string ArtistName { get; set; }
        public string AlbumName { get; set; }
        public string AlbumArtUri { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is CurrentPlayingSongInfo cpsi)
            {
                return SongName == cpsi.SongName
                    && ArtistName == cpsi.ArtistName
                    && AlbumArtUri == cpsi.AlbumArtUri
                    && AlbumName == cpsi.AlbumName;
            }

            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = 2090074316;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SongName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ArtistName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AlbumName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AlbumArtUri);
            return hashCode;
        }
    }
}
