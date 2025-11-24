using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using FlappyBird.Business.Models;
using FlappyBird.Business.Services;

namespace PRN212.G5.FlappyBird.Helpers
{
    /// <summary>
    /// Quản lý các frame animation của bird
    /// </summary>
    public class BirdFrameManager
    {
        private BitmapImage[] dayBirdFlyFrames = Array.Empty<BitmapImage>();
        private BitmapImage[] dayBirdFallFrames = Array.Empty<BitmapImage>();
        private BitmapImage[] dayBirdDeathFrames = Array.Empty<BitmapImage>();
        private BitmapImage[] nightBirdFlyFrames = Array.Empty<BitmapImage>();
        private BitmapImage[] nightBirdFallFrames = Array.Empty<BitmapImage>();
        private BitmapImage[] nightBirdDeathFrames = Array.Empty<BitmapImage>();

        private BitmapImage[] currentFlyFrames = Array.Empty<BitmapImage>();
        private BitmapImage[] currentFallFrames = Array.Empty<BitmapImage>();
        private BitmapImage[] currentDeathFrames = Array.Empty<BitmapImage>();

        public BitmapImage[] CurrentFlyFrames => currentFlyFrames;
        public BitmapImage[] CurrentFallFrames => currentFallFrames;
        public BitmapImage[] CurrentDeathFrames => currentDeathFrames;

        /// <summary>
        /// Load tất cả các frame của bird dựa trên skin đã chọn
        /// </summary>
        public void LoadAllBirdFrames()
        {
            // Get selected skin from SkinManager
            var selectedSkin = SkinManager.GetSelectedSkin();

            // Day theme - Load frames from selected skin
            var birdFlyDay = AssetHelper.LoadBitmapSafe(selectedSkin.DayFlyFrame);
            var birdFallDay = AssetHelper.LoadBitmapSafe(selectedSkin.DayFallFrame);

            // Only use frames that actually exist
            if (birdFlyDay != null)
            {
                dayBirdFlyFrames = new[] { birdFlyDay };
            }
            else
            {
                // Fallback to a default if the day fly frame is missing
                dayBirdFlyFrames = new BitmapImage[0];
            }

            if (birdFallDay != null)
            {
                dayBirdFallFrames = new[] { birdFallDay };
                dayBirdDeathFrames = new[] { birdFallDay };
            }
            else
            {
                dayBirdFallFrames = new BitmapImage[0];
                dayBirdDeathFrames = new BitmapImage[0];
            }

            // Night theme - Load frames from selected skin
            var birdFlyNight = AssetHelper.LoadBitmapSafe(selectedSkin.NightFlyFrame);
            var birdFallNight = AssetHelper.LoadBitmapSafe(selectedSkin.NightFallFrame);

            if (birdFlyNight != null)
            {
                nightBirdFlyFrames = new[] { birdFlyNight };
            }
            else
            {
                // Fallback to day frames if night frames are missing
                nightBirdFlyFrames = dayBirdFlyFrames;
            }

            if (birdFallNight != null)
            {
                nightBirdFallFrames = new[] { birdFallNight };
                nightBirdDeathFrames = new[] { birdFallNight };
            }
            else
            {
                nightBirdFallFrames = dayBirdFallFrames;
                nightBirdDeathFrames = dayBirdDeathFrames;
            }
        }

        /// <summary>
        /// Chuyển đổi frame theo theme (day/night)
        /// </summary>
        public void UseBirdFramesForTheme(bool night, BirdService birdService, Image birdImage)
        {
            currentFlyFrames = night ? nightBirdFlyFrames : dayBirdFlyFrames;
            currentFallFrames = night ? nightBirdFallFrames : dayBirdFallFrames;
            currentDeathFrames = night ? nightBirdDeathFrames : dayBirdDeathFrames;

            birdService.BirdState.FrameIndex = 0;
            birdService.BirdState.AnimationState = BirdAnimationState.Flying;

            if (currentFlyFrames.Length > 0 && currentFlyFrames[0] != null)
                birdImage.Source = currentFlyFrames[0];
        }
    }
}

