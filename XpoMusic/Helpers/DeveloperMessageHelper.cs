using XpoMusic.Classes;
using XpoMusic.XpotifyApi;
using XpoMusic.XpotifyApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.Helpers
{
    public static class DeveloperMessageHelper
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static async Task<DeveloperMessage> GetNextDeveloperMessage()
        {
            try
            {
                var messages = await DeveloperMessageApi.GetDeveloperMessages();

                if (messages == null || messages.messages == null)
                    return null;

                var readMessageIds = LocalConfiguration.DeveloperMessageShownIds;
                var unreadMessages = messages.messages.Where(x => !readMessageIds.Contains(x.id)).ToList();

                var firstUnreadMessage = unreadMessages.FirstOrDefault();

                if (firstUnreadMessage == null)
                    return null;

                LocalConfiguration.AddIdToDeveloperMessageShownIds(firstUnreadMessage.id);
                return firstUnreadMessage;
            }
            catch (Exception ex)
            {
                logger.Warn("GetNextDeveloperMessage failed: " + ex.ToString());
                AnalyticsHelper.Log("exception-GetNextDeveloperMessage", ex.Message, ex.ToString());
                return null;
            }
        }
    }
}
