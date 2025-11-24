using System.Windows;

namespace PRN212.G5.FlappyBird
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnMainWindowClose;

            var startWindow = new LoginWindow();

            MainWindow = startWindow;
            startWindow.Show();
        }
    }
}
