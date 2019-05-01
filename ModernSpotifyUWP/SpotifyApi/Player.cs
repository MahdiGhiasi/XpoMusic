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
                AnalyticsHelper.Log("api", "me/getplayer::" + result.StatusCode.ToString());

            return JsonConvert.DeserializeObject<CurrentlyPlayingContext>(resultString);
        }

        public async Task<bool> NextTrack()
        {
            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/next", HttpMethod.Post);

            if (result.IsSuccessStatusCode == false)
                AnalyticsHelper.Log("api", "me/player/next::" + result.StatusCode.ToString());

            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<bool> PreviousTrack()
        {
            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/previous", HttpMethod.Post);

            if (result.IsSuccessStatusCode == false)
                AnalyticsHelper.Log("api", "me/player/previous::" + result.StatusCode.ToString());

            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<bool> ResumePlaying()
        {
            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/play", HttpMethod.Put);

            if (result.IsSuccessStatusCode == false)
                AnalyticsHelper.Log("api", "me/player/play::" + result.StatusCode.ToString());

            if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                logger.Info("ResumePlaying api returned NotFound.");

                var resultText = await result.Content.ReadAsStringAsync();
                if (resultText.Contains("NO_ACTIVE_DEVICE"))
                {
                    logger.Info("ResumePlaying api content included NO_ACTIVE_DEVICE. The complete text was:\n" + resultText);
                    logger.Info("Will try to mark current device as active.");

                    var devices = await GetDevices();
                    var thisDevice = devices.devices.FirstOrDefault(x => x.name.Contains("Edge") && x.name.Contains("Web"));

                    if (thisDevice != null)
                    {
                        logger.Info("Guessed " + thisDevice.name + " to be the current device. Will try to transfer playback to it.");
                        var transferResult = await TransferPlayback(thisDevice.id, ensurePlayback: true);

                        logger.Info("Transfer playback to " + thisDevice.name + " " + (transferResult ? "succeded." : "failed."));

                        return transferResult;
                    }
                }
            }

            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<bool> Pause()
        {
            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/pause", HttpMethod.Put);

            if (result.IsSuccessStatusCode == false)
                AnalyticsHelper.Log("api", "me/player/pause::" + result.StatusCode.ToString());

            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<Devices> GetDevices()
        {
            var result = await SendRequestWithTokenAsync("https://api.spotify.com/v1/me/player/devices", HttpMethod.Get);
            var resultString = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode == false)
                AnalyticsHelper.Log("api", "me/player/getdevices::" + result.StatusCode.ToString());

            return JsonConvert.DeserializeObject<Devices>(resultString);
        }

        public async Task<bool> TransferPlayback(string deviceId, bool ensurePlayback)
        {
            var ensurePlaybackString = ensurePlayback ? "true" : "false";
            var data = $"{{\"device_ids\":[\"{deviceId}\"], \"play\": \"{ensurePlaybackString}\"}}";

            var result = await SendJsonRequestWithTokenAsync("https://api.spotify.com/v1/me/player", HttpMethod.Put, data);

            if (result.IsSuccessStatusCode == false)
                AnalyticsHelper.Log("api", "me/player:transferplayback::" + result.StatusCode.ToString());

            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }

        public async Task<bool> SetVolume(string deviceId, double volume)
        {
            var volume_percent = (int)Math.Round(volume * 100.0);

            var result = await SendRequestWithTokenAsync($"https://api.spotify.com/v1/me/player/volume?volume_percent={volume_percent}&device_id={deviceId}", HttpMethod.Put);

            if (result.IsSuccessStatusCode == false)
                AnalyticsHelper.Log("api", "me/player:setvolume::" + result.StatusCode.ToString());

            return (result.StatusCode == System.Net.HttpStatusCode.NoContent);
        }
    }
}