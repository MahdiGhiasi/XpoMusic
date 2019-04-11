using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace ModernSpotifyUWP.Helpers
{
    public static class TileHelper
    {
        public static async Task PinPageToStart(string pageUrl, string title)
        {
            // TODO: Download and use album/artist/playlist image for tile, if applicable

            string tileId = Guid.NewGuid().ToString();

            string arguments = "pageUrl=" + WebUtility.UrlEncode(pageUrl);

            SecondaryTile tile = new SecondaryTile(
                tileId,
                title,
                arguments,
                new Uri("ms-appx:///Assets/Square150x150Logo.png"),
                TileSize.Default);

            // Enable wide and large tile sizes
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
            tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/LargeTile.png");

            var image = await GetTileImage(pageUrl);
            if (image != null)
            {
                tile.VisualElements.Square150x150Logo = image;
                tile.VisualElements.Wide310x150Logo = image;
                tile.VisualElements.Square310x310Logo = image;
            }


            // Show the display name on all sizes
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ShowNameOnWide310x150Logo = true;
            tile.VisualElements.ShowNameOnSquare310x310Logo = true;

            var result = await tile.RequestCreateAsync();

            if (!result)
                Debug.WriteLine("Tile creation failed");
        }

        public static async Task<Uri> GetTileImage(string pageUrl)
        {
            try
            {
                if (pageUrl.ToLower().StartsWith("https://open.spotify.com/playlist/"))
                {
                    string playlistId = pageUrl.Substring("https://open.spotify.com/playlist/".Length);
                    if (playlistId.Contains('/'))
                        playlistId = playlistId.Substring(0, playlistId.IndexOf('/') - 1);

                    var image = await ImageSaveHelper.GetAndSaveImage(await SongImageProvider.GetPlaylistArt(playlistId));

                    return image;
                }
                else if (pageUrl.ToLower().StartsWith("https://open.spotify.com/artist/"))
                {
                    string artistId = pageUrl.Substring("https://open.spotify.com/artist/".Length);
                    if (artistId.Contains('/'))
                        artistId = artistId.Substring(0, artistId.IndexOf('/') - 1);

                    var image = await ImageSaveHelper.GetAndSaveImage(await SongImageProvider.GetArtistArt(artistId));

                    return image;
                }
                else if (pageUrl.ToLower().StartsWith("https://open.spotify.com/album/"))
                {
                    string albumId = pageUrl.Substring("https://open.spotify.com/album/".Length);
                    if (albumId.Contains('/'))
                        albumId = albumId.Substring(0, albumId.IndexOf('/') - 1);

                    var image = await ImageSaveHelper.GetAndSaveImage(await SongImageProvider.GetAlbumArt(albumId));

                    return image;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetTileImage failed: " + ex.ToString());
            }

            return null;
        }
    }
}
