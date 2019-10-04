using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.SpotifyApi.Model
{
    public class FollowedArtistsResponse
    {
        public Paging<Artist> artists { get; set; }
    }
}
