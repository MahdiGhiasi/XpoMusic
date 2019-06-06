using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.XpotifyApi.Model
{
    public class DeveloperMessage
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public long timestamp { get; set; }
        public DateTime messageDate => DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime.ToLocalTime();
    }
}
