using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FlappyBird.Business.Models;
using FlappyBird.Business.Services;

namespace PRN212.G5.FlappyBird.Helpers
{
    /// <summary>
    /// Renderer để tạo và quản lý UI elements cho game
    /// </summary>
    public class GameUIRenderer
    {
        private readonly StageService stageService;
        private readonly Canvas gameCanvas;

        public GameUIRenderer(StageService stageService, Canvas gameCanvas)
        {
            this.stageService = stageService;
            this.gameCanvas = gameCanvas;
        }

        /// <summary>
        /// Tạo UI cho clouds
        /// </summary>
        public List<Image> CreateCloudsUI()
        {
            var clouds = new List<Image>();
            string cloudFile = stageService.IsNight ? "Cloud-Night.png" : "Cloud-Day.png";
            foreach (var cloudState in stageService.Clouds)
            {
                var cloud = new Image
                {
                    Width = cloudState.Width,
                    Height = cloudState.Height,
                    Source = new BitmapImage(new Uri(AssetHelper.Pack(cloudFile))),
                    Stretch = Stretch.Fill,
                    Opacity = 0.9,
                    SnapsToDevicePixels = true
                };
                gameCanvas.Children.Add(cloud);
                Canvas.SetLeft(cloud, cloudState.X);
                Canvas.SetTop(cloud, cloudState.Y);
                clouds.Add(cloud);
            }
            return clouds;
        }

        /// <summary>
        /// Tạo UI cho pipe pair
        /// </summary>
        public (Image top, Image bottom) CreatePipeUI(PipePairState pairState)
        {
            string pipeFile = stageService.IsNight ? "Pipe-night.png" : "Pipe-day.png";

            // Đảm bảo các giá trị được set đúng TRƯỚC khi tạo UI
            if (pairState.MinTopHeight == 0) pairState.MinTopHeight = 100;
            if (pairState.MinBottomHeight == 0) pairState.MinBottomHeight = 100;
            if (pairState.CurrentTopHeight == 0)
            {
                if (pairState.BaseTopHeight > 0)
                    pairState.CurrentTopHeight = pairState.BaseTopHeight;
                else
                    pairState.CurrentTopHeight = 200; // Default height
            }

            var top = new Image
            {
                Width = stageService.GetPipeWidth(),
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri(AssetHelper.Pack(pipeFile))),
                SnapsToDevicePixels = true,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform { ScaleY = -1 },
                Visibility = Visibility.Visible
            };
            var bottom = new Image
            {
                Width = stageService.GetPipeWidth(),
                Stretch = Stretch.Fill,
                Source = new BitmapImage(new Uri(AssetHelper.Pack(pipeFile))),
                SnapsToDevicePixels = true,
                Visibility = Visibility.Visible
            };

            // Set Z-index
            Panel.SetZIndex(top, 3);
            Panel.SetZIndex(bottom, 3);

            // Thêm vào Canvas TRƯỚC
            gameCanvas.Children.Add(top);
            gameCanvas.Children.Add(bottom);
            
            // Set X position
            Canvas.SetLeft(top, pairState.X);
            Canvas.SetLeft(bottom, pairState.X);
            
            // Apply geometry SAU khi đã thêm vào Canvas (sẽ set Top và Height)
            ApplyPipeGeometry(pairState, top, bottom);

            return (top, bottom);
        }

        /// <summary>
        /// Áp dụng geometry cho pipe pair
        /// </summary>
        public void ApplyPipeGeometry(PipePairState pairState, Image top, Image bottom)
        {
            // Đảm bảo các giá trị được set đúng
            if (pairState.MinTopHeight == 0) pairState.MinTopHeight = 100;
            if (pairState.MinBottomHeight == 0) pairState.MinBottomHeight = 100;
            if (pairState.CurrentTopHeight == 0) 
            {
                if (pairState.BaseTopHeight > 0)
                    pairState.CurrentTopHeight = pairState.BaseTopHeight;
                else
                    pairState.CurrentTopHeight = 200; // Default
            }
            
            double maxTopHeight = stageService.GetCanvasHeight() - stageService.GetGap() - pairState.MinBottomHeight;
            double clampedTopHeight = Math.Clamp(pairState.CurrentTopHeight, pairState.MinTopHeight, maxTopHeight);

            // Đảm bảo height > 0
            if (clampedTopHeight <= 0) clampedTopHeight = 100;

            top.Height = clampedTopHeight;
            Canvas.SetTop(top, 0);

            double bottomTop = clampedTopHeight + stageService.GetGap();
            double bottomHeight = stageService.GetCanvasHeight() - bottomTop;
            
            // Đảm bảo bottom height > 0
            if (bottomHeight <= 0) bottomHeight = 100;
            
            bottom.Height = bottomHeight;
            Canvas.SetTop(bottom, bottomTop);

            Panel.SetZIndex(top, 3);
            Panel.SetZIndex(bottom, 3);
        }

        /// <summary>
        /// Tạo UI cho NoTouch obstacle
        /// </summary>
        public Image CreateNoTouchUI(NoTouchState state)
        {
            try
            {
                string imagePath = AssetHelper.Pack("NoTouch.png");
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                
                var obstacle = new Image
                {
                    Width = 60,
                    Height = 60,
                    Stretch = Stretch.Fill,
                    Source = bitmap,
                    SnapsToDevicePixels = true
                };
                
                RenderOptions.SetBitmapScalingMode(obstacle, BitmapScalingMode.HighQuality);
                gameCanvas.Children.Add(obstacle);
                
                Canvas.SetLeft(obstacle, state.X);
                Canvas.SetTop(obstacle, state.CurrentY);
                Panel.SetZIndex(obstacle, 15);
                
                return obstacle;
            }
            catch
            {
                return null!;
            }
        }

        /// <summary>
        /// Tạo UI cho Gate
        /// </summary>
        public Ellipse CreateGateUI(GateState gateState)
        {
            var gate = new Ellipse
            {
                Width = 80,
                Height = 80,
                Fill = new SolidColorBrush(stageService.IsNight ? Colors.Gold : Colors.DarkBlue),
                Opacity = 0.9,
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2
            };
            
            RenderOptions.SetBitmapScalingMode(gate, BitmapScalingMode.LowQuality);
            RenderOptions.SetEdgeMode(gate, EdgeMode.Aliased);
            
            gameCanvas.Children.Add(gate);
            Canvas.SetLeft(gate, gateState.X);
            Canvas.SetTop(gate, gateState.Y);
            Panel.SetZIndex(gate, 20);
            
            return gate;
        }

        /// <summary>
        /// Cập nhật assets động (pipes, clouds) theo theme
        /// </summary>
        public void UpdateDynamicAssets(bool night, List<(Image top, Image bottom)> pipePairs, List<Image> clouds)
        {
            string pipeFile = night ? "Pipe-night.png" : "Pipe-day.png";
            string cloudFile = night ? "Cloud-Night.png" : "Cloud-Day.png";

            // Update tất cả pipes hiện có (bao gồm cả pipes trong group)
            foreach (var pair in pipePairs)
            {
                if (pair.top != null && pair.bottom != null)
                {
                    pair.top.Source = new BitmapImage(new Uri(AssetHelper.Pack(pipeFile)));
                    pair.bottom.Source = new BitmapImage(new Uri(AssetHelper.Pack(pipeFile)));
                }
            }
            foreach (var cloud in clouds)
                cloud.Source = new BitmapImage(new Uri(AssetHelper.Pack(cloudFile)));
        }
    }
}

