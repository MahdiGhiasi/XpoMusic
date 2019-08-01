using XpoMusic.Classes;
using XpoMusic.Classes.Model.LyricsViewer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace XpoMusic.Helpers.Integration
{
    public static class LyricsViewerIntegrationHelper
    {
        private const string lyricsViewerPackageFamilyName = "36835MahdiGhiasi.LyricsViewerforSpotify_yddpmccgg2mz2";
        private const string lyricsViewerAppServiceName = "LyricsViewerService";

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static bool isLyricsViewerInstalled = true;

        private static AppServiceConnection connection = null;
        private static bool connectionActive = false;

        private static async Task SendPlaybackToLyricsViewer(CurrentPlayingSongInfo currentSong)
        {
            if (!isLyricsViewerInstalled)
                return;

            if (connection == null || !connectionActive)
            {
                connection = new AppServiceConnection
                {
                    AppServiceName = lyricsViewerAppServiceName,
                    PackageFamilyName = lyricsViewerPackageFamilyName,
                };
                connection.ServiceClosed += Connection_ServiceClosed;

                var connectionResult = await connection.OpenAsync();

                if (connectionResult == AppServiceConnectionStatus.AppNotInstalled)
                {
                    logger.Info("Can't connect to LyricsViewer app service: AppNotInstalled.");
                    isLyricsViewerInstalled = false;
                    return;
                }
                else if (connectionResult != AppServiceConnectionStatus.Success)
                {
                    logger.Info($"Can't connect to LyricsViewer app service: {connectionResult}.");
                    return;
                }

                connectionActive = true;
            }

            var vs = new ValueSet
            {
                { "messageType", "currentPlayingSongInfo" },
                { "sender", "Xpotify" },
                { "data", JsonConvert.SerializeObject(currentSong) },
            };
            var result = await connection.SendMessageAsync(vs);

            logger.Info($"Sent currentPlayingSongInfo to LyricsViewer. Result was '{result.Status}' and response was: {JsonConvert.SerializeObject(result.Message)}");
        }

        private static void Connection_ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            connectionActive = false;
        }

        internal static async void SendPlaybackToLyricsViewer()
        {
            if (!isLyricsViewerInstalled)
                return;

            try
            {
                var currentSong = new CurrentPlayingSongInfo
                {
                    ArtistName = PlayStatusTracker.LastPlayStatus.ArtistName,
                    AlbumName = PlayStatusTracker.LastPlayStatus.AlbumName,
                    SongName = PlayStatusTracker.LastPlayStatus.SongName,
                    AlbumArtUri = await SongImageProvider.GetAlbumArt(PlayStatusTracker.LastPlayStatus.AlbumId),
                };

                await SendPlaybackToLyricsViewer(currentSong);
            }
            catch (Exception ex)
            {
                logger.Warn("SendPlaybackToLyricsViewer failed: " + ex.ToString());
            }
        }

        internal static void InitIntegration()
        {
            PlayStatusTracker.LastPlayStatus.Updated += (s, e) =>
            {
                SendPlaybackToLyricsViewer();
            };
        }
    }
}
