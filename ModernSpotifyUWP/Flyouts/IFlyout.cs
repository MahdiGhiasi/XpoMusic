using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModernSpotifyUWP.Flyouts
{
    internal interface IFlyout<T>
    {
        event EventHandler<T> FlyoutCloseRequest;
    }
}
