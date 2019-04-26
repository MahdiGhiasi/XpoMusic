﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}