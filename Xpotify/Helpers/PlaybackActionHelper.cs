using Xpotify.Classes;
using Xpotify.SpotifyApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.Helpers
{
    public static class PlaybackActionHelper
    {
        private static WebViewController _controller;

        public static void SetController(WebViewController controller)
        {
            _controller = controller;
        }

        public static async Task<bool> Play()
        {
            if (await _controller.Play())
            {
                await Task.Delay(100);
                return true;
            }

            return await (new Player()).ResumePlaying();
        }

        public static async Task<bool> Pause()
        {
            if (await _controller.Pause())
            {
                await Task.Delay(100);
                return true;
            }

            return await (new Player()).Pause();
        }

        public static async Task<bool> NextTrack()
        {
            // Progress bar on CompactOverlay should jump *immediately* to 0,
            // so the user get the feeling that their command was received.
            PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds = 0;

            if (await _controller.NextTrack())
            {
                await Task.Delay(100);
                return true;
            }

            return await (new Player()).NextTrack();
        }

        public static async Task<bool> PreviousTrack(bool canGoToBeginningOfCurrentSong = true)
        {
            // Progress bar on CompactOverlay should jump *immediately* to 0,
            // so the user get the feeling that their command was received.
            PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds = 0;

            if (!canGoToBeginningOfCurrentSong && await (new Player()).PreviousTrack())
            {
                // Prefer API call when not going to the beginning of the current song
                return true;
            }

            if (await _controller.PreviousTrack(canGoToBeginningOfCurrentSong))
            {
                await Task.Delay(100);
                return true;
            }

            return await (new Player()).PreviousTrack();
        }
    }
}
