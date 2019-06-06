using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Xpotify.XpotifyApi.Model
{
    public class AssetPackage
    {
        public AssetPackageInfo AssetPackageInfo { get; set; }
        public StorageFile File { get; set; }
    }
}
