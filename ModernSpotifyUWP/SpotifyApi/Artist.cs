using ModernSpotifyUWP.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.SpotifyApi
{
    public class Artist : ApiBase
    {
        public async Task<Model.Artist> GetArtist(string artistId)
        {
            AnalyticsHelper.Log("api", "getartist");

            var result = await SendRequestWithTokenAsync($"https://api.spotify.com/v1/artists/{artistId}", HttpMethod.Get);
            var resultString = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Model.Artist>(resultString);
        }

    }
}
