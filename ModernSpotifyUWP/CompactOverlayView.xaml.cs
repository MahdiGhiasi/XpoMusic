using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Helpers;
using ModernSpotifyUWP.SpotifyApi;
using ModernSpotifyUWP.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace ModernSpotifyUWP
{
    public sealed partial class CompactOverlayView : UserControl
    {
        public event EventHandler ExitCompactOverlayRequested;

        //private CompactOverlayViewModel ViewModel => this.DataContext as CompactOverlayViewModel;

        private DispatcherTimer timer;
        private string currentSongId;

        private enum AnimationState
        {
            None,
            HiddenToRightSide,
            HiddenToLeftSide,
        }
        private AnimationState animationState = AnimationState.None;

        private DateTime spinnerShowTime = DateTime.MaxValue;
        private readonly TimeSpan maximumSpinnerShowTime = TimeSpan.FromSeconds(7);

        public CompactOverlayView()
        {
            this.InitializeComponent();

            Update();

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1),
            };
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private async void Timer_Tick(object sender, object e)
        {
            try
            {
                await Update();
            }
            catch { }
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
                    hideToLeftStoryboard.Begin();
                    await Task.Delay(300);
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

        private async void RefreshPlayStatus()
        {
            await Task.Delay(1000);
            await PlayStatusTracker.RefreshPlayStatus();
        }

        public void PrepareToExit()
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();
        }

        private void CloseCompactOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            ExitCompactOverlayRequested?.Invoke(this, new EventArgs());
        }

        private async void PauseButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                ViewModel.IsPlaying = false;

                if (await (new Player()).Pause())
                {
                    timer.Stop();
                    await Task.Delay(500);
                    await PlayStatusTracker.RefreshPlayStatus();
                }
                else
                {
                    ViewModel.IsPlaying = true;

                    await PlayStatusTracker.RefreshPlayStatus();
                }
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
            finally
            {
                if (!timer.IsEnabled)
                    timer.Start();
            }
        }

        private async void PlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                ViewModel.IsPlaying = true;

                if (await (new Player()).ResumePlaying())
                {
                    timer.Stop();
                    await Task.Delay(500);
                    await PlayStatusTracker.RefreshPlayStatus();
                }
                else
                {
                    ViewModel.IsPlaying = false;

                    await PlayStatusTracker.RefreshPlayStatus();
                }
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
            finally
            {
                if (!timer.IsEnabled)
                    timer.Start();
            }
        }

        private async void PrevTrackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                HideToRightAnimation();

                if (!(await (new Player()).PreviousTrack()))
                {
                    showFromRightStoryboard.Begin();
                    animationState = AnimationState.None;
                    ViewModel.ProgressRingActive = false;
                    return;
                }

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

                if (!(await (new Player()).NextTrack()))
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
                HideToRightAnimation();
            }
            else
            {
                HideToLeftAnimation();
            }
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Update();
                await PlayStatusTracker.RefreshPlayStatus();
            }
            catch { }
        }
    }
}
