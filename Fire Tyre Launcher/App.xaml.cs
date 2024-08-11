using System.Threading;
using System.Windows;

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
