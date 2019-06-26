using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Xpotify.Classes;
using Xpotify.Classes.Cache;
using Xpotify.Classes.Model;
using Xpotify.Helpers;
using Xpotify.SpotifyApi;

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
            PlayQueue,
            SeekPlayback,
            SeekVolume,
        }

        private enum AnimationState
        {
            None,
            HiddenToRightSide,
            HiddenToLeftSide,
        }
        #endregion

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
                    songInfoCache.Clear();
                    HideNameAndAlbumArtQuick();
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

        public event EventHandler<ActionRequestedEventArgs> ActionRequested;
        public FrameworkElement MainBackgroundControl => this.mainBackgroundGrid;

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
        private bool? currentShowArtistArtState = null;
        private bool isCompactOverlayFromNowPlaying = false;
        
        private SemaphoreQueue volumeSetSemaphore = new SemaphoreQueue(1, 1);

        private SongExtraInfoStore songInfoCache = new SongExtraInfoStore();

        public NowPlayingView()
        {
            this.InitializeComponent();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ShowArtistArt")
                TryUpdate();
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
                if (!IsOpen)
                    return;

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

            ViewModel.Volume = PlayStatusTracker.LastPlayStatus.Volume * 100;

            ViewModel.IsPlaying = PlayStatusTracker.LastPlayStatus.IsPlaying;

            ViewModel.PrevButtonEnabled = true;
            ViewModel.NextButtonEnabled = PlayStatusTracker.LastPlayStatus.IsNextTrackAvailable;

            if (currentSongId != PlayStatusTracker.LastPlayStatus.SongId
                || currentShowArtistArtState != ViewModel.ShowArtistArt)
            {
                currentSongId = PlayStatusTracker.LastPlayStatus.SongId;
                currentShowArtistArtState = ViewModel.ShowArtistArt;

                if (animationState == AnimationState.None)
                {
                    if (prevTrackCommandIssued)
                    {
                        prevTrackCommandIssued = false;

                        animationState = AnimationState.HiddenToRightSide;
                        await hideToRightStoryboard.RunAsync();
                    }
                    else
                    {
                        animationState = AnimationState.HiddenToLeftSide;
                        await hideToLeftStoryboard.RunAsync();
                    }
                }

                ViewModel.SongName = PlayStatusTracker.LastPlayStatus.SongName;
                ViewModel.AlbumName = PlayStatusTracker.LastPlayStatus.AlbumName;
                ViewModel.ArtistName = PlayStatusTracker.LastPlayStatus.ArtistName;

                var albumArtUrl = await SongImageProvider.GetAlbumArt(PlayStatusTracker.LastPlayStatus.AlbumId);
                ViewModel.AlbumArtUri = new Uri(albumArtUrl);

                if (ViewModel.ShowArtistArt)
                {
                    var artistArtUrl = await SongImageProvider.GetArtistArt(PlayStatusTracker.LastPlayStatus.ArtistId);
                    ViewModel.BackgroundArtUri = new Uri(artistArtUrl);
                    ViewModel.BlurEnabled = false;
                }
                else
                {
                    ViewModel.BackgroundArtUri = new Uri(albumArtUrl);
                    ViewModel.BlurEnabled = true;
                }

                await Task.Delay(200);

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

            try
            {
                var extraInfo = await songInfoCache.GetItem(currentSongId);
                ViewModel.IsSavedToLibrary = extraInfo.IsSavedToLibrary;
            }
            catch (Exception ex)
            {
                logger.Warn("GetSongExtraInfo failed: " + ex.ToString());
            }
        }

        private void SetTopBar()
        {
            Window.Current.SetTitleBar(titleBarArea);
        }

        private void OnViewClosed()
        {
            ViewModel.BackgroundArtUri = null;
            ViewModel.AlbumArtUri = null;
            HideNameAndAlbumArtQuick();
            currentSongId = "";
            currentShowArtistArtState = null;
        }

        private void HideNameAndAlbumArtQuick()
        {
            hideQuickStoryboard.Begin();
            animationState = AnimationState.HiddenToLeftSide;
        }

        private async void RefreshPlayStatus()
        {
            await Task.Delay(1000);
            await PlayStatusTracker.RefreshPlayStatus();
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                AnalyticsHelper.PageView("NowPlaying");
                view.ExitFullScreenMode();
                return;
            }

            if (isCompactOverlayFromNowPlaying)
            {
                HideNameAndAlbumArtQuick();
                currentSongId = "";

                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

                isCompactOverlayFromNowPlaying = false;
                ViewMode = NowPlayingViewMode.Normal;

                AnalyticsHelper.PageView("NowPlaying");

                return;
            }

            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Action.Back,
            });
        }

        private async void PauseButton_Click(object sender, RoutedEventArgs e)
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
                    RefreshPlayStatus();
                    ViewModel.PlayPauseButtonEnabled = true;
                }
                else
                {
                    ViewModel.IsPlaying = true;
                    ViewModel.PlayPauseButtonEnabled = true;

                    RefreshPlayStatus();
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

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
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
                    RefreshPlayStatus();
                    ViewModel.PlayPauseButtonEnabled = true;
                }
                else
                {
                    ViewModel.IsPlaying = false;
                    ViewModel.PlayPauseButtonEnabled = true;

                    RefreshPlayStatus();
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

        private async void PrevTrackButton_Click(object sender, RoutedEventArgs e)
        {
            if (PlayStatusTracker.LastPlayStatus.IsPrevTrackAvailable)
                await PrevTrack(canGoToBeginningOfCurrentSong: true);
            else
                ActionRequested?.Invoke(this, new ActionRequestedEventArgs
                {
                    Action = Action.SeekPlayback,
                    AdditionalData = 0.0,
                });
        }

        private async Task PrevTrack(bool canGoToBeginningOfCurrentSong)
        {
            try
            {
                if (!canGoToBeginningOfCurrentSong)
                    HideToRightAnimation();

                if (!(await PlaybackActionHelper.PreviousTrack(canGoToBeginningOfCurrentSong)))
                {
                    if (!canGoToBeginningOfCurrentSong)
                        showFromRightStoryboard.Begin();
                    animationState = AnimationState.None;
                    ViewModel.ProgressRingActive = false;
                    return;
                }

                SetPrevTrackCommandIssued();
                ViewModel.PrevButtonEnabled = false;
                RefreshPlayStatus();
                ViewModel.PrevButtonEnabled = true;
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
        }

        private async void NextTrackButton_Click(object sender, RoutedEventArgs e)
        {
            await NextTrack();
        }

        private async Task NextTrack()
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
                RefreshPlayStatus();
                ViewModel.NextButtonEnabled = PlayStatusTracker.LastPlayStatus.IsNextTrackAvailable;
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
        }

        private async void HideToRightAnimation()
        {
            animationState = AnimationState.HiddenToRightSide;
            spinnerShowTime = DateTime.UtcNow;

            ViewModel.ProgressRingActive = true;

            await hideToRightStoryboard.RunAsync();
        }

        private async void HideToLeftAnimation()
        {
            animationState = AnimationState.HiddenToLeftSide;
            spinnerShowTime = DateTime.UtcNow;

            ViewModel.ProgressRingActive = true;

            await hideToLeftStoryboard.RunAsync();
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
                RefreshPlayStatus();
            }
            catch (Exception ex)
            {
                logger.Warn("Loaded event failed: " + ex.ToString());
            }
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleFullscreen();
        }

        public void ToggleFullscreen()
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                AnalyticsHelper.PageView("NowPlaying");
                view.ExitFullScreenMode();
            }
            else
            {
                AnalyticsHelper.PageView("NowPlayingFullScreen");
                view.TryEnterFullScreenMode();
            }
        }

        private async void MiniViewButton_Click(object sender, RoutedEventArgs e)
        {
            await SwitchToMiniView();
        }

        public async Task SwitchToMiniView()
        {
            if (ViewMode == NowPlayingViewMode.CompactOverlay)
                return;

            var viewMode = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            viewMode.ViewSizePreference = ViewSizePreference.Custom;
            viewMode.CustomSize = LocalConfiguration.CompactOverlaySize;

            var modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, viewMode);

            if (modeSwitched)
            {
                HideNameAndAlbumArtQuick();
                currentSongId = "";
                ViewMode = NowPlayingViewMode.CompactOverlay;
                isCompactOverlayFromNowPlaying = true;
                AnalyticsHelper.PageView("CompactOverlay");
            }
        }

        private void ShowNowPlayingListButton_Click(object sender, RoutedEventArgs e)
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Action.PlayQueue,
            });
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ViewModel.StoryboardOffset = e.NewSize.Width;
        }

        private void PlaybackSlider_ValueChangedByUserManipulation(object sender, SliderExtendedValueChangedEventArgs e)
        {
            ActionRequested?.Invoke(this, new ActionRequestedEventArgs
            {
                Action = Action.SeekPlayback,
                AdditionalData = (e.NewValue / ViewModel.ProgressBarMaximum),
            });
        }

        private async void VolumeSlider_ValueChangedByUserManipulation(object sender, SliderExtendedValueChangedEventArgs e)
        {
            try
            {
                await volumeSetSemaphore.WaitAsync();

                // There are new shiny requests waiting, so I'm not needed anymore :(
                if (volumeSetSemaphore.WaitingCount > 0)
                    return;

                ActionRequested?.Invoke(this, new ActionRequestedEventArgs
                {
                    Action = Action.SeekVolume,
                    AdditionalData = e.NewValue / 100.0,
                });

                // Don't overload the SeekVolume request.
                await Task.Delay(200);
            }
            finally
            {
                volumeSetSemaphore.Release();
            }
        }

        private async void SaveToYourLibrary_Click(object sender, RoutedEventArgs e)
        {
            var extraInfo = await songInfoCache.GetItem(currentSongId);
            extraInfo.IsSavedToLibrary = true;
            ViewModel.IsSavedToLibrary = true;

            var library = new Library();
            var result = await library.SaveTrackToLibrary(currentSongId);

            extraInfo.IsSavedToLibrary = result;
            ViewModel.IsSavedToLibrary = result;
        }

        private async void RemoveFromYourLibrary_Click(object sender, RoutedEventArgs e)
        {
            var extraInfo = await songInfoCache.GetItem(currentSongId);
            extraInfo.IsSavedToLibrary = false;
            ViewModel.IsSavedToLibrary = false;

            var library = new Library();
            var result = await library.RemoveTrackFromLibrary(currentSongId);

            extraInfo.IsSavedToLibrary = !result;
            ViewModel.IsSavedToLibrary = !result;
        }

        #region Swipe Gesture
        const double _minimumDeltaXForSwipe = 20.0;
        Point? pressStartPoint = null, lastPoint = null;
        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            pressStartPoint = e.GetCurrentPoint(null).Position;
        }

        private void UserControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!pressStartPoint.HasValue)
                return;

            lastPoint = e.GetCurrentPoint(null).Position;
            var movement = lastPoint.Value.X - pressStartPoint.Value.X;

            if ((!PlayStatusTracker.LastPlayStatus.IsPrevTrackAvailable && movement > 0) ||
                (!PlayStatusTracker.LastPlayStatus.IsNextTrackAvailable && movement < 0))
            {
                movement = 0;
                pressStartPoint = lastPoint;
            }

            swipeTranslateTransform.X = movement;
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (!pressStartPoint.HasValue)
                return;

            SwipeFinished(e.GetCurrentPoint(null).Position.X);
        }

        private async void SwipeFinished(double finalX)
        {
            var deltaX = finalX - pressStartPoint.Value.X;
            pressStartPoint = null;

            if (Math.Abs(deltaX) > _minimumDeltaXForSwipe)
            {
                if (deltaX > 0)
                    await PrevTrack(canGoToBeginningOfCurrentSong: false);
                else
                    await NextTrack();
            }
            else
            {
                swipeTranslateTransform.X = 0;
            }
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (!pressStartPoint.HasValue)
                return;
            if (!lastPoint.HasValue)
                return;

            SwipeFinished(lastPoint.Value.X);
        }
        #endregion
    }

    public class ActionRequestedEventArgs
    {
        public NowPlayingView.Action Action { get; set; }
        public object AdditionalData { get; set; }
    }
}
