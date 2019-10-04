using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.SpotifyApi.Model
{
    public class Paging<T>
    {
        public string href { get; set; }
        public T[] items { get; set; }
        public int limit { get; set; }
        public int offset { get; set; }
        public string next { get; set; }
        public string previous { get; set; }
        public int total { get; set; }
        public bool hasNext => next != null;
        public bool hasPrev => previous != null;
    }
}
