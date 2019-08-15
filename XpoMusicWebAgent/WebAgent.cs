using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using XpoMusicWebAgent.Model;

namespace XpoMusicWebAgent
{
    [AllowForWeb]
    public sealed class WebAgent
    {
        // NOTE:
        // It seems that WebView automatically changes PascalCase method names to 
        // camelCase on the Javascript side.

        public event EventHandler<ProgressBarCommandEventArgs> ProgressBarCommandReceived;
        public event EventHandler<StatusReportReceivedEventArgs> StatusReportReceived;
        public event EventHandler<ActionRequestedEventArgs> ActionRequested;
        public event EventHandler<InitFailedEventArgs> InitializationFailed;
        public event EventHandler<LogMessageReceivedEventArgs> LogMessageReceived;
        public event EventHandler<object> NewAccessTokenRequested;

        private TaskCompletionSource<string> newAccessTokenTcs;

        public bool WebPlayerBackupEnabled { get; set; }

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

        public IAsyncOperation<string> GetNewAccessTokenAsync()
        {
            return GetNewAccessToken().AsAsyncOperation();
        }

        private Task<string> GetNewAccessToken()
        {
            newAccessTokenTcs = new TaskCompletionSource<string>();
            NewAccessTokenRequested?.Invoke(this, new EventArgs());
            return newAccessTokenTcs.Task;
        }

        public void SetNewAccessToken(string accessToken)
        {
            newAccessTokenTcs.TrySetResult(accessToken);
        }

        public void Log(string message)
        {
            LogMessageReceived?.Invoke(this, new LogMessageReceivedEventArgs
            {
                Message = message,
            });
        }

        public bool IsWebPlayerBackupEnabled()
        {
            return WebPlayerBackupEnabled;
        }
    }
}
