using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XpoMusic.Classes.Model;
using XpoMusic.SpotifyApi;

namespace XpoMusic.Classes.Cache
{
    public class SongExtraInfoStore : CacheStore<SongExtraInfo>
    {
        protected override async Task<SongExtraInfo> RetrieveItem(string key)
        {
            var library = new Library();
            var info = new SongExtraInfo
            {
                SongId = key,
                IsSavedToLibrary = await library.IsTrackSaved(key),
            };
            return info;
        }
    }
}
