using System;
using System.Windows.Media.Imaging;

namespace PRN212.G5.FlappyBird.Helpers
{
    public static class AssetHelper
    {
        /// <summary>
        /// Tạo pack URI cho asset file
        /// </summary>
        public static string Pack(string file) => $"pack://application:,,,/Assets/{file}";

        /// <summary>
        /// Tạo đường dẫn asset từ thư mục Assets
        /// </summary>
        public static string AssetPath(string file) => System.IO.Path.Combine(AppContext.BaseDirectory, "Assets", file);

        /// <summary>
        /// Load bitmap an toàn, trả về null nếu lỗi
        /// </summary>
        public static BitmapImage LoadBitmapSafe(string file)
        {
            try
            {
                return new BitmapImage(new Uri(Pack(file)));
            }
            catch
            {
                return null!;
            }
        }

    }
}

