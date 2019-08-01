using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace XpoMusic.Helpers
{
    public static class ImageSaveHelper
    {
        private const string tileImageCacheFolderName = "TileImageCache";

        public static async Task<Uri> GetAndSaveTileOriginalImage(string remoteUri)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(remoteUri);
                var content = await response.Content.ReadAsByteArrayAsync();

                var folder = (await ApplicationData.Current.LocalFolder.TryGetItemAsync(tileImageCacheFolderName)) as StorageFolder;
                if (folder == null)
                    folder = await CreateTileCacheFolder();

                var fileName = DateTime.Now.ToString("yyyyMMddHHmm") + "-" + Guid.NewGuid() + ".jpg";
                var file = await folder.CreateFileAsync(fileName);

                await FileIO.WriteBytesAsync(file, content);

                return new Uri($"ms-appdata:///local/{tileImageCacheFolderName}/{fileName}");
            }
        }

        private static async Task<StorageFolder> CreateTileCacheFolder()
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync(tileImageCacheFolderName);
        }

        public static async Task<Uri> SaveWritableBitmapToTileImageCache(WriteableBitmap wb, string tag)
        {
            var folder = (await ApplicationData.Current.LocalFolder.TryGetItemAsync(tileImageCacheFolderName)) as StorageFolder;
            if (folder == null)
                folder = await CreateTileCacheFolder();

            var fileName = DateTime.Now.ToString("yyyyMMddHHmm") + "-" + Guid.NewGuid() + "-" + tag + ".jpg";
            var file = await folder.CreateFileAsync(fileName);

            using (IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                Stream pixelStream = wb.PixelBuffer.AsStream();
                byte[] pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                                    (uint)wb.PixelWidth,
                                    (uint)wb.PixelHeight,
                                    96.0,
                                    96.0,
                                    pixels);
                await encoder.FlushAsync();
            }

            return new Uri($"ms-appdata:///local/{tileImageCacheFolderName}/{fileName}");
        }
    }
}
