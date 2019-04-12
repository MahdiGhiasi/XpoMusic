using GoogleAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.Helpers
{
    public static class HitBuilderEx
    {
        private const string HitType_Pageview = "pageview";

        public static IDictionary<string, string> CreatePageView(string pageName = null, string title = null)
        {
            var data = new Dictionary<string, string>
            {
                { "t", HitType_Pageview },
            };

            if (pageName != null) data.Add("dp", pageName);
            if (title != null) data.Add("dt", title);

            return data;
        }

    }
}
