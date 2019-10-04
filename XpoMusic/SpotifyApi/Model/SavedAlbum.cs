using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.SpotifyApi.Model
{
    public class SavedAlbum
    {
        public string added_at { get; set; }
        public AlbumSimplified album { get; set; }
    }
}
