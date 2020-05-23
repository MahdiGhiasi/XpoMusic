﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XpoMusic.Classes;

namespace XpoMusic.Helpers
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
            else if (uriLowerCase.StartsWith("xpomusic:https://"))
            {
                uri = uri.Substring("xpomusic:".Length);
                uriLowerCase = uri.ToLower();
            }
            else if (uriLowerCase.StartsWith("xpotify:"))
            {
                uri = Regex.Replace(uri, "^xpotify\\:", "spotify:", RegexOptions.IgnoreCase);
                uriLowerCase = uri.ToLower();
            }
            else if (uriLowerCase.StartsWith("xpomusic:"))
            {
                uri = Regex.Replace(uri, "^xpomusic\\:", "spotify:", RegexOptions.IgnoreCase);
                uriLowerCase = uri.ToLower();
            }
            else if (uriLowerCase.StartsWith("spotify:nl:"))
            {
                uri = ParseNlUri(uri.Substring("spotify:nl:".Length));
                uriLowerCase = uri.ToLower();
            }

            if (uriLowerCase.StartsWith(WebViewController.SpotifyPwaUrlHome))
                return uri;


            var spotifyUriMatch = Regex.Match(uriLowerCase, "^spotify:[A-Za-z]+:");
            if (spotifyUriMatch.Success)
            {
                var categoryTerm = spotifyUriMatch.ToString();
                var category = categoryTerm.Split(':')[1].ToLower();
                var idx = uriLowerCase.IndexOf(categoryTerm) + categoryTerm.Length;
                return "https://open.spotify.com/" + category + "/" + uri.Substring(idx);
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
            else if (code2.Contains("artist:"))
                type = "artist:";
            else if (code2.Contains("show:"))
                type = "show:";
            else
                return "";

            var code3 = code2.Substring(code2.IndexOf(type)).Trim();

            return $"spotify:{code3}";
        }
    }
}
