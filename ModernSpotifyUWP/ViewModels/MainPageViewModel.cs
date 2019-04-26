using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace ModernSpotifyUWP.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private double topBarButtonWidth;
        public double TopBarButtonWidth
        {
            get => topBarButtonWidth;
            set
            {
                topBarButtonWidth = value;
                FirePropertyChangedEvent(nameof(TopBarButtonWidth));
                FirePropertyChangedEvent(nameof(TopBarBehindSystemControlsAreaWidth));
            }
        }

        private double topBarButtonHeight;
        public double TopBarButtonHeight
        {
            get => topBarButtonHeight;
            set
            {
                topBarButtonHeight = value;
                FirePropertyChangedEvent(nameof(TopBarButtonHeight));
            }
        }

        public double TopBarBehindSystemControlsAreaWidth
        {
            get
            {
                var isTabletMode = (UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Touch);

                return TopBarButtonWidth * (isTabletMode ? 1.0 : 3.0);
            }
        }
    }
}
