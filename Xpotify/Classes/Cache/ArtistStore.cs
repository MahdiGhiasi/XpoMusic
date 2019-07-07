using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.Classes.Cache
{
    public class ArtistStore : CacheStore<SpotifyApi.Model.Artist>
    {
        protected override Task<SpotifyApi.Model.Artist> RetrieveItem(string key)
        {
            var artist = new SpotifyApi.Artist();
            return artist.GetArtist(key);
        }
    }
}
