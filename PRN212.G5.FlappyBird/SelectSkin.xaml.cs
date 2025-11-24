using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PRN212.G5.FlappyBird
{
    /// <summary>
    /// Interaction logic for SelectSkin.xaml
    /// </summary>
    public partial class SelectSkin : Window
    {
        public SelectSkin()
        {
            InitializeComponent();
            LoadSkins();
        }

        private void LoadSkins()
        {
            foreach (var skin in SkinManager.AvailableSkins)
            {
                var skinCard = CreateSkinCard(skin);
                SkinsPanel.Children.Add(skinCard);
            }
        }

        private Border CreateSkinCard(BirdSkin skin)
        {
            var card = new Border
            {
                Width = 200,
                Height = 250,
                Margin = new Thickness(15),
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B2F2F")),
                BorderThickness = new Thickness(4),
                BorderBrush = skin.Id == SkinManager.SelectedSkinId
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D")),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Preview image
            var imageContainer = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECF0F1")),
                CornerRadius = new CornerRadius(10),
                Margin = new Thickness(15)
            };

            try
            {
                var birdImage = new Image
                {
                    Width = 100,
                    Height = 100,
                    Stretch = Stretch.Uniform,
                    Source = new System.Windows.Media.Imaging.BitmapImage(
                        new Uri($"pack://application:,,,/Assets/{skin.PreviewImage}"))
                };
                imageContainer.Child = birdImage;
            }
            catch
            {
                // Fallback if image not found
                var placeholder = new TextBlock
                {
                    Text = "🐦",
                    FontSize = 60,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                imageContainer.Child = placeholder;
            }

            Grid.SetRow(imageContainer, 0);
            grid.Children.Add(imageContainer);

            // Skin info
            var infoPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 5, 15, 15)
            };

            var nameText = new TextBlock
            {
                Text = skin.DisplayName,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
            infoPanel.Children.Add(nameText);

            if (skin.Id == SkinManager.SelectedSkinId)
            {
                var selectedBadge = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B2F2F")),
                    CornerRadius = new CornerRadius(5),
                    Padding = new Thickness(8, 5, 8, 5),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var badgeText = new TextBlock
                {
                    Text = "SELECTED",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White
                };
                selectedBadge.Child = badgeText;
                infoPanel.Children.Add(selectedBadge);
            }

            Grid.SetRow(infoPanel, 1);
            grid.Children.Add(infoPanel);

            card.Child = grid;

            // Click handler
            card.MouseDown += (s, e) =>
            {
                SkinManager.SelectedSkinId = skin.Id;
                Close();
            };

            // Hover effect
            card.MouseEnter += (s, e) =>
            {
                card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B2F2F"));
                card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B2F2F"));
            };

            card.MouseLeave += (s, e) =>
            {
                card.BorderBrush = skin.Id == SkinManager.SelectedSkinId
                    ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"))
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"));
                card.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B2F2F"));
            };

            return card;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BackButton_MouseEnter(object sender, MouseEventArgs e)
        {
            BackButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6B37C"));
        }

        private void BackButton_MouseLeave(object sender, MouseEventArgs e)
        {
            BackButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D6B37C"));
        }
    }
}
