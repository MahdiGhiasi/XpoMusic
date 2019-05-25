using ModernSpotifyUWP.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace ModernSpotifyUWP.Helpers
{
    public static class LiveTileHelper
    {
        private static readonly string liveTileTemplate = @"
<tile>
    <visual>

        <binding template='TileMedium' hint-textStacking='center'>
            <image src='{artistPhoto}' placement='background' hint-overlay='40'/>
            <group>
                <subgroup>
                    <text hint-style='base' hint-align='center' hint-wrap='true'>{songName}</text>
                    <text hint-style='caption' hint-align='center'>{artistName}</text>
                </subgroup>
            </group>
        </binding>

        <binding template='TileWide' hint-textStacking='center' >
            <image src='{artistPhoto}' placement='background' hint-overlay='40'/>
            <group>
                <subgroup hint-textStacking='center' hint-weight='90'>
                    <text hint-style='base' hint-align='center' hint-wrap='true'>{songName}</text>
                    <text hint-style='caption' hint-align='center'>{artistName}</text>
                    <text hint-style='captionSubtle' hint-align='center'>{albumName}</text>
                </subgroup>
            </group>
        </binding>

        <binding template='TileLarge' hint-textStacking='center'>
            <image src='{artistPhoto}' placement='background' hint-overlay='40'/>
            <group>
                <subgroup hint-weight='2'/>
                <subgroup hint-weight='3'>
                    <image src='{albumPhoto}' />
                </subgroup>
                <subgroup hint-weight='2'/>
            </group>
            <group>
                <subgroup hint-weight='15'/>
                <subgroup hint-weight='1'>
                    <image src='ms-appx:///Assets/TransparentSquare.png' />
                </subgroup>
                <subgroup hint-weight='15'/>
            </group>
            <text hint-style='subtitle' hint-align='center' hint-wrap='true'>{songName}</text>
            <text hint-style='body' hint-align='center'>{artistName}</text>
            <text hint-style='captionSubtle' hint-align='center'>{albumName}</text>
        </binding>

    </visual>
</tile>
";

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static TileUpdater tileUpdater;
        private static int? lastTileUpdateHash = null;

        public static void InitLiveTileUpdates()
        {
            tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();

            ClearLiveTile(); // Clear live tile on startup

            PlayStatusTracker.LastPlayStatus.Updated += LastPlayStatus_Updated;
            Application.Current.Suspending += Current_Suspending;
        }

        private static void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            ClearLiveTile();
            logger.Info("Cleared live tile on Suspending event.");
        }

        private static async void LastPlayStatus_Updated(object sender, EventArgs e)
        {
            try
            {
                //if (!PlayStatusTracker.LastPlayStatus.IsPlaying)
                //{
                //    ClearLiveTile();
                //    return;
                //}

                await UpdateLiveTile();
            }
            catch (Exception ex)
            {
                logger.Warn("LastPlayStatus_Updated error: " + ex.ToString());
            }
        }

        private static async Task UpdateLiveTile()
        {
            var artistPhoto = await SongImageProvider.GetArtistArt(PlayStatusTracker.LastPlayStatus.ArtistId);
            var albumPhoto = await SongImageProvider.GetAlbumArt(PlayStatusTracker.LastPlayStatus.AlbumId);

            var template = liveTileTemplate
                .Replace("{albumName}", WebUtility.HtmlEncode(PlayStatusTracker.LastPlayStatus.AlbumName))
                .Replace("{artistName}", WebUtility.HtmlEncode(PlayStatusTracker.LastPlayStatus.ArtistName))
                .Replace("{songName}", WebUtility.HtmlEncode(PlayStatusTracker.LastPlayStatus.SongName))
                .Replace("{artistPhoto}", WebUtility.HtmlEncode(artistPhoto))
                .Replace("{albumPhoto}", WebUtility.HtmlEncode(albumPhoto));

            if (lastTileUpdateHash.HasValue && lastTileUpdateHash.Value.GetHashCode() == template.GetHashCode())
                return; // Do not update tile again with same data.
            lastTileUpdateHash = template.GetHashCode();

            var xml = new XmlDocument();
            xml.LoadXml(template);

            var notification = new TileNotification(xml);

            tileUpdater.Update(notification);

            logger.Info("Live tile updated.");
        }

        public static void ClearLiveTile()
        {
            tileUpdater.Clear();
        }
    }
}
