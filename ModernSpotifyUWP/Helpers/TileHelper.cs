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

            // Show the display name on all sizes
            tile.VisualElements.ShowNameOnSquare150x150Logo = true;
            tile.VisualElements.ShowNameOnWide310x150Logo = true;
            tile.VisualElements.ShowNameOnSquare310x310Logo = true;

            var result = await tile.RequestCreateAsync();

            if (!result)
                Debug.WriteLine("Tile creation failed");
        }
    }
}
