using XpoMusic.Helpers;
using XpoMusic.SpotifyApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using XpoMusic.Classes.Model;
using XpoMusic.Classes.Cache;
using XpoMusicWebAgent.Model;

namespace XpoMusic.Classes
{
    public static class PlayStatusTracker
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static DispatcherTimer timer;
        static DateTime lastStatusFetch = DateTime.MinValue;

        public static bool IsLocalStatusTrackingOperational { get; private set; } = false;

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
            public static bool IsNextTrackAvailable { get; internal set; }
            public static bool IsPrevTrackAvailable { get; internal set; }

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
                IsLocalStatusTrackingOperational = false;
                await RefreshPlayStatus();
            }
        }

        public static void StartRegularRefresh()
        {
            if (!timer.IsEnabled)
                timer.Start();
        }

        private static int timesFingerprintEmpty = 0;
        private static string lastLocalFingerprint = "";
        public static async void LocalPlaybackDataReceived(NowPlayingData data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data.TrackFingerprint))
                {
                    if (timesFingerprintEmpty == 20)
                    {
                        logger.Warn($"LocalPlaybackDataError #{timesFingerprintEmpty}");
                        AnalyticsHelper.Log("localPlaybackDataError", "fingerprintInvalid", JsonConvert.SerializeObject(data));
                        timesFingerprintEmpty++;
                    }
                    else if (timesFingerprintEmpty < 20)
                    {
                        timesFingerprintEmpty++;
                        logger.Info($"LocalPlaybackDataError #{timesFingerprintEmpty}");
                    }
                }

                if (!data.Success
                    && !string.IsNullOrWhiteSpace(data.TrackFingerprint)
                    && data.TrackFingerprint != lastLocalFingerprint)
                {
                    logger.Info("LocalPlaybackDataReceived, success = false, fingerprint changed to " + data.TrackFingerprint);
                    await RefreshPlayStatus();
                    AnalyticsHelper.Log("localPlaybackDataError", "invalid", JsonConvert.SerializeObject(data));
                }
                else if (data.Success)
                {
                    bool changed = (LastPlayStatus.SongId != data.TrackId
                        || LastPlayStatus.Volume != data.Volume 
                        || LastPlayStatus.IsNextTrackAvailable != data.IsNextTrackAvailable
                        || LastPlayStatus.IsPrevTrackAvailable != data.IsPrevTrackAvailable);

                    LastPlayStatus.AlbumId = data.AlbumId;
                    LastPlayStatus.ArtistId = data.ArtistId;
                    LastPlayStatus.ArtistName = data.ArtistName;
                    LastPlayStatus.ProgressedMilliseconds = data.ElapsedTime;
                    LastPlayStatus.SongLengthMilliseconds = data.TotalTime;
                    LastPlayStatus.SongId = data.TrackId;
                    LastPlayStatus.SongName = data.TrackName;
                    LastPlayStatus.IsPlaying = data.IsPlaying;
                    LastPlayStatus.Volume = data.Volume;
                    LastPlayStatus.IsNextTrackAvailable = data.IsNextTrackAvailable;
                    LastPlayStatus.IsPrevTrackAvailable = data.IsPrevTrackAvailable;

                    try
                    {
                        var album = await GlobalCache.Album.GetItem(data.AlbumId);
                        LastPlayStatus.AlbumName = album.name;
                    }
                    catch (Exception ex)
                    {
                        logger.Info("LocalPlaybackDataReceived:GetAlbumInfo failed: " + ex.ToString());
                    }

                    if (changed)
                    {
                        LastPlayStatus.InvokeUpdated();
                        logger.Info("LocalPlaybackDataReceived and changed = true for " + data.TrackFingerprint);
                    }

                    lastStatusFetch = DateTime.UtcNow;
                    IsLocalStatusTrackingOperational = true;
                }

                lastLocalFingerprint = data.TrackFingerprint;
            }
            catch (Exception ex)
            {
                logger.Info("LocalPlaybackDataReceived failed: " + ex.ToString());
            }
        }

        public static async Task RefreshPlayStatus()
        {
            try
            {
                // Don't ask API if local status tracking is working correctly.
                if (IsLocalStatusTrackingOperational)
                    return;

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
                    LastPlayStatus.IsNextTrackAvailable = true;
                    LastPlayStatus.IsPrevTrackAvailable = true;

                    LastPlayStatus.InvokeUpdated();
                }
            }
            catch (Exception ex)
            {
                logger.Info("RefreshPlayStatus failed: " + ex.ToString());
            }
        }
    }
}
