using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.Classes.Cache
{
    public static class GlobalCache
    {
        public static ArtistStore Artist { get; } = new ArtistStore();
        public static AlbumStore Album { get; } = new AlbumStore();
        public static PlaylistStore Playlist { get; } = new PlaylistStore();
    }
}
