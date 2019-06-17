using Xpotify.Classes;
using Xpotify.Helpers;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Xpotify.Controls
{
    public sealed partial class NowPlayingView : UserControl
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Enums
        public enum NowPlayingViewMode
        {
            Normal,
            CompactOverlay,
        }

        public enum Action
        {
            Back,
            FullScreen,
            PlayQueue,
            MiniView,
        }

        private enum AnimationState
        {
            None,
            HiddenToRightSide,
            HiddenToLeftSide,
        }
        #endregion

        public event EventHandler<Action> ActionRequested;

        public FrameworkElement MainBackgroundControl => this.mainBackgroundGrid;

        #region Custom Properties
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof(bool), typeof(NowPlayingView), new PropertyMetadata(defaultValue: false,
                propertyChangedCallback: new PropertyChangedCallback(OnIsOpenPropertyChanged)));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set
            {
                if (IsOpen != value)
                    SetValue(IsOpenProperty, value);

                if (IsOpen && !timer.IsEnabled)
                {
                    SetTopBar();
                    TryUpdate();
                    timer.Start();
                }
                else if (!IsOpen && timer.IsEnabled)
                {
                    OnViewClosed();
                    timer.Stop();
                }

                mainGrid.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NowPlayingView).IsOpen = (bool)e.NewValue;
        }

        public static readonly DependencyProperty ViewModeProperty = DependencyProperty.Register(
            "ViewMode", typeof(NowPlayingViewMode), typeof(NowPlayingView), new PropertyMetadata(defaultValue: NowPlayingViewMode.Normal,
                propertyChangedCallback: new PropertyChangedCallback(OnViewModePropertyChanged)));

        public NowPlayingViewMode ViewMode
        {
            get => (NowPlayingViewMode)GetValue(ViewModeProperty);
            set
            {
                if (ViewMode != value)
                    SetValue(ViewModeProperty, value);

                ViewModel.ViewMode = value;
            }
        }

        private static void OnViewModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as NowPlayingView).ViewMode = (NowPlayingViewMode)e.NewValue;
        }
        #endregion

        /// <summary>
        /// If this variable is set, track change animation will run in reverse direction as a song change is detected.
        /// NOTE: DO NOT set this variable directly to true, use SetPrevTrackCommandIssued() method, which reverts the
        /// change automatically after 4 seconds.
        /// </summary>
        private bool prevTrackCommandIssued = false;

        private AnimationState animationState = AnimationState.None;

        private DateTime spinnerShowTime = DateTime.MaxValue;
        private readonly TimeSpan maximumSpinnerShowTime = TimeSpan.FromSeconds(7);

        private DispatcherTimer timer;
        private string currentSongId = "";

        public NowPlayingView()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        public void ActivateProgressRing()
        {
            ViewModel.ProgressRingActive = true;
        }

        private void Timer_Tick(object sender, object e)
        {
            TryUpdate();
        }

        private async void TryUpdate()
        {
            try
            {
                await Update();
            }
            catch (Exception ex)
            {
                logger.Warn("Update failed: " + ex.ToString());
            }
        }

        private async Task Update()
        {
            if (ViewModel.ProgressBarValue != ViewModel.ProgressBarMaximum && PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds == ViewModel.ProgressBarMaximum)
            {
                // Song just reached the end. refresh status now.
                RefreshPlayStatus();
            }

            ViewModel.ProgressBarMaximum = PlayStatusTracker.LastPlayStatus.SongLengthMilliseconds;
            ViewModel.ProgressBarValue = PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds;

            ViewModel.IsPlaying = PlayStatusTracker.LastPlayStatus.IsPlaying;

            if (currentSongId != PlayStatusTracker.LastPlayStatus.SongId)
            {
                currentSongId = PlayStatusTracker.LastPlayStatus.SongId;

                if (animationState == AnimationState.None)
                {
                    if (prevTrackCommandIssued)
                    {
                        prevTrackCommandIssued = false;

                        animationState = AnimationState.HiddenToRightSide;
                        hideToRightStoryboard.Begin();
                        await Task.Delay(300);
                    }
                    else
                    {
                        hideToLeftStoryboard.Begin();
                        await Task.Delay(300);
                    }
                }

                ViewModel.SongName = PlayStatusTracker.LastPlayStatus.SongName;
                ViewModel.AlbumName = PlayStatusTracker.LastPlayStatus.AlbumName;
                ViewModel.ArtistName = PlayStatusTracker.LastPlayStatus.ArtistName;

                var artistArtUrl = await SongImageProvider.GetArtistArt(PlayStatusTracker.LastPlayStatus.ArtistId);
                var albumArtUrl = await SongImageProvider.GetAlbumArt(PlayStatusTracker.LastPlayStatus.AlbumId);

                ViewModel.ArtistArtUri = new Uri(artistArtUrl);
                ViewModel.AlbumArtUri = new Uri(albumArtUrl);

                if (animationState == AnimationState.HiddenToRightSide)
                    showFromLeftStoryboard.Begin();
                else // None or HiddenToLeftSide
                    showFromRightStoryboard.Begin();

                ViewModel.ProgressRingActive = false;
                animationState = AnimationState.None;
            }
            else if ((DateTime.UtcNow - spinnerShowTime) > maximumSpinnerShowTime 
                && ViewModel.ProgressRingActive)
            {
                // Workaround for when prev track gets pushed when no prev track is there,
                // or in general when a command fails.
                // (progress ring should not be shown forever!)

                if (animationState == AnimationState.HiddenToRightSide)
                    showFromRightStoryboard.Begin();
                else // None or HiddenToLeftSide
                    showFromLeftStoryboard.Begin();

                ViewModel.ProgressRingActive = false;
                animationState = AnimationState.None;
            }
        }

        private void SetTopBar()
        {
            Window.Current.SetTitleBar(titleBarArea);
        }

        private void OnViewClosed()
        {
            ViewModel.ArtistArtUri = null;
            ViewModel.AlbumArtUri = null;
            animationState = AnimationState.HiddenToLeftSide;
            currentSongId = "";
        }

        private async void RefreshPlayStatus()
        {
            await Task.Delay(1000);
            await PlayStatusTracker.RefreshPlayStatus();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ActionRequested?.Invoke(this, Action.Back);
        }

        private async void PauseButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                ViewModel.IsPlaying = false;
                ViewModel.PlayPauseButtonEnabled = false;
                timer.Stop();

                // This delay is a workaround for PlayPauseButtonEnabled to
                // update the newly appeared UI. Otherwize it appears after
                // running PlaybackActionHelper.Play().
                await Task.Delay(100);

                if (await PlaybackActionHelper.Pause())
                {
                    await Task.Delay(1000);
                    await PlayStatusTracker.RefreshPlayStatus();
                    ViewModel.PlayPauseButtonEnabled = true;
                }
                else
                {
                    ViewModel.IsPlaying = true;
                    ViewModel.PlayPauseButtonEnabled = true;

                    await PlayStatusTracker.RefreshPlayStatus();
                }
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
            finally
            {
                ViewModel.PlayPauseButtonEnabled = true;
                if (!timer.IsEnabled)
                    timer.Start();
            }
        }

        private async void PlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                ViewModel.IsPlaying = true;
                ViewModel.PlayPauseButtonEnabled = false;
                timer.Stop();

                // This delay is a workaround for PlayPauseButtonEnabled to
                // update the newly appeared UI. Otherwize it appears after
                // running PlaybackActionHelper.Play().
                await Task.Delay(100);

                if (await PlaybackActionHelper.Play())
                {
                    await Task.Delay(1000);
                    await PlayStatusTracker.RefreshPlayStatus();
                    ViewModel.PlayPauseButtonEnabled = true;
                }
                else
                {
                    ViewModel.IsPlaying = false;
                    ViewModel.PlayPauseButtonEnabled = true;

                    await PlayStatusTracker.RefreshPlayStatus();
                }
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
            finally
            {
                ViewModel.PlayPauseButtonEnabled = true;
                if (!timer.IsEnabled)
                    timer.Start();
            }
        }

        private async void PrevTrackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                //HideToRightAnimation();

                if (!(await PlaybackActionHelper.PreviousTrack()))
                {
                    //showFromRightStoryboard.Begin();
                    animationState = AnimationState.None;
                    ViewModel.ProgressRingActive = false;
                    return;
                }

                SetPrevTrackCommandIssued();
                ViewModel.PrevButtonEnabled = false;
                await Task.Delay(1000);
                await PlayStatusTracker.RefreshPlayStatus();
                ViewModel.PrevButtonEnabled = true;
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
        }

        private async void NextTrackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                HideToLeftAnimation();

                if (!(await PlaybackActionHelper.NextTrack()))
                {
                    showFromLeftStoryboard.Begin();
                    animationState = AnimationState.None;
                    ViewModel.ProgressRingActive = false;
                    return;
                }

                ViewModel.NextButtonEnabled = false;
                await Task.Delay(1000);
                await PlayStatusTracker.RefreshPlayStatus();
                ViewModel.NextButtonEnabled = true;
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
        }

        private async void HideToRightAnimation()
        {
            hideToRightStoryboard.Begin();
            animationState = AnimationState.HiddenToRightSide;
            spinnerShowTime = DateTime.UtcNow;

            await Task.Delay(300);
            ViewModel.ProgressRingActive = true;
        }

        private async void HideToLeftAnimation()
        {
            hideToLeftStoryboard.Begin();
            animationState = AnimationState.HiddenToLeftSide;
            spinnerShowTime = DateTime.UtcNow;

            await Task.Delay(300);
            ViewModel.ProgressRingActive = true;
        }

        public void PlayChangeTrackAnimation(bool reverse)
        {
            if (reverse)
            {
                SetPrevTrackCommandIssued();
            }
            else
            {
                HideToLeftAnimation();
            }
        }

        private Guid lastPrevTrackCommandGuid = Guid.Empty;
        private async void SetPrevTrackCommandIssued()
        {
            var commandGuid = Guid.NewGuid();
            lastPrevTrackCommandGuid = commandGuid;

            // This is only valid if a change is detected in the next 4 seconds,
            // otherwise, its value is returned to false.
            prevTrackCommandIssued = true;
            await Task.Delay(TimeSpan.FromSeconds(4));

            if (lastPrevTrackCommandGuid != commandGuid)
                return; // A newer prev command has arrived during the last seconds, so we won't invalidate the prevTrackCommandIssued now.
            
            prevTrackCommandIssued = false;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Update();
                await PlayStatusTracker.RefreshPlayStatus();
            }
            catch (Exception ex)
            {
                logger.Warn("Loaded event failed: " + ex.ToString());
            }
        }

        private void FullScreenButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ActionRequested?.Invoke(this, Action.FullScreen);
        }

        private void MiniViewButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ActionRequested?.Invoke(this, Action.MiniView);
        }

        private void ShowNowPlayingListButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ActionRequested?.Invoke(this, Action.PlayQueue);
        }
    }
}
