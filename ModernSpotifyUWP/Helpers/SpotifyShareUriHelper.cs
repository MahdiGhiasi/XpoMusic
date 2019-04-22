using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.Helpers
{
    public static class SpotifyShareUriHelper
    {
        public static string GetPwaUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return "";

            uri = uri.Replace("http://", "https://");

            var uriLowerCase = uri.ToLower();

            if (uriLowerCase.StartsWith(WebViewHelper.SpotifyPwaUrlBeginsWith))
                return uri;

            if (uriLowerCase.StartsWith("spotify:"))
            {
                if (uriLowerCase.Contains("spotify:artist:"))
                {
                    var idx = uriLowerCase.IndexOf("spotify:artist:") + "spotify:artist:".Length;
                    return "https://open.spotify.com/artist/" + uri.Substring(idx);
                }
                else if (uriLowerCase.Contains("spotify:album:"))
                {
                    var idx = uriLowerCase.IndexOf("spotify:album:") + "spotify:album:".Length;
                    return "https://open.spotify.com/album/" + uri.Substring(idx);
                }
                else if (uriLowerCase.Contains("spotify:playlist:"))
                {
                    var idx = uriLowerCase.IndexOf("spotify:playlist:") + "spotify:playlist:".Length;
                    return "https://open.spotify.com/playlist/" + uri.Substring(idx);
                }
                else if (uriLowerCase.Contains("spotify:track:"))
                {
                    var idx = uriLowerCase.IndexOf("spotify:track:") + "spotify:track:".Length;
                    return "https://open.spotify.com/track/" + uri.Substring(idx);
                }
            }

            return "";
        }
    }
}
