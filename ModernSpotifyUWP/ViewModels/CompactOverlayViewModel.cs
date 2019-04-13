using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ModernSpotifyUWP.ViewModels
{
    public class CompactOverlayViewModel : ViewModelBase
    {
        private string artistName;
        public string ArtistName
        {
            get
            {
                return artistName;
            }
            set
            {
                if (artistName != value)
                {
                    artistName = value;
                    FirePropertyChangedEvent(nameof(ArtistName));
                }
            }
        }

        private string albumName;
        public string AlbumName
        {
            get
            {
                return albumName;
            }
            set
            {
                if (albumName != value)
                {
                    albumName = value;
                    FirePropertyChangedEvent(nameof(AlbumName));
                }
            }
        }

        private string songName;
        public string SongName
        {
            get
            {
                return songName;
            }
            set
            {
                if (songName != value)
                {
                    songName = value;
                    FirePropertyChangedEvent(nameof(SongName));
                }
            }
        }

        private ImageSource artistArt;
        public ImageSource ArtistArt
        {
            get
            {
                return artistArt;
            }
            set
            {
                if (artistArt != value)
                {
                    artistArt = value;
                    FirePropertyChangedEvent(nameof(ArtistArt));
                }
            }
        }

        private ImageSource albumArt;
        public ImageSource AlbumArt
        {
            get
            {
                return albumArt;
            }
            set
            {
                if (albumArt != value)
                {
                    albumArt = value;
                    FirePropertyChangedEvent(nameof(AlbumArt));
                }
            }
        }

        private bool isPlaying = false;
        public bool IsPlaying
        {
            get
            {
                return isPlaying;
            }
            set
            {
                isPlaying = value;
                FirePropertyChangedEvent(nameof(PlayButtonVisibility));
                FirePropertyChangedEvent(nameof(PauseButtonVisibility));
            }
        }

        public Visibility PlayButtonVisibility => IsPlaying ? Visibility.Collapsed : Visibility.Visible;
        public Visibility PauseButtonVisibility => IsPlaying ? Visibility.Visible : Visibility.Collapsed;

        private double progressBarValue = 0.0;
        public double ProgressBarValue
        {
            get
            {
                return progressBarValue;
            }
            set
            {
                progressBarValue = value;

                if (progressBarValue <= ProgressBarMaximum)
                {
                    FirePropertyChangedEvent(nameof(ProgressBarValue));
                    FirePropertyChangedEvent(nameof(ProgressBarMaximum));
                }
            }
        }

        private double progressBarMaximum = 100.0;
        public double ProgressBarMaximum
        {
            get
            {
                return progressBarMaximum;
            }
            set
            {
                progressBarMaximum = value;

                if (ProgressBarValue <= progressBarMaximum)
                {
                    FirePropertyChangedEvent(nameof(ProgressBarValue));
                    FirePropertyChangedEvent(nameof(ProgressBarMaximum));
                }
            }
        }

        private bool progressRingActive = false;
        public bool ProgressRingActive
        {
            get
            {
                return progressRingActive;
            }
            set
            {
                progressRingActive = value;
                FirePropertyChangedEvent(nameof(ProgressRingActive));
            }
        }

        private bool prevButtonEnabled = true;
        public bool PrevButtonEnabled
        {
            get
            {
                return prevButtonEnabled;
            }
            set
            {
                prevButtonEnabled = value;
                FirePropertyChangedEvent(nameof(PrevButtonEnabled));
            }
        }

        private bool nextButtonEnabled = true;
        public bool NextButtonEnabled
        {
            get
            {
                return nextButtonEnabled;
            }
            set
            {
                nextButtonEnabled = value;
                FirePropertyChangedEvent(nameof(NextButtonEnabled));
            }
        }
    }
}
