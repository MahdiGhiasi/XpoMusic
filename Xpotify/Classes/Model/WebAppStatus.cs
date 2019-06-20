using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.Classes.Model
{
    public class WebAppStatus
    {
        public bool BackButtonEnabled { get; set; }
        public NowPlayingData NowPlaying { get; set; }
    }
}
