using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using XpotifyWebAgent.Model;

namespace XpotifyWebAgent
{
    [AllowForWeb]
    public sealed class XpotifyWebAgent
    {
        // NOTE:
        // It seems that WebView automatically changes PascalCase method names to 
        // camelCase on the Javascript side.

        public event EventHandler<ProgressBarCommandEventArgs> ProgressBarCommandReceived;
        public event EventHandler<StatusReportReceivedEventArgs> StatusReportReceived;
        public event EventHandler<ActionRequestedEventArgs> ActionRequested;
        public event EventHandler<InitFailedEventArgs> InitializationFailed;

        public void ShowProgressBar(double left, double top, double width)
        {
            ProgressBarCommandReceived?.Invoke(this, new ProgressBarCommandEventArgs
            {
                Command = ProgressBarCommand.Show,
                Left = left,
                Top = top,
                Width = width,
            });
        }

        public void HideProgressBar()
        {
            ProgressBarCommandReceived?.Invoke(this, new ProgressBarCommandEventArgs
            {
                Command = ProgressBarCommand.Hide,
            });
        }

        public void StatusReport(string data)
        {
            StatusReportReceived?.Invoke(this, new StatusReportReceivedEventArgs
            {
                Status = JsonConvert.DeserializeObject<WebAppStatus>(data),
            });
        }

        public void OpenSettings()
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Model.Action.OpenSettings,
            });
        }

        public void OpenAbout()
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Model.Action.OpenAbout,
            });
        }

        public void OpenDonate()
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Model.Action.OpenDonate,
            });
        }

        public void OpenMiniView()
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Model.Action.OpenMiniView,
            });
        }

        public void OpenNowPlaying()
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Model.Action.OpenNowPlaying,
            });
        }

        public void PinToStart()
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Model.Action.PinToStart,
            });
        }

        public void NavigateToClipboardUri()
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Model.Action.NavigateToClipboardUri,
            });
        }

        public void InitFailed(string errors)
        {
            InitializationFailed?.Invoke(this, new InitFailedEventArgs
            {
                Errors = errors,
            });
        }
    }
}
