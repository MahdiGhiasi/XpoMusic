using Plugin.SecureStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.SpotifyApi
{
    public static class TokenHelper
    {
        const string accessTokenKey = "accessToken";
        const string refreshTokenKey = "refreshToken";

        public static async Task GetAndSaveNewTokenAsync()
        {
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", GetTokens().RefreshToken),
                new KeyValuePair<string, string>("client_id", Secrets.SpotifyClientId),
                new KeyValuePair<string, string>("client_secret", Secrets.SpotifyClientSecret),
            });

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", formContent);
            var json = await response.Content.ReadAsStringAsync();
            var results = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            if (!results.ContainsKey("access_token"))
                throw new UnauthorizedAccessException();

            var accessToken = results["access_token"].ToString();

            SaveTokens(accessToken, GetTokens().RefreshToken);
        }

        public static TokenResult GetTokens()
        {
            return new TokenResult
            {
                AccessToken = CrossSecureStorage.Current.GetValue(accessTokenKey),
                RefreshToken = CrossSecureStorage.Current.GetValue(refreshTokenKey),
            };
        }

        public static void SaveTokens(string newAccessToken, string newRefreshToken)
        {
            CrossSecureStorage.Current.SetValue(accessTokenKey, newAccessToken);
            CrossSecureStorage.Current.SetValue(refreshTokenKey, newRefreshToken);
        }

        public static bool HasTokens()
        {
            return CrossSecureStorage.Current.HasKey(accessTokenKey)
                && CrossSecureStorage.Current.HasKey(refreshTokenKey);
        }

    }

    public class TokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
