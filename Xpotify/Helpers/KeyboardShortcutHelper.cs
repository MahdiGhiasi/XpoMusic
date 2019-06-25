using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Xpotify.Classes;

namespace Xpotify.Helpers
{
    public static class KeyboardShortcutHelper
    {
        public enum KeyDownProcessResult
        {
            Handled,
            AskJs,
            GoBack,
        };

        public static async Task<KeyDownProcessResult> KeyDown(VirtualKey key, bool shiftPressed, bool ctrlPressed, bool altPressed, WebViewController controller, Controls.NowPlayingView nowPlaying)
        {
            if (key == VirtualKey.Space)
            {
                // Play/Pause
                await controller.PlayPause();
            }
            else if (key == VirtualKey.Right && ctrlPressed)
            {
                // Next Track
                await controller.NextTrack();
                if (nowPlaying.IsOpen)
                    nowPlaying.PlayChangeTrackAnimation(reverse: false);
            }
            else if (key == VirtualKey.Left && ctrlPressed)
            {
                // Prev Track
                await controller.PreviousTrack();
                if (nowPlaying.IsOpen)
                    nowPlaying.PlayChangeTrackAnimation(reverse: true);
            }
            else if (key == VirtualKey.Up && ctrlPressed & shiftPressed)
            {
                // Max volume
                await controller.SeekVolume(1.0);
            }
            else if (key == VirtualKey.Down && ctrlPressed && shiftPressed)
            {
                // Mute
                await controller.SeekVolume(0.0);
            }
            else if (key == VirtualKey.Up && ctrlPressed)
            {
                // Volume up
                var newVolume = Math.Min(PlayStatusTracker.LastPlayStatus.Volume + 0.1, 1.0);
                await controller.SeekVolume(newVolume);
                PlayStatusTracker.LastPlayStatus.Volume = newVolume;
            }
            else if (key == VirtualKey.Down && ctrlPressed)
            {
                // Volume down
                var newVolume = Math.Max(PlayStatusTracker.LastPlayStatus.Volume - 0.1, 0.0);
                await controller.SeekVolume(newVolume);
                PlayStatusTracker.LastPlayStatus.Volume = newVolume;
            }
            else if (key == VirtualKey.Left && altPressed)
            {
                // Back
                return KeyDownProcessResult.GoBack;
            }
            else if (key == VirtualKey.Escape)
            {
                // Back
                return KeyDownProcessResult.GoBack;
            }
            else if (key == VirtualKey.M && ctrlPressed && nowPlaying.IsOpen && nowPlaying.ViewMode == Controls.NowPlayingView.NowPlayingViewMode.Normal)
            {
                await nowPlaying.SwitchToMiniView();
            }
            else if (key == VirtualKey.Left || key == VirtualKey.Right || key == VirtualKey.Up || key == VirtualKey.Down)
            {
                // Do nothing, but don't switch out of now playing either.
            }
            else
            {
                return KeyDownProcessResult.AskJs;
            }
            return KeyDownProcessResult.Handled;
        }
    }
}
