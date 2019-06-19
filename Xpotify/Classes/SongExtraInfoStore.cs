using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xpotify.Classes.Model;
using Xpotify.SpotifyApi;

namespace Xpotify.Classes
{
    public class SongExtraInfoStore
    {
        private Dictionary<string, SongExtraInfo> data = new Dictionary<string, SongExtraInfo>();

        public void Clear()
        {
            data.Clear();
        }

        public async Task<SongExtraInfo> GetSongExtraInfo(string songId)
        {
            if (!data.ContainsKey(songId))
            {
                var library = new Library();
                var info = new SongExtraInfo
                {
                    SongId = songId,
                    IsSavedToLibrary = await library.IsTrackSaved(songId),
                };
                data[songId] = info;
            }

            return data[songId];
        }
    }
}
