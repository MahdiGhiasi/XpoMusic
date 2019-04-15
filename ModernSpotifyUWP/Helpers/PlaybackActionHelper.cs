using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.SpotifyApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.Helpers
{
    public static class PlaybackActionHelper
    {
        public static async Task<bool> Play()
        {
            if (await WebViewInjectionHandler.Play())
            {
                await Task.Delay(100);
                return true;
            }

            return await (new Player()).ResumePlaying();
        }

        public static async Task<bool> Pause()
        {
            if (await WebViewInjectionHandler.Pause())
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

            if (await WebViewInjectionHandler.NextTrack())
            {
                await Task.Delay(100);
                return true;
            }

            return await (new Player()).NextTrack();
        }

        public static async Task<bool> PreviousTrack()
        {
            // Progress bar on CompactOverlay should jump *immediately* to 0,
            // so the user get the feeling that their command was received.
            PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds = 0;

            if (await WebViewInjectionHandler.PreviousTrack())
            {
                await Task.Delay(100);
                return true;
            }

            return await (new Player()).PreviousTrack();
        }
    }
}
