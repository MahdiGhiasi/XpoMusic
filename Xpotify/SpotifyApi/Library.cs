using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.SpotifyApi
{
    public class Library : ApiBase
    {
        public async Task<bool> IsTrackSaved(string trackId)
        {
            return (await IsTrackSaved(new[] { trackId }))[0];
        }

        public async Task<bool[]> IsTrackSaved(string[] trackIds)
        {
            List<bool> output = new List<bool>();

            for (int i = 0; i < trackIds.Length; i += 50)
            {
                var slice = trackIds.Skip(i).Take(50).ToArray();
                output.AddRange(await IsTrackSavedInternal(slice));
            }

            return output.ToArray();
        }

        private async Task<bool[]> IsTrackSavedInternal(string[] trackIds)
        {
            var result = await SendRequestWithTokenAsync(
                "https://api.spotify.com/v1/me/tracks/contains?" +
                $"ids={string.Join(',', trackIds)}", HttpMethod.Get);

            var resultString = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<bool[]>(resultString);
        }

        public async Task<bool> SaveTrackToLibrary(string trackId)
        {
            var result = await SendRequestWithTokenAsync(
                "https://api.spotify.com/v1/me/tracks?" +
                $"ids={trackId}", HttpMethod.Put);

            return result.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveTrackFromLibrary(string trackId)
        {
            var result = await SendRequestWithTokenAsync(
                "https://api.spotify.com/v1/me/tracks?" +
                $"ids={trackId}", HttpMethod.Delete);

            return result.IsSuccessStatusCode;
        }
    }
}
