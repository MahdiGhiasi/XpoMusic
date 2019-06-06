using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

namespace Xpotify.Helpers
{
    public static class JumpListHelper
    {
        /// <summary>
        /// Entries get added to the jump list when you play music via Cortana.
        /// This function removes them.
        /// </summary>
        public static async void DeleteRecentJumplistEntries()
        {
            var jumpList = await JumpList.LoadCurrentAsync();

            jumpList.SystemGroupKind = JumpListSystemGroupKind.None;
            jumpList.Items.Clear();

            await jumpList.SaveAsync();
        }
    }
}
