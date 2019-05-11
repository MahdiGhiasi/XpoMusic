using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Helpers;
using ModernSpotifyUWP.XpotifyApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ModernSpotifyUWP.XpotifyApi
{
    public static class AssetUpdatesApi
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly string assetUpdateCacheFolderName = "AssetUpdateDownloadCache";

        private static string UpdateUri
        {
            get
            {
                var endpoint = "https://ghiasi.net/xpotify/assets";
                var version = PackageHelper.GetAppVersion().ToString(3);
                var fileName = "assetPackageInfo.json";
                var query = "?date=" + DateTime.Now.ToString("yyyyMMddHHmmss");
                var proSuffix = PackageHelper.IsProVersion ? "pro" : "";

                return $"{endpoint}/{version}{proSuffix}/{fileName}{query}";
            }
        }

        private static async Task<AssetPackageInfo> GetLatestAssetPackageUri()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(UpdateUri);
                    if (response.IsSuccessStatusCode == false)
                    {
                        logger.Warn($"GetLatestAssetPackageUri failed because request failed with status code {response.StatusCode}.");
                        return null;
                    }

                    var result = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AssetPackageInfo>(result);
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetLatestAssetPackageUri failed: " + ex.ToString());
                return null;
            }
        }

        public static async Task<AssetPackage> GetLatestAssetPackage()
        {
            try
            {
                var assetPackageInfo = await GetLatestAssetPackageUri();

                if (assetPackageInfo == null)
                    return null;

                if (assetPackageInfo.version <= LocalConfiguration.LatestAssetUpdateVersion)
                {
                    logger.Info($"Latest asset package version is {assetPackageInfo.version} and the latest downloaded version is {LocalConfiguration.LatestAssetUpdateVersion}. Will not download.");
                    return null;
                }

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(assetPackageInfo.downloadUri);
                    if (response.IsSuccessStatusCode == false)
                    {
                        logger.Warn($"GetLatestAssetPackage failed because request failed with status code {response.StatusCode}.");
                        return null;
                    }

                    var result = await response.Content.ReadAsByteArrayAsync();

                    var folder = await GetAssetUpdateCacheFolder();
                    var file = await folder.CreateFileAsync($"{assetPackageInfo.version}.zip");
                    await FileIO.WriteBytesAsync(file, result);

                    return new AssetPackage
                    {
                        AssetPackageInfo = assetPackageInfo,
                        File = file,
                    };
                }
            }
            catch (Exception ex)
            {
                logger.Warn("GetLatestAssetPackage failed: " + ex.ToString());
                return null;
            }
        }

        private static async Task<StorageFolder> GetAssetUpdateCacheFolder()
        {
            var folder = (await ApplicationData.Current.LocalFolder.TryGetItemAsync(assetUpdateCacheFolderName)) as StorageFolder;
            if (folder == null)
                folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(assetUpdateCacheFolderName);

            return folder;
        }
    }
}
