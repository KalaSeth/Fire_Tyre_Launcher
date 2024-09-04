using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Fire_Tyre_Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {

            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Loaded += (sender, args) =>
                {
                    var dpiScale = VisualTreeHelper.GetDpi(mainWindow);
                    mainWindow.LayoutTransform = new ScaleTransform(1 / dpiScale.DpiScaleX, 1 / dpiScale.DpiScaleY);
                };
            }

            const string appName = "Fire Tyre";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // Application is already running, bring the existing instance to the foreground
                MessageBox.Show("The application is already running.", "Launcher Instance", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
