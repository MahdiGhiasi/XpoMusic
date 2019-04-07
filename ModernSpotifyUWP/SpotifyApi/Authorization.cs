using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.SpotifyApi
{
    public static class Authorization
    {
        internal static readonly string Scopes = "user-read-recently-played playlist-read-private user-read-email user-library-read user-read-playback-state user-read-private user-modify-playback-state user-read-currently-playing";
        internal static readonly string RedirectUri = "https://xpotify.ghiasi.net/login/redirect";

        public static string GetAuthorizationUrl(string state)
        {
            return "https://accounts.spotify.com/authorize?"
                + $"client_id={WebUtility.UrlEncode(Secrets.SpotifyClientId)}&"
                + $"response_type=code&"
                + $"redirect_uri={WebUtility.UrlEncode(RedirectUri)}&"
                + $"state={WebUtility.UrlEncode(state)}&"
                + $"scope={WebUtility.UrlEncode(Scopes)}&"
                + $"show_dialog=false";
        }

        public static async Task RetrieveAndSaveTokensFromAuthCode(string code)
        {
            var httpClient = new HttpClient();

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            msg.Headers.Clear();
            msg.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[] 
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                new KeyValuePair<string, string>("client_id", Secrets.SpotifyClientId),
                new KeyValuePair<string, string>("client_secret", Secrets.SpotifyClientSecret),
            });

            var response = await httpClient.SendAsync(msg);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);

            var accessToken = responseData["access_token"].ToString();
            var refreshToken = responseData["refresh_token"].ToString();

            TokenHelper.SaveTokens(accessToken, refreshToken);
        }
    }
}
