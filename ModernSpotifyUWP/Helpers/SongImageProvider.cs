using Xpotify.SpotifyApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xpotify.Helpers
{
    public static class SongImageProvider
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static Dictionary<string, string> artistImages = new Dictionary<string, string>();
        static Dictionary<string, string> albumImages = new Dictionary<string, string>();
        static Dictionary<string, string> playlistImages = new Dictionary<string, string>();

        public static async Task<string> GetArtistArt(string artistId)
        {
            if (artistImages.ContainsKey(artistId))
                return artistImages[artistId];
            if (string.IsNullOrEmpty(artistId))
                return "";

            try
            {
                var artistApi = new Artist();
                var artist = await artistApi.GetArtist(artistId);

                artistImages[artistId] = artist.images.OrderBy(x => x.width).Last().url;
                return artistImages[artistId];
            }
            catch (Exception ex)
            {
                logger.Info($"Fetching artist art for {artistId} failed: {ex}");
                return "";
            }
        }

        public static async Task<string> GetAlbumArt(string albumId)
        {
            if (albumImages.ContainsKey(albumId))
                return albumImages[albumId];
            if (string.IsNullOrEmpty(albumId))
                return "";

            try
            {
                var albumApi = new Album();
                var album = await albumApi.GetAlbum(albumId);

                albumImages[albumId] = album.images.OrderBy(x => x.width).Last().url;
                return albumImages[albumId];
            }
            catch (Exception ex)
            {
                logger.Info($"Fetching album art for {albumId} failed: {ex}");
                return "";
            }
        }

        public static async Task<string> GetPlaylistArt(string playlistId)
        {
            if (playlistImages.ContainsKey(playlistId))
                return playlistImages[playlistId];
            if (string.IsNullOrEmpty(playlistId))
                return "";

            try
            {
                var playlistApi = new Playlist();
                var playlist = await playlistApi.GetPlaylist(playlistId);

                playlistImages[playlistId] = playlist.images.OrderBy(x => x.width).Last().url;
                return playlistImages[playlistId];
            }
            catch (Exception ex)
            {
                logger.Info($"Fetching album art for {playlistId} failed: {ex}");
                return "";
            }
        }
    }
}

