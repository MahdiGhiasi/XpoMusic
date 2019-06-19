using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.UI.Core;
using Xpotify.Classes;
using Xpotify.Controls;

namespace Xpotify.Helpers
{
    public static class MediaControlsHelper
    {
        public static event EventHandler<TrackChangedEventArgs> TrackChanged;

        private static CoreDispatcher _dispatcher;

        public static async void Init(CoreDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            var mediaControls = SystemMediaTransportControls.GetForCurrentView();
            mediaControls.IsEnabled = true;
            mediaControls.IsPreviousEnabled = true;
            mediaControls.IsNextEnabled = true;
            mediaControls.IsPlayEnabled = true;
            mediaControls.IsPauseEnabled = true;
            mediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
            mediaControls.ButtonPressed += SystemControls_ButtonPressed;
            await mediaControls.DisplayUpdater.CopyFromFileAsync(MediaPlaybackType.Music,
                await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Media/silent.wav")));

            PlayStatusTracker.MediaControls = mediaControls;
            PlayStatusTracker.StartRegularRefresh();
        }

        private static async void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs e)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var mediaControls = SystemMediaTransportControls.GetForCurrentView();

                try
                {
                    switch (e.Button)
                    {
                        case SystemMediaTransportControlsButton.Play:
                            if (await PlaybackActionHelper.Play())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;

                            break;
                        case SystemMediaTransportControlsButton.Pause:
                            if (await PlaybackActionHelper.Pause())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;

                            break;
                        case SystemMediaTransportControlsButton.Stop:
                            if (await PlaybackActionHelper.Pause())
                                mediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;

                            break;
                        case SystemMediaTransportControlsButton.Next:
                            if (await PlaybackActionHelper.NextTrack())
                                TrackChanged?.Invoke(null, new TrackChangedEventArgs
                                {
                                    Direction = TrackChangedEventArgs.TrackChangeDirection.Forward,
                                });
                                

                            break;
                        case SystemMediaTransportControlsButton.Previous:
                            if (await PlaybackActionHelper.PreviousTrack())
                                TrackChanged?.Invoke(null, new TrackChangedEventArgs
                                {
                                    Direction = TrackChangedEventArgs.TrackChangeDirection.Backward,
                                });

                            // Necessary for progress bar update, in case 'previous' command goes to 
                            // the beginning of the same track.
                            await Task.Delay(500);
                            await PlayStatusTracker.RefreshPlayStatus();

                            break;
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    UnauthorizedHelper.OnUnauthorizedError();
                }
            });
        }

        public class TrackChangedEventArgs
        {
            public TrackChangeDirection Direction { get; internal set; }

            public enum TrackChangeDirection
            {
                Forward,
                Backward,
            }
        }
    }
}
