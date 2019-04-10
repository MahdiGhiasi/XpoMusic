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
    public class Album : ApiBase
    {
        public async Task<Model.AlbumSimplified> GetAlbum(string albumId)
        {
            StoreEventHelper.Log("api:getalbum");

            var result = await SendRequestWithTokenAsync($"https://api.spotify.com/v1/albums/{albumId}", HttpMethod.Get);
            var resultString = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Model.AlbumSimplified>(resultString);
        }

    }
}
