using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ModernSpotifyUWP.Helpers
{
    public static class ImageSaveHelper
    {
        private const string tileImageCacheFolderName = "TileImageCache";

        public static async Task<Uri> GetAndSaveImage(string remoteUri)
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
    }
}
