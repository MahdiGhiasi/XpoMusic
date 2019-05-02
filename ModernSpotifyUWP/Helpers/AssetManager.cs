using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.XpotifyApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ModernSpotifyUWP.Helpers
{
    public static class AssetManager
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly string assetUpdateFolderName = "AssetUpdate";

        private static List<StorageFolder> assetFolders = null;
        private static SemaphoreSlim loadAssetFoldersSemaphore = new SemaphoreSlim(1, 1);
        private static Dictionary<string, string> assetStringFileCache = new Dictionary<string, string>();

        public static async void UpdateAssets()
        {
            try
            {
                var package = await AssetUpdatesApi.GetLatestAssetPackage();
                if (package == null)
                {
                    logger.Info("No new asset package available.");
                    return;
                }

                var destinationFolder = await GetAssetUpdateFolder(package.AssetPackageInfo.version.ToString());

                using (var stream = await package.File.OpenStreamForReadAsync())
                {
                    var archive = new ZipArchive(stream);
                    await Task.Run(() => archive.ExtractToDirectory(destinationFolder.Path, overwriteFiles: true));
                }

                LocalConfiguration.LatestAssetUpdateVersion = package.AssetPackageInfo.version;
                logger.Info($"Asset update version {package.AssetPackageInfo.version} downloaded and extracted.");

                // The zip file is no longer needed.
                await package.File.DeleteAsync();

                // Reload assetFolders on next asset load request.
                assetFolders = null;
            }
            catch (Exception ex)
            {
                logger.Warn("UpdateAssets failed: " + ex.ToString());
            }
        }

        private static async Task<StorageFolder> GetAssetUpdateFolder(string name)
        {
            var parentFolder = await GetAssetUpdateParentFolder();

            var folder = (await parentFolder.TryGetItemAsync(name)) as StorageFolder;
            if (folder == null)
                folder = await parentFolder.CreateFolderAsync(name);

            return folder;
        }

        private static async Task<StorageFolder> GetAssetUpdateParentFolder()
        {
            var folder = (await ApplicationData.Current.LocalFolder.TryGetItemAsync(assetUpdateFolderName)) as StorageFolder;
            if (folder == null)
                folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(assetUpdateFolderName);

            return folder;
        }

        private static async Task LoadAssetFolders()
        {
            try
            {
                await loadAssetFoldersSemaphore.WaitAsync();

                if (assetFolders != null)
                    return;

                var parentFolder = await GetAssetUpdateParentFolder();
                var folders = await parentFolder.GetFoldersAsync();

                assetFolders = folders.OrderByDescending(x =>
                {
                    if (int.TryParse(x.DisplayName, out int result))
                        return result;

                    return 0;
                }).ToList();

                var defaultFolder = await Windows.Application­Model.Package.Current.InstalledLocation.GetFolderAsync("InjectedAssets");
                assetFolders.Add(defaultFolder);

                logger.Info("Asset folders:\n" + string.Join('\n', assetFolders.Select(x => x.Path)));
                logger.Info("LatestAssetUpdateVersion: " + LocalConfiguration.LatestAssetUpdateVersion);
            }
            finally
            {
                loadAssetFoldersSemaphore.Release();
            }
        }

        public static async Task<string> LoadAssetString(string fileName)
        {
            if (assetFolders == null)
                await LoadAssetFolders();

            foreach (var folder in assetFolders)
            {
                if ((await folder.TryGetItemAsync(fileName)) is StorageFile file)
                {
                    return await ReadTextFile(fileName, file);
                }
            }

            logger.Warn($"LoadAssetString: File '{fileName}' not found.");
            return null;
        }

        private static async Task<string> ReadTextFile(string fileName, StorageFile file)
        {
            if (assetStringFileCache.ContainsKey(file.Path))
            {
#if DEBUG
                logger.Info($"Asset '{fileName}' loaded from cache.");
#endif
                return assetStringFileCache[file.Path];
            }

            var text = await FileIO.ReadTextAsync(file);
            assetStringFileCache[file.Path] = text;
            logger.Info($"Asset '{fileName}' loaded from '{file.Path}'.");

            return text;
        }
    }
}
