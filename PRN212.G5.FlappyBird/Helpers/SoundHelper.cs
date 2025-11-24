using System;
using System.Windows.Media;

namespace PRN212.G5.FlappyBird.Helpers
{
    public static class SoundHelper
    {
        /// <summary>
        /// Phát sound effect
        /// </summary>
        public static void PlaySfx(MediaPlayer player, string file, double volume = 0.6)
        {
            player.Stop();
            player.Open(new Uri(AssetHelper.AssetPath(file)));
            player.Volume = volume;
            player.Position = TimeSpan.Zero;
            player.Play();
        }
    }
}

