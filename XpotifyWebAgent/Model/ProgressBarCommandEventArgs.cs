using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XpotifyWebAgent.Model
{
    public sealed class ProgressBarCommandEventArgs
    {
        public ProgressBarCommand Command { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
    }

    public enum ProgressBarCommand
    {
        Show,
        Hide,
    }
}
