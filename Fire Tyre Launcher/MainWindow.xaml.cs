using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Fire_Tyre_Launcher
{
    enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate,
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
                        PlayButton.Content = "Downloading Update";
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
            versionFile = Path.Combine(roothPath, "version.txt");
            gameZip = Path.Combine(roothPath, "Fire Tyre.zip");
            gameExe = Path.Combine(roothPath, "Fire Tyre", "Fire Tyre.exe");
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
            }else
            {
                InstallGameFiles(false, Version.Zero);
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

            // Update UI with download progress
            DownloadProgressBar.Value = percentage;  // Assuming you have a ProgressBar control
            DownloadProgressText.Text = $"{e.BytesReceived / (1024.0 * 1024.0):0} MB of {e.TotalBytesToReceive / (1024.0 * 1024.0):0} MB :   {percentage:0}%";
        }

        private void DownloadCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, roothPath);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);
                VersionText.Text = onlineVersion;

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
                startInfo.WorkingDirectory = Path.Combine(roothPath, "Fire Tyre");
                Process.Start(startInfo);

                Close();
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
