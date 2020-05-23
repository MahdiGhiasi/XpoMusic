using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpoMusic.WebAgent.Model
{
    public sealed class ActionRequestedEventArgs
    {
        public Action Action { get; set; }
    }

    public enum Action
    {
        PinToStart,
        OpenSettings,
        OpenDonate,
        OpenAbout,
        OpenMiniView,
        OpenNowPlaying,
        NavigateToClipboardUri,
    }
}
