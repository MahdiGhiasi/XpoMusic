using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.SpotifyApi.Model
{
    public class Device
    {
        public string id;
        public bool is_active;
        public bool is_private_session;
        public bool is_restricted;
        public string name;
        public string type;
        public int volume_percent;
    }
}
