using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XpoMusic.Classes;
using XpoMusic.SpotifyApi;
using XpoMusicWebAgent.Model;

namespace XpoMusic.Helpers
{
    public static class StuckResolveHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static int stuckResolveTryCount = 0;
        private static int sameElapsedCounter = 0;
        private static int lastElapsed = -1;

        public static async void StatusReportReceived(NowPlayingData data, WebViewController controller)
        {
            if (!data.IsPlaying)
                return;

            logger.Info(data.ElapsedTime.ToString());

            try
            {
                if (data.ElapsedTime == lastElapsed)
                {
                    sameElapsedCounter++;

                    if (sameElapsedCounter > AppConstants.Instance.StuckDetectSameElapsedCount)
                    {
                        if (stuckResolveTryCount >= AppConstants.Instance.MaxStuckResolveTryCount)
                        {
                            logger.Warn("MaxStuckResolveTryCount exceeded.");

                            if (stuckResolveTryCount == AppConstants.Instance.MaxStuckResolveTryCount)
                                LogPlaybackStuck("MaxStuckResolveTryCountExceeded");

                            // TODO: Probably reload the page?
                        }

                        if (data.ElapsedTime == 0)
                        {
                            await PlayStartStuckResolve(controller);
                        }
                        else
                        {
                            await PlayMiddleStuckResolve(controller);
                        }

                        stuckResolveTryCount++;
                        sameElapsedCounter = -1 * AppConstants.Instance.StuckDetectSameElapsedExtraCount;
                    }
                }
                else
                {
                    stuckResolveTryCount = 0;
                    sameElapsedCounter = 0;
                    lastElapsed = data.ElapsedTime;
                }
            }
            catch (Exception ex)
            {
                logger.Warn("StuckResolveHelper.StatusReportReceived failed: " + ex.ToString());
            }
        }

        private static async Task PlayMiddleStuckResolve(WebViewController controller)
        {
            await controller.TryResolvePlaybackMiddleStuck();
            LogPlaybackStuck("14");
        }

        private static async Task PlayStartStuckResolve(WebViewController controller)
        {
            bool apiFixSuccess = false;

            var player = new Player();
            apiFixSuccess = await player.PreviousTrack();

            if (apiFixSuccess)
            {
                LogPlaybackStuck("11");
            }
            else
            {
                await controller.TryResolvePlaybackStartStuck();

                LogPlaybackStuck("12");
            }
        }

        private static void LogPlaybackStuck(string code)
        {
            ToastHelper.SendDebugToast("PlaybackStuck", code);
            AnalyticsHelper.Log("playbackStuck", code);
            logger.Info($"PlaybackStuck - {code}");
        }
    }
}
