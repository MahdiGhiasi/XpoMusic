using Xpotify.Helpers;
using Xpotify.SpotifyApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace Xpotify.Classes
{
    public static class PlayStatusTracker
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static DispatcherTimer timer;
        static DateTime lastStatusFetch = DateTime.MinValue;

        public static SystemMediaTransportControls MediaControls { get; internal set; }

        public static class LastPlayStatus
        {
            public static string ArtistName { get; internal set; }
            public static string ArtistId { get; internal set; }
            public static string AlbumName { get; internal set; }
            public static string AlbumId { get; internal set; }
            public static string SongName { get; internal set; }
            public static string SongId { get; internal set; }
            public static int SongLengthMilliseconds { get; internal set; }
            public static bool IsPlaying { get; internal set; }
            public static double Volume { get; internal set; }

            private static int progressedMilliseconds;
            private static DateTime progressedMillisecondsSetMoment;
            public static int ProgressedMilliseconds
            {
                get
                {
                    if (!IsPlaying)
                        return Math.Min(progressedMilliseconds, SongLengthMilliseconds);

                    return Math.Min(SongLengthMilliseconds, (int)(progressedMilliseconds 
                        + (DateTime.UtcNow - progressedMillisecondsSetMoment).TotalMilliseconds));
                }
                set
                {
                    progressedMilliseconds = value;
                    progressedMillisecondsSetMoment = DateTime.UtcNow;
                }
            }

            public static event EventHandler Updated;

            internal static void InvokeUpdated()
            {
                Updated?.Invoke(null, null);
            }
        }

        static PlayStatusTracker()
        {
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            timer.Tick += Timer_Tick;
        }

        public static void SeekPlayback(double percentage)
        {
            LastPlayStatus.ProgressedMilliseconds = (int)(LastPlayStatus.SongLengthMilliseconds * percentage);
            LastPlayStatus.InvokeUpdated();
        }

        public static void SeekVolume(double percentage)
        {
            LastPlayStatus.Volume = percentage;
            LastPlayStatus.InvokeUpdated();
        }

        private static async void Timer_Tick(object sender, object e)
        {
            // Ignore if not logged in
            if (!TokenHelper.HasTokens())
                return;

            if ((DateTime.UtcNow - lastStatusFetch) > AppConstants.Instance.PlayStatePollInterval)
            {
                await RefreshPlayStatus();
            }
        }

        public static void StartRegularRefresh()
        {
            timer.Start();
        }

        public static async Task RefreshPlayStatus()
        {
            try
            {
                lastStatusFetch = DateTime.UtcNow;

                var player = new Player();
                var current = await player.GetCurrentlyPlaying();

                //if (current == null)
                //{
                //    LastPlayStatus.AlbumId = "";
                //    LastPlayStatus.AlbumName = "";
                //    LastPlayStatus.ArtistId = "";
                //    LastPlayStatus.ArtistName = "";
                //    LastPlayStatus.ProgressedMilliseconds = 0;
                //    LastPlayStatus.SongLengthMilliseconds = 0;
                //    LastPlayStatus.SongId = "";
                //    LastPlayStatus.SongName = "";
                //    LastPlayStatus.IsPlaying = false;
                //    LastPlayStatus.Volume = 0.0;
                //
                //    LastPlayStatus.InvokeUpdated();
                //}
                //else
                if (current != null)
                {
                    LastPlayStatus.AlbumId = current.item.album.id;
                    LastPlayStatus.AlbumName = current.item.album.name;
                    LastPlayStatus.ArtistId = current.item.artists.FirstOrDefault().id ?? "";
                    LastPlayStatus.ArtistName = current.item.artists.FirstOrDefault().name ?? "Unknown Artist";
                    LastPlayStatus.ProgressedMilliseconds = current.progress_ms;
                    LastPlayStatus.SongLengthMilliseconds = current.item.duration_ms;
                    LastPlayStatus.SongId = current.item.id;
                    LastPlayStatus.SongName = current.item.name;
                    LastPlayStatus.IsPlaying = current.is_playing;
                    LastPlayStatus.Volume = (current.device.volume_percent ?? 50) / 100.0;

                    LastPlayStatus.InvokeUpdated();
                }

                await UpdateMediaControls();
            }
            catch (Exception ex)
            {
                logger.Info("RefreshPlayStatus failed: " + ex.ToString());
            }
        }

        private static async Task UpdateMediaControls()
        {
            if (MediaControls == null)
                return;

            MediaControls.PlaybackStatus = (LastPlayStatus.IsPlaying) ? MediaPlaybackStatus.Playing : MediaPlaybackStatus.Paused;
            MediaControls.DisplayUpdater.MusicProperties.Title = LastPlayStatus.SongName;
            MediaControls.DisplayUpdater.MusicProperties.AlbumTitle = LastPlayStatus.AlbumName;
            MediaControls.DisplayUpdater.MusicProperties.Artist = LastPlayStatus.ArtistName;

            try
            {
                var albumArt = await SongImageProvider.GetAlbumArt(LastPlayStatus.AlbumId);
                if (string.IsNullOrEmpty(albumArt))
                    MediaControls.DisplayUpdater.Thumbnail = null;
                else
                    MediaControls.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(new Uri(albumArt));
            }
            catch { }

            MediaControls.DisplayUpdater.Update();
        }
    }
}
