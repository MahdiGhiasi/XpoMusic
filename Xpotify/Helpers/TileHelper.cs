using Xpotify.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Media.Imaging;

namespace Xpotify.Helpers
{
    public static class TileHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static async Task<bool> PinPageToStart(string pageUrl, string title)
        {
            // TODO: Download and use album/artist/playlist image for tile, if applicable

            string tileId = Guid.NewGuid().ToString();

            string arguments = "pageUrl=" + WebUtility.UrlEncode(pageUrl);

            SecondaryTile tile = new SecondaryTile(
                tileId,
                title,
                arguments,
                new Uri("ms-appx:///Assets/Logo/Square150x150Logo.png"),
                TileSize.Default);

            // Enable wide and large tile sizes
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Logo/Wide310x150Logo.png");
            tile.VisualElements.Square310x310Logo = new Uri("ms-appx:///Assets/Logo/LargeTile.png");

            var images = await GetTileImages(pageUrl);
            if (images != null)
            {
                tile.VisualElements.Square150x150Logo = images.SquareImage;
                tile.VisualElements.Wide310x150Logo = images.WideImage;
                tile.VisualElements.Square310x310Logo = images.SquareImage;
            }

            // Show the display name on all sizes
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ShowNameOnWide310x150Logo = true;
            tile.VisualElements.ShowNameOnSquare310x310Logo = true;

            var result = await tile.RequestCreateAsync();

            if (!result)
                logger.Info("Tile creation failed");

            return result;
        }

        public static async Task<TileImageCollection> GetTileImages(string pageUrl)
        {
            try
            {
                if (pageUrl.ToLower().StartsWith("https://open.spotify.com/playlist/"))
                {
                    string playlistId = pageUrl.Substring("https://open.spotify.com/playlist/".Length);
                    if (playlistId.Contains('/'))
                        playlistId = playlistId.Substring(0, playlistId.IndexOf('/') - 1);

                    var image = await ImageSaveHelper.GetAndSaveTileOriginalImage(await SongImageProvider.GetPlaylistArt(playlistId));
                    var tileImages = await CreateTileImages(image);

                    await (await StorageFile.GetFileFromApplicationUriAsync(image)).DeleteAsync();

                    return tileImages;
                }
                else if (pageUrl.ToLower().StartsWith("https://open.spotify.com/artist/"))
                {
                    string artistId = pageUrl.Substring("https://open.spotify.com/artist/".Length);
                    if (artistId.Contains('/'))
                        artistId = artistId.Substring(0, artistId.IndexOf('/') - 1);

                    var image = await ImageSaveHelper.GetAndSaveTileOriginalImage(await SongImageProvider.GetArtistArt(artistId));
                    var tileImages = await CreateTileImages(image);

                    await (await StorageFile.GetFileFromApplicationUriAsync(image)).DeleteAsync();

                    return tileImages;
                }
                else if (pageUrl.ToLower().StartsWith("https://open.spotify.com/album/"))
                {
                    string albumId = pageUrl.Substring("https://open.spotify.com/album/".Length);
                    if (albumId.Contains('/'))
                        albumId = albumId.Substring(0, albumId.IndexOf('/') - 1);

                    var image = await ImageSaveHelper.GetAndSaveTileOriginalImage(await SongImageProvider.GetAlbumArt(albumId));
                    var tileImages = await CreateTileImages(image);

                    await (await StorageFile.GetFileFromApplicationUriAsync(image)).DeleteAsync();

                    return tileImages;
                }
            }
            catch (Exception ex)
            {
                logger.Info("GetTileImage failed: " + ex.ToString());
            }

            return null;
        }

        private static async Task<TileImageCollection> CreateTileImages(Uri imageUri)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(imageUri);
            var decoder = await BitmapDecoder.CreateAsync(await file.OpenAsync(FileAccessMode.Read));

            WriteableBitmap squareBitmap, wideBitmap;

            (Point squarePoint, Size squareSize) = GetCropDetails(decoder, 1.0);
            squareBitmap = await CropBitmap.GetCroppedBitmapAsync(file, squarePoint, squareSize, 1.0);

            (Point widePoint, Size wideSize) = GetCropDetails(decoder, 310.0 / 150.0);
            wideBitmap = await CropBitmap.GetCroppedBitmapAsync(file, widePoint, wideSize, 1.0);

            var squareFile = await ImageSaveHelper.SaveWritableBitmapToTileImageCache(squareBitmap, "square");
            var wideFile = await ImageSaveHelper.SaveWritableBitmapToTileImageCache(wideBitmap, "wide");

            return new TileImageCollection
            {
                SquareImage = squareFile,
                WideImage = wideFile,
            };
        }

        public static (Point, Size) GetCropDetails(BitmapDecoder decoder, double desiredRatio)
        {
            double sourceRatio = ((double)decoder.PixelWidth) / ((double)decoder.PixelHeight);

            if (sourceRatio == desiredRatio)
            {
                return (new Point(0, 0), new Size(decoder.PixelWidth, decoder.PixelHeight));
            }
            else if (sourceRatio > desiredRatio)
            {
                var destWidth = (desiredRatio / sourceRatio) * decoder.PixelWidth;
                var x = (decoder.PixelWidth / 2.0) - (destWidth / 2.0);
                return (new Point(x, 0), new Size(destWidth, decoder.PixelHeight));
            }
            else
            {
                var destHeight = (sourceRatio / desiredRatio) * decoder.PixelHeight;
                var y = (decoder.PixelHeight / 2.0) - (destHeight / 2.0);
                return (new Point(0, y), new Size(decoder.PixelWidth, destHeight));
            }
        }
    }

    public class TileImageCollection
    {
        public Uri SquareImage;
        public Uri WideImage;
    }
}
