using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using XpoMusic.WebAgent.Model;

namespace XpoMusic.WebAgent
{
    public sealed class XpoWebAgent : WebAgentBase
    {
        public event EventHandler<StatusReportReceivedEventArgs> StatusReportReceived;
        public event EventHandler<ActionRequestedEventArgs> ActionRequested;
        public event EventHandler<InitFailedEventArgs> InitializationFailed;
        public event EventHandler<LogMessageReceivedEventArgs> LogMessageReceived;
        public event EventHandler<object> NewAccessTokenRequested;

        private TaskCompletionSource<string> newAccessTokenTcs;

        public bool WebPlayerBackupEnabled { get; set; }
        public string OSVersion { get; set; }

        public XpoWebAgent(WebView2 webView, string objectAccessibleName) : base(webView, objectAccessibleName)
        {
        }

        public void ShowProgressBar(double left, double top, double width)
        {
            //ProgressBarCommandReceived?.Invoke(this, new ProgressBarCommandEventArgs
            //{
            //    Command = ProgressBarCommand.Show,
            //    Left = left,
            //    Top = top,
            //    Width = width,
            //});
        }

        public void HideProgressBar()
        {
            //ProgressBarCommandReceived?.Invoke(this, new ProgressBarCommandEventArgs
            //{
            //    Command = ProgressBarCommand.Hide,
            //});
        }

        public void StatusReport(WebAppStatus data)
        {
            StatusReportReceived?.Invoke(this, new StatusReportReceivedEventArgs
            {
                Status = data,
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

        public Task<string> GetNewAccessTokenAsync()
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

        public int GetOSBuildVersion()
        {
            try
            {
                return new Version(OSVersion).Build;
            }
            catch (Exception ex)
            {
                Log("Exception in WebAgent.GetOSBuildVersion(): " + ex.ToString());
                return 0;
            }
        }

        public Task DelayTest()
        {
            return Task.Delay(10000);
        }

        public async Task<int> AsyncTaskTest()
        {
            await Task.Delay(500);
            return 256;
        }
    }
}
