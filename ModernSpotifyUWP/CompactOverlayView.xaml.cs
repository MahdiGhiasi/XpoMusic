using ModernSpotifyUWP.Classes;
using ModernSpotifyUWP.Helpers;
using ModernSpotifyUWP.SpotifyApi;
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

        private DispatcherTimer timer;
        private string artistArtUrl;
        private string albumArtUrl;

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
            songName.Text = PlayStatusTracker.LastPlayStatus.SongName;
            artistName.Text = PlayStatusTracker.LastPlayStatus.ArtistName;

            if (progressBar.Value != progressBar.Maximum && PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds == progressBar.Maximum)
            {
                // Song just reached the end. refresh status now.
                RefreshPlayStatus();
            }

            if (progressBar.Maximum != PlayStatusTracker.LastPlayStatus.SongLengthMilliseconds)
            {
                progressBar.Value = 0;
                progressBar.Maximum = PlayStatusTracker.LastPlayStatus.SongLengthMilliseconds;
            }
            progressBar.Value = PlayStatusTracker.LastPlayStatus.ProgressedMilliseconds;

            if (PlayStatusTracker.LastPlayStatus.IsPlaying)
            {
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
            }
            else
            {
                playButton.Visibility = Visibility.Visible;
                pauseButton.Visibility = Visibility.Collapsed;
            }

            var artistArtUrl = await SongImageProvider.GetArtistArt(PlayStatusTracker.LastPlayStatus.ArtistId);
            var albumArtUrl = await SongImageProvider.GetAlbumArt(PlayStatusTracker.LastPlayStatus.AlbumId);

            if (artistArtUrl != this.artistArtUrl)
            {
                artistArt.Source = new BitmapImage(new Uri(artistArtUrl));
                this.artistArtUrl = artistArtUrl;
            }

            if (albumArtUrl != this.albumArtUrl)
            {
                albumArt.Source = new BitmapImage(new Uri(albumArtUrl));
                this.albumArtUrl = albumArtUrl;
            }

        }

        private async void RefreshPlayStatus()
        {
            await Task.Delay(1000);
            await PlayStatusTracker.RefreshPlayStatus();
        }

        private void CloseCompactOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();
            ExitCompactOverlayRequested?.Invoke(this, new EventArgs());
        }

        private async void PauseButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await (new Player()).Pause();

                timer.Stop();
                playButton.Visibility = Visibility.Visible;
                pauseButton.Visibility = Visibility.Collapsed;
                await Task.Delay(500);
                await PlayStatusTracker.RefreshPlayStatus();
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
            finally
            {
                timer.Start();
            }
        }

        private async void PlayButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await (new Player()).ResumePlaying();

                timer.Stop();
                playButton.Visibility = Visibility.Collapsed;
                pauseButton.Visibility = Visibility.Visible;
                await Task.Delay(500);
                await PlayStatusTracker.RefreshPlayStatus();
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
            finally
            {
                timer.Start();
            }
        }

        private async void PrevTrackButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                await (new Player()).PreviousTrack();

                (sender as Control).IsEnabled = false;
                await Task.Delay(1000);
                await PlayStatusTracker.RefreshPlayStatus();
                (sender as Control).IsEnabled = true;
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
                await (new Player()).NextTrack();

                (sender as Control).IsEnabled = false;
                await Task.Delay(1000);
                await PlayStatusTracker.RefreshPlayStatus();
                (sender as Control).IsEnabled = true;
            }
            catch (UnauthorizedAccessException)
            {
                UnauthorizedHelper.OnUnauthorizedError();
            }
        }
    }
}
