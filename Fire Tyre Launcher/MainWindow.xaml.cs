using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Fire_Tyre_Launcher
{
    enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate,
        install,
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string roothPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;
        private string gamelocation;
        private string newsloc;

        private List<Uri> imageUris;
        private int currentIndex = 0;
        private DispatcherTimer timer;

        private LauncherStatus _status;
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;

                switch (_status)
                {
                    case LauncherStatus.ready:
                        PlayButton.Content = "Play";
                        break;
                    case LauncherStatus.failed:
                        PlayButton.Content = "Failed - - Retry";
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Content = "Downloading Game";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Content = "Downloading Updates";
                        break;
                    case LauncherStatus.install:
                        PlayButton.Content = "Download";
                        break;

                    default:
                        break;
                }
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Window_MouseLeftButtonDown);
            roothPath = Directory.GetCurrentDirectory();
            gamelocation = Path.Combine(roothPath, "Data");
            versionFile = Path.Combine(gamelocation, "version.txt");
            gameZip = Path.Combine(gamelocation, "Fire Tyre.zip");
            gameExe = Path.Combine(gamelocation, "Fire Tyre", "Fire Tyre.exe");
            newsloc = Path.Combine(gamelocation, "news.txt");

            if (!Directory.Exists(gamelocation))
            {
                Directory.CreateDirectory(gamelocation);
            }

            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1JxKi2d8jaStf6ZkdDd0gr6QAjDt2JdEE?alt=media&key=AIzaSyDVwCLXRkNFj3BuPCOuGyDO8aGg7-0Y5UI"), newsloc);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(LoadBulletPoints);
            }
            catch
            {

            }



            DownloadImage.Visibility = Visibility.Collapsed;
            DownloadProgressBar.Visibility = Visibility.Collapsed;
            DownloadProgressText.Visibility = Visibility.Collapsed;

            CloseSettings();
        }

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localversion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localversion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://www.googleapis.com/drive/v3/files/1eqQVwYbMI_-nI0AOL-DUxq_a8OVwKeYX?alt=media&key=AIzaSyDVwCLXRkNFj3BuPCOuGyDO8aGg7-0Y5UI"));

                    if (onlineVersion.isDifferentThan(localversion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception e)
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error checking for updates: {e} ");

                }
            }
            else
            {
                Status = LauncherStatus.install;
            }

        }

        private void InstallGameFiles(bool isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();

                if (isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://www.googleapis.com/drive/v3/files/1eqQVwYbMI_-nI0AOL-DUxq_a8OVwKeYX?alt=media&key=AIzaSyDVwCLXRkNFj3BuPCOuGyDO8aGg7-0Y5UI"));
                }

                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://www.googleapis.com/drive/v3/files/1DT880Hd34sX-bniBOUBM6FxxkbSuFcjj?alt=media&key=AIzaSyDVwCLXRkNFj3BuPCOuGyDO8aGg7-0Y5UI"), gameZip, _onlineVersion);
            }
            catch (Exception e)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error installing game files: {e} ");

            }
        }

        // Event handler to update download progress
        private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            DownloadImage.Visibility = Visibility.Visible;
            DownloadProgressBar.Visibility = Visibility.Visible;
            DownloadProgressText.Visibility = Visibility.Visible;

            // Update UI with download progress
            DownloadProgressBar.Value = percentage;  // Assuming you have a ProgressBar control
            DownloadProgressText.Text = $"{e.BytesReceived / (1024.0 * 1024.0):0} MB of {e.TotalBytesToReceive / (1024.0 * 1024.0):0} MB :   {percentage:0}%";
        }

        private void DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                if (!Directory.Exists(Path.Combine(gamelocation, "temp")))
                {
                    Directory.CreateDirectory(Path.Combine(gamelocation, "temp"));
                }

                ZipFile.ExtractToDirectory(gameZip, gamelocation, overwriteFiles: true);
                // Directory.Move(Path.Combine(gamelocation, "temp", "Fire Tyre"), gamelocation);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);
                VersionText.Text = onlineVersion;

                DownloadImage.Visibility = Visibility.Collapsed;
                DownloadProgressBar.Visibility = Visibility.Collapsed;
                DownloadProgressText.Visibility = Visibility.Collapsed;

                Status = LauncherStatus.ready;
            }
            catch
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error finishing downloads: {e} ");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(gamelocation, "Fire Tyre");
                Process.Start(startInfo);

                Close();
            }
            else if (Status == LauncherStatus.install)
            {
                if (!Directory.Exists(gamelocation))
                {
                    Directory.CreateDirectory(gamelocation);
                }
                InstallGameFiles(false, Version.Zero);
            }
            else if (Status == LauncherStatus.failed)
            {
                CheckForUpdates();
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            Close();
        }

        private void LinkdinButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            System.Diagnostics.Process.Start("https://in.linkedin.com/company/zherblast");
        }

        private void ZherWebButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            System.Diagnostics.Process.Start("https://zherblast.com/");
        }

        private void InstaButton_Click(object sender, RoutedEventArgs eventArgs)
        {
            System.Diagnostics.Process.Start("https://www.instagram.com/zherblast/");
        }

        private void Discord_Click(object sender, RoutedEventArgs eventArgs)
        {
            System.Diagnostics.Process.Start("https://discord.gg/AAzq5q4aBh");
        }

        private void YoutubeButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.youtube.com/@ZherBlast");
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettings();
        }

        private void UninstallGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.Delete(gamelocation, true);
                Status = LauncherStatus.install;
                MessageBox.Show("Uninstalled Finished.");
                CloseSettings();
                CheckForUpdates();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Uninstalling Game: " + ex.Message);
            }
        }

        private void RepairGameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(versionFile))
                {
                    try
                    {
                        File.WriteAllText(versionFile, "6.6.6");
                    }
                    catch
                    {
                        MessageBox.Show("Error with reading files.");
                    }
                }
                CloseSettings();
                CheckForUpdates();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Repairing Game: " + ex.Message);
            }
        }

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            CloseSettings();
        }

        void ShowSettings()
        {
            Setup2.Visibility = Visibility.Visible;
            UninstallGameButton.Visibility = Visibility.Visible;
            RepairGameButton.Visibility = Visibility.Visible;
            SetText.Visibility = Visibility.Visible;
            CloseSettingsButton.Visibility = Visibility.Visible;
        }

        void CloseSettings()
        {
            Setup2.Visibility = Visibility.Collapsed;
            UninstallGameButton.Visibility = Visibility.Collapsed;
            RepairGameButton.Visibility = Visibility.Collapsed;
            SetText.Visibility = Visibility.Collapsed;
            CloseSettingsButton.Visibility = Visibility.Collapsed;
        }

        private void LoadBulletPoints(object sender, AsyncCompletedEventArgs e)
        {
            if (File.Exists(newsloc))
            {
                try
                {
                    string fileContent = File.ReadAllText(newsloc);

                    string[] lines = fileContent.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    string formattedContent = string.Join(Environment.NewLine, lines);

                    BulletTextBlock.Text = formattedContent;
                }
                catch (Exception)
                {
                    //MessageBox.Show($"Error loading file: {ex.Message}");
                }
            }
        }

    }
  
  

    struct Version
    {
        internal static Version Zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subminor;

        internal Version(short _major, short _minor, short _subminor)
        {
            major = _major;
            minor = _minor;
            subminor = _subminor;
        }

        internal Version(string _version)
        {
            string[] _versionstring = _version.Split('.');
            if (_versionstring.Length != 3)
            {
                major = 0;
                minor = 0;
                subminor = 0;
                return;
            }
            major = short.Parse(_versionstring[0]);
            minor = short.Parse(_versionstring[1]);
            subminor = short.Parse(_versionstring[2]);
        }

        internal bool isDifferentThan(Version _otherVersion)
        {
            if (major != _otherVersion.major)
            {
                return true;
            }
            else
            {
                if (minor != _otherVersion.minor)
                {
                    return true;
                }
                else
                {
                    if (subminor != _otherVersion.subminor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subminor}";
        }
    }
}
