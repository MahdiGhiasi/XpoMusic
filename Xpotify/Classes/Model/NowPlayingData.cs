using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.Classes.Model
{
    public class NowPlayingData
    {
        public string TrackName { get; set; }
        public string TrackId { get; set; }
        public string AlbumId { get; set; }
        public string ArtistName { get; set; }
        public string ArtistId { get; set; }
        public string TrackFingerprint { get; set; }
        public double Volume { get; set; }
        public int ElapsedTime { get; set; }
        public int TotalTime { get; set; }
        public bool IsPrevTrackAvailable { get; set; }
        public bool IsNextTrackAvailable { get; set; }
        public bool IsPlaying { get; set; }
        public bool IsTrackSavedToLibrary { get; set; }
        public bool Success { get; set; }
    }
}
