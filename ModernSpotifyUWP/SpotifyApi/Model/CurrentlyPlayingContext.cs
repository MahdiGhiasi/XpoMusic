using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.SpotifyApi.Model
{
    public class CurrentlyPlayingContext
    {
        public int progress_ms;
        public bool is_playing;
        public string currently_playing_type;
        public Track item;
    }
}
