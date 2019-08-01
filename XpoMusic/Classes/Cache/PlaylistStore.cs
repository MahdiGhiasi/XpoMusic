using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.Classes.Cache
{
    public class PlaylistStore : CacheStore<SpotifyApi.Model.Playlist>
    {
        protected override Task<SpotifyApi.Model.Playlist> RetrieveItem(string key)
        {
            var playlist = new SpotifyApi.Playlist();
            return playlist.GetPlaylist(key);
        }
    }
}
