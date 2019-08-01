using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using XpoMusic.Classes;
using static XpoMusic.Controls.NowPlayingView;

namespace XpoMusic.ViewModels
{
    public class NowPlayingViewModel : ViewModelBase
    {
        private NowPlayingViewMode viewMode;
        public NowPlayingViewMode ViewMode
        {
            get
            {
                return viewMode;
            }
            set
            {
                viewMode = value;
                FirePropertyChangedEvent(nameof(ViewMode));
                FirePropertyChangedEvent(nameof(IsCompactOverlayViewMode));
                FirePropertyChangedEvent(nameof(IsNormalViewMode));
                FirePropertyChangedEvent(nameof(ShowArtistArt));
            }
        }

        public bool IsCompactOverlayViewMode => ViewMode == NowPlayingViewMode.CompactOverlay;
        public bool IsNormalViewMode => ViewMode == NowPlayingViewMode.Normal;

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

        private Uri backgroundArtUri;
        public Uri BackgroundArtUri
        {
            get
            {
                return backgroundArtUri;
            }
            set
            {
                if (backgroundArtUri != value)
                {
                    backgroundArtUri = value;
                    backgroundArt = (value == null) ? null : new BitmapImage(value);

                    FirePropertyChangedEvent(nameof(BackgroundArtUri));
                    FirePropertyChangedEvent(nameof(BackgroundArt));
                }
            }
        }

        private ImageSource backgroundArt;
        public ImageSource BackgroundArt
        {
            get
            {
                return backgroundArt;
            }
        }

        private Uri albumArtUri;
        public Uri AlbumArtUri
        {
            get
            {
                return albumArtUri;
            }
            set
            {
                if (albumArtUri != value)
                {
                    AlbumArtContainerOpacity = 0.0;

                    albumArtUri = value;
                    albumArt = (value == null) ? null : new BitmapImage(value);

                    FirePropertyChangedEvent(nameof(AlbumArtUri));
                    FirePropertyChangedEvent(nameof(AlbumArt));
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


        private bool playPauseButtonEnabled = true;
        public bool PlayPauseButtonEnabled
        {
            get
            {
                return playPauseButtonEnabled;
            }
            set
            {
                playPauseButtonEnabled = value;
                FirePropertyChangedEvent(nameof(PlayPauseButtonEnabled));
            }
        }

        private double albumArtContainerOpacity = 0.0;
        public double AlbumArtContainerOpacity
        {
            get
            {
                return albumArtContainerOpacity;
            }
            set
            {
                albumArtContainerOpacity = value;
                FirePropertyChangedEvent(nameof(AlbumArtContainerOpacity));
            }
        }

        double storyboardOffset;
        public double StoryboardOffset
        {
            get => storyboardOffset;
            set
            {
                storyboardOffset = value;
                FirePropertyChangedEvent(nameof(StoryboardOffset));
                FirePropertyChangedEvent(nameof(StoryboardOffsetNegative));
            }
        }

        public double StoryboardOffsetNegative => -StoryboardOffset;

        public bool ShowArtistArt
        {
            get
            {
                if (ViewMode == NowPlayingViewMode.CompactOverlay)
                    return LocalConfiguration.MiniViewShowArtistArt;
                else
                    return LocalConfiguration.NowPlayingShowArtistArt;
            }
            set
            {
                if (ViewMode == NowPlayingViewMode.CompactOverlay)
                    LocalConfiguration.MiniViewShowArtistArt = value;
                else
                    LocalConfiguration.NowPlayingShowArtistArt = value;

                FirePropertyChangedEvent(nameof(ShowArtistArt));
            }
        }

        private bool blurEnabled = false;
        public bool BlurEnabled
        {
            get => blurEnabled;
            set
            {
                blurEnabled = value;
                FirePropertyChangedEvent(nameof(BlurEnabled));
            }
        }

        public double volume = 50;
        public double Volume
        {
            get => volume;
            set
            {
                volume = value;
                FirePropertyChangedEvent(nameof(Volume));
                FirePropertyChangedEvent(nameof(VolumeIconGlyph));
            }
        }

        public string VolumeIconGlyph
        {
            get
            {
                if (Volume > 66)
                    return "\uE995";
                else if (Volume > 33)
                    return "\uE994";
                else if (Volume > 0)
                    return "\uE993";
                else
                    return "\uE992";
            }
        }

        private bool isSavedToLibrary = false;
        public bool IsSavedToLibrary
        {
            get => isSavedToLibrary;
            set
            {
                isSavedToLibrary = value;
                FirePropertyChangedEvent(nameof(IsSavedToLibrary));
            }
        }
    }
}
