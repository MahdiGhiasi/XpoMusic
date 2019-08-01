using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.Classes.Cache
{
    public class AlbumStore : CacheStore<SpotifyApi.Model.AlbumSimplified>
    {
        protected override Task<SpotifyApi.Model.AlbumSimplified> RetrieveItem(string key)
        {
            var album = new SpotifyApi.Album();
            return album.GetAlbum(key);
        }
    }
}
