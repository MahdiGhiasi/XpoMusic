using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace XpoMusic.ViewModels
{
    public class TopBarViewModel : ViewModelBase
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

        private Visibility openLinkFromClipboardVisibility;
        public Visibility OpenLinkFromClipboardVisibility
        {
            get
            {
                if (!ShowExtraButtons)
                    return Visibility.Collapsed;

                return openLinkFromClipboardVisibility;
            }
            set
            {
                openLinkFromClipboardVisibility = value;
                FirePropertyChangedEvent(nameof(OpenLinkFromClipboardVisibility));
            }
        }

        private bool showExtraButtons;
        public bool ShowExtraButtons
        {
            get => showExtraButtons;
            set
            {
                showExtraButtons = value;
                FirePropertyChangedEvent(nameof(ShowExtraButtons));
                FirePropertyChangedEvent(nameof(OpenLinkFromClipboardVisibility));
            }
        }
    }
}
