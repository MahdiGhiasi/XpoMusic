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
        public event EventHandler<ProgressBarCommandEventArgs> ProgressBarCommandReceived;

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
    }
}
