using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Xpotify.Helpers
{
    public static class SpotifyShareUriHelper
    {
        public static string GetPwaUri(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return "";

            uri = Regex.Replace(uri, "http\\://", "https://", RegexOptions.IgnoreCase);

            var uriLowerCase = uri.ToLower();

            if (uriLowerCase.StartsWith("xpotify:https://"))
            {
                uri = uri.Substring("xpotify:".Length);
                uriLowerCase = uri.ToLower();
            }
            else if (uriLowerCase.StartsWith("xpotify:"))
            {
                uri = Regex.Replace(uri, "^xpotify\\:", "spotify:", RegexOptions.IgnoreCase);
                uriLowerCase = uri.ToLower();
            }
            else if (uriLowerCase.StartsWith("spotify:nl:"))
            {
                uri = ParseNlUri(uri.Substring("spotify:nl:".Length));
                uriLowerCase = uri.ToLower();
            }

            if (uriLowerCase.StartsWith(WebViewController.SpotifyPwaUrlBeginsWith))
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
                else if (uriLowerCase.Contains("spotify:dailymix:"))
                {
                    var idx = uriLowerCase.IndexOf("spotify:dailymix:") + "spotify:dailymix:".Length;
                    return "https://open.spotify.com/dailymix/" + uri.Substring(idx);
                }
            }

            return "";
        }

        private static string ParseNlUri(string base64Code)
        {
            var code = Encoding.UTF8.GetString(Convert.FromBase64String(base64Code));
            var code2 = new string(code.Where(x => char.IsLetterOrDigit(x) || x == ':').ToArray());

            string type;
            if (code2.Contains("album:"))
                type = "album:";
            else if (code2.Contains("playlist:"))
                type = "playlist:";
            else if (code2.Contains("dailymix:"))
                type = "dailymix:";
            else if (code2.Contains("track:"))
                type = "track:";
            else
                return "";

            var code3 = code2.Substring(code2.IndexOf(type)).Trim();

            return $"spotify:{code3}";
        }
    }
}
