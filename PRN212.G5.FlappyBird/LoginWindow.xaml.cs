using PRN212.G5.FlappyBird.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Path = System.IO.Path;

namespace PRN212.G5.FlappyBird
{
    // Bird Skin Model
    public class BirdSkin
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string DayFlyFrame { get; set; } = string.Empty;
        public string DayFallFrame { get; set; } = string.Empty;
        public string NightFlyFrame { get; set; } = string.Empty;
        public string NightFallFrame { get; set; } = string.Empty;
        public string PreviewImage { get; set; } = string.Empty;
        public bool IsUnlocked { get; set; } = true;
    }

    // Static Skin Manager
    public static class SkinManager
    {
        private static int selectedSkinId = 1;

        public static List<BirdSkin> AvailableSkins { get; } = new()
        {
            new BirdSkin
            {
                Id = 1,
                Name = "Yellow",
                DisplayName = "Classic Yellow Bird",
                DayFlyFrame = "birdfly-1.png",
                DayFallFrame = "birdfall-1.png",
                NightFlyFrame = "birdfly-3.png",
                NightFallFrame = "birdfall-3.png",
                PreviewImage = "birdfly-1.png",
                IsUnlocked = true
            },
            new BirdSkin
            {
                Id = 2,
                Name = "Green",
                DisplayName = "Green Bird",
                DayFlyFrame = "birdfly-2.png",
                DayFallFrame = "birdfall-2.png",
                NightFlyFrame = "birdfly-2.png",
                NightFallFrame = "birdfall-2.png",
                PreviewImage = "birdfly-2.png",
                IsUnlocked = true
            },
            new BirdSkin
            {
                Id = 3,
                Name = "Purple",
                DisplayName = "Purple Bird",
                DayFlyFrame = "birdfly-3.png",
                DayFallFrame = "birdfall-3.png",
                NightFlyFrame = "birdfly-3.png",
                NightFallFrame = "birdfall-3.png",
                PreviewImage = "birdfly-3.png",
                IsUnlocked = true
            }
        };

        public static int SelectedSkinId
        {
            get => selectedSkinId;
            set => selectedSkinId = value;
        }

        public static BirdSkin GetSelectedSkin()
        {
            return AvailableSkins.Find(s => s.Id == selectedSkinId) ?? AvailableSkins[0];
        }
    }

    public partial class LoginWindow : Window
    {
        private const double DefaultPipeSpeed = 5.0;
        private const double MinPipeSpeed = 3.0;
        private const double MaxPipeSpeed = 10.0;

        private double selectedPipeSpeed = DefaultPipeSpeed;
        private double musicVolume = 50;

        private MediaPlayer mediaPlayer;

        public LoginWindow(double initialPipeSpeed = DefaultPipeSpeed)
        {
            InitializeMediaPlayer();

            PreloadVideo();

            InitializeComponent();

            PreloadVideo();

            selectedPipeSpeed = Math.Clamp(initialPipeSpeed, MinPipeSpeed, MaxPipeSpeed);
        }
        private void PreloadVideo()
        {
            if (mediaBGElement != null)
            {
                try
                {
                    string videoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "LoginWindowBG.mp4");
                    mediaBGElement.Source = new Uri(videoPath, UriKind.Absolute);
                    mediaBGElement.Volume = 0;

                    // Load video nhưng chưa play
                    mediaBGElement.LoadedBehavior = MediaState.Manual;
                    mediaBGElement.Play();
                    mediaBGElement.Pause(); // Pause ngay để giữ frame đầu
                }
                catch (Exception ex)
                {
                    // Log error nếu cần
                }
            }
        }

        private void InitializeMediaPlayer()
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            string assetPath = Path.Combine(AppContext.BaseDirectory, "Assets", "BGM.mp3");
            mediaPlayer.Open(new Uri(assetPath));
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Play immediately - no delay
            if (mediaBGElement != null)
            {
                mediaBGElement.Position = TimeSpan.Zero;
                mediaBGElement.Play();
            }

            if (mediaPlayer != null)
            {
                mediaPlayer.Volume = musicVolume / 100.0;
                mediaPlayer.Position = TimeSpan.Zero;
                mediaPlayer.Play();
            }
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            // Loop nhạc BGM
            if (mediaPlayer != null)
            {
                mediaPlayer.Position = TimeSpan.Zero;
                mediaPlayer.Play();
            }
        }

        private void MediaBGElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Loop video
            mediaBGElement.Position = TimeSpan.Zero;
            mediaBGElement.Play();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Dừng và giải phóng MediaPlayer
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();
            }

            var mainWindow = new MainWindow(selectedPipeSpeed);
            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();

            Close();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(selectedPipeSpeed, musicVolume)
            {
                Owner = this
            };

            if (settingsWindow.ShowDialog() == true)
            {
                selectedPipeSpeed = Math.Clamp(settingsWindow.SelectedPipeSpeed, MinPipeSpeed, MaxPipeSpeed);
                musicVolume = settingsWindow.SelectedVolume;

                // Cập nhật volume
                if (mediaPlayer != null)
                {
                    mediaPlayer.Volume = musicVolume / 100.0;
                }
            }
        }

        private void SkinsButton_Click(object sender, RoutedEventArgs e)
        {
            var skinWindow = new SelectSkin
            {
                Owner = this
            };
            skinWindow.ShowDialog();
        }
        protected override void OnClosed(EventArgs e)
        {
            // Stop video
            if (mediaBGElement != null)
            {
                mediaBGElement.Stop();
                mediaBGElement.Close();
            }

            // Cleanup audio
            if (mediaPlayer != null)
            {
                mediaPlayer.Stop();
                mediaPlayer.Close();
                mediaPlayer = null;
            }

            base.OnClosed(e);
        }
    }
}