using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FlappyBird.Business.Services;

namespace PRN212.G5.FlappyBird.Helpers
{
    /// <summary>
    /// Helper cho các animation trong game
    /// </summary>
    public class AnimationHelper
    {
        private readonly StageService stageService;
        private readonly Canvas gameCanvas;
        private readonly UIElement dayLayer;
        private readonly UIElement nightLayer;
        private readonly UIElement dayGround;
        private readonly UIElement nightGround;
        private readonly Image birdImage;
        private bool isTransitioning = false;

        public bool IsTransitioning => isTransitioning;

        public AnimationHelper(
            StageService stageService,
            Canvas gameCanvas,
            UIElement dayLayer,
            UIElement nightLayer,
            UIElement dayGround,
            UIElement nightGround,
            Image birdImage)
        {
            this.stageService = stageService;
            this.gameCanvas = gameCanvas;
            this.dayLayer = dayLayer;
            this.nightLayer = nightLayer;
            this.dayGround = dayGround;
            this.nightGround = nightGround;
            this.birdImage = birdImage;
        }

        /// <summary>
        /// Chuyển đổi day/night mượt mà
        /// </summary>
        public void SmoothToggleDayNight(Action<bool> updateAssets, Action<bool> updateBirdFrames)
        {
            if (isTransitioning) return;
            isTransitioning = true;

            // Gọi StageService để toggle day/night (business logic)
            bool targetNight = stageService.ToggleDayNight();
            // Tăng duration lên 2.5 giây để chuyển đổi mượt mà hơn
            var dur = TimeSpan.FromSeconds(2.5);

            // Dùng PowerEase với Power = 2 để có hiệu ứng mượt mà, tự nhiên hơn
            var dayAnim = new DoubleAnimation
            {
                To = targetNight ? 0 : 1,
                Duration = dur,
                EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut }
            };

            var nightAnim = new DoubleAnimation
            {
                To = targetNight ? 1 : 0,
                Duration = dur,
                EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut }
            };

            // Animate ground opacity
            var dayGroundAnim = new DoubleAnimation
            {
                To = targetNight ? 0 : 1,
                Duration = dur,
                EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut }
            };

            var nightGroundAnim = new DoubleAnimation
            {
                To = targetNight ? 1 : 0,
                Duration = dur,
                EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut }
            };

            // Update assets ở giữa quá trình chuyển đổi (50%) và đảm bảo pipes mới cũng đúng màu
            // Dùng DispatcherTimer với interval lớn hơn để tối ưu
            bool assetsUpdated = false;
            var updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(dur.TotalSeconds * 0.5) // Update ở 50%
            };
            
            // Update assets ngay lập tức để pipes mới được tạo sẽ dùng màu đúng
            // (StageService đã được update trong ToggleDayNight())
            
            updateTimer.Tick += (_, __) =>
            {
                if (!assetsUpdated)
                {
                    updateAssets(targetNight);
                    updateBirdFrames(targetNight);
                    assetsUpdated = true;
                    updateTimer.Stop();
                }
            };
            updateTimer.Start();
            
            // Animate ground
            dayGround.BeginAnimation(UIElement.OpacityProperty, dayGroundAnim);
            nightGround.BeginAnimation(UIElement.OpacityProperty, nightGroundAnim);
            
            dayAnim.Completed += (_, __) =>
            {
                updateTimer.Stop();
                isTransitioning = false;
                // Update lại tất cả assets một lần nữa
                updateAssets(targetNight);
                updateBirdFrames(targetNight);
            };

            dayLayer.BeginAnimation(UIElement.OpacityProperty, dayAnim);
            nightLayer.BeginAnimation(UIElement.OpacityProperty, nightAnim);
        }

        /// <summary>
        /// Reset về day mode
        /// </summary>
        public void ResetStageToDay(bool animate, Action updateAssets, Action updateBirdFrames)
        {
            isTransitioning = false;

            // Gọi StageService để reset về day mode (business logic)
            stageService.ResetToDay();

            dayLayer.BeginAnimation(UIElement.OpacityProperty, null);
            nightLayer.BeginAnimation(UIElement.OpacityProperty, null);
            dayGround.BeginAnimation(UIElement.OpacityProperty, null);
            nightGround.BeginAnimation(UIElement.OpacityProperty, null);

            if (animate)
            {
                var dayAnim = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromSeconds(1.0),
                    EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut }
                };
                var nightAnim = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(1.0),
                    EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut }
                };
                var dayGroundAnim = new DoubleAnimation
                {
                    To = 1,
                    Duration = TimeSpan.FromSeconds(1.0),
                    EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut }
                };
                var nightGroundAnim = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromSeconds(1.0),
                    EasingFunction = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut }
                };

                dayAnim.Completed += (_, __) =>
                {
                    // StageService đã được reset trong ResetStageToDay()
                    updateAssets();
                    updateBirdFrames();
                };

                dayLayer.BeginAnimation(UIElement.OpacityProperty, dayAnim);
                nightLayer.BeginAnimation(UIElement.OpacityProperty, nightAnim);
                dayGround.BeginAnimation(UIElement.OpacityProperty, dayGroundAnim);
                nightGround.BeginAnimation(UIElement.OpacityProperty, nightGroundAnim);
            }
            else
            {
                dayLayer.Opacity = 1;
                nightLayer.Opacity = 0;
                dayGround.Opacity = 1;
                nightGround.Opacity = 0;
                // StageService đã được reset trong ResetStageToDay()
                updateAssets();
                updateBirdFrames();
            }
        }

        /// <summary>
        /// Animation khi bird chết
        /// </summary>
        public void AnimateBirdDeath(BirdService birdService, double canvasHeight)
        {
            // Create a falling animation for the dead bird
            var fallDuration = TimeSpan.FromSeconds(0.8);
            var currentTop = Canvas.GetTop(birdImage);
            var groundLevel = canvasHeight - birdImage.Height;

            var fallAnim = new DoubleAnimation
            {
                From = currentTop,
                To = groundLevel,
                Duration = fallDuration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            // Rotate bird to falling position
            var rotateAnim = new DoubleAnimation
            {
                From = birdService.BirdState.Rotation,
                To = 90, // MaxDownRotation
                Duration = fallDuration,
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            rotateAnim.Completed += (_, __) =>
            {
                birdService.BirdState.Rotation = 90;
            };

            var rt = birdImage.RenderTransform as RotateTransform;
            if (rt != null)
            {
                rt.BeginAnimation(RotateTransform.AngleProperty, rotateAnim);
            }

            birdImage.BeginAnimation(Canvas.TopProperty, fallAnim);
        }
    }
}

