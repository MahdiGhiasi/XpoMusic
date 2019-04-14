using ModernSpotifyUWP.Helpers;
using ModernSpotifyUWP.SpotifyApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.SpotifyApi
{
    public class Player : ApiBase
    {
        public async Task<CurrentlyPlayingContext> GetCurrentlyPlaying()
        {
            //AnalyticsHelper.Log("api:me/getplayer");

            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player", HttpMethod.Get);
            var resultString = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode == false)
                AnalyticsHelper.Log("api:me/getplayer::" + result.StatusCode.ToString());

            return JsonConvert.DeserializeObject<CurrentlyPlayingContext>(resultString);
        }

        public async Task<bool> NextTrack()
        {
            AnalyticsHelper.Log("api:me/player/next");

            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/next", HttpMethod.Post);
            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<bool> PreviousTrack()
        {
            AnalyticsHelper.Log("api:me/player/previous");

            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/previous", HttpMethod.Post);
            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<bool> ResumePlaying()
        {
            AnalyticsHelper.Log("api:me/player/play");

            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/play", HttpMethod.Put);

            if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                var resultText = await result.Content.ReadAsStringAsync();
                if (resultText.Contains("NO_ACTIVE_DEVICE"))
                {
                    var devices = await GetDevices();
                    var thisDevice = devices.devices.FirstOrDefault(x => x.name.Contains("Edge") && x.name.Contains("Web"));

                    if (thisDevice != null)
                    {
                        var transferResult = await TransferPlayback(thisDevice.id, ensurePlayback: true);
                        return transferResult;
                    }
                }
            }

            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<bool> Pause()
        {
            AnalyticsHelper.Log("api:me/player/pause");

            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/pause", HttpMethod.Put);
            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<Devices> GetDevices()
        {
            AnalyticsHelper.Log("api:me/player/getdevices");

            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/devices", HttpMethod.Get);
            var resultString = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Devices>(resultString);
        }

        public async Task<bool> TransferPlayback(string deviceId, bool ensurePlayback)
        {
            AnalyticsHelper.Log("api:me/player:transferplayback");

            var ensurePlaybackString = ensurePlayback ? "true" : "false";
            var data = $"{{\"device_ids\":[\"{deviceId}\"], \"play\": \"{ensurePlaybackString}\"}}";

            var result = await SendJsonRequestWithTokenAsync("https://api.spotify.com/v1/me/player", HttpMethod.Put, data);
            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<bool> SetVolume(string deviceId, double volume)
        {
            AnalyticsHelper.Log("api:me/player:setvolume");

            var volume_percent = (int)Math.Round(volume * 100.0);

            var result = await SendRequestWithTokenAsync($"https://api.spotify.com/v1/me/player/volume?volume_percent={volume_percent}&device_id={deviceId}", HttpMethod.Put);
            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }
    }
}