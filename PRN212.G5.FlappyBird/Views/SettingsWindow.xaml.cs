using System;
using System.Windows;

namespace PRN212.G5.FlappyBird.Views
{
    public partial class SettingsWindow : Window
    {
        private const double DefaultPipeSpeed = 5.0;
        private const double DefaultVolume = 50.0;

        public double SelectedPipeSpeed { get; private set; }
        public double SelectedVolume { get; private set; }

        public SettingsWindow(double initialPipeSpeed, double initialVolume = DefaultVolume)
        {
            InitializeComponent();

            // Thi?t l?p giá tr? ban ??u cho Pipe Speed
            PipeSpeedSlider.Value = initialPipeSpeed;
            PipeSpeedValueText.Text = initialPipeSpeed.ToString("F1");

            // Thi?t l?p giá tr? ban ??u cho Volume (ch? hi?n th? s?, không có %)
            VolumeSlider.Value = initialVolume;
            VolumeValueText.Text = initialVolume.ToString("F0");

            SelectedPipeSpeed = initialPipeSpeed;
            SelectedVolume = initialVolume;
        }

        private void PipeSpeedSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PipeSpeedValueText != null)
            {
                PipeSpeedValueText.Text = e.NewValue.ToString("F1");
            }
        }

        private void VolumeSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VolumeValueText != null)
            {
                // Ch? hi?n th? s?, không có d?u %
                VolumeValueText.Text = e.NewValue.ToString("F0");
            }
        }

        private void DefaultButton_Click(object sender, RoutedEventArgs e)
        {
            PipeSpeedSlider.Value = DefaultPipeSpeed;
            VolumeSlider.Value = DefaultVolume;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedPipeSpeed = PipeSpeedSlider.Value;
            SelectedVolume = VolumeSlider.Value;
            DialogResult = true;
            Close();
        }
    }
}