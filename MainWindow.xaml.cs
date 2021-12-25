using FileWire;
using IWshRuntimeLibrary;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using File = System.IO.File;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        MainWindow window;
        String newVersion = "";
        String newSelfVersion = "";
        public MainWindow(bool startInstall, bool updating = false)
        {
            InitializeComponent();
            this.Closing += (_, __) =>
            {
                __.Cancel = true;
            };
            try
            {
                using (RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        Object o = key.GetValue("AppsUseLightTheme");
                        if (o != null)
                        {
                            if (o.Equals(0))
                            {
                                setDarkColors();
                            }
                            else
                            {
                                setLightColors();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            window = this;
            if (startInstall)
            {

                isUpdateAvailable();
                isSelfUpdateAvailable();
                argsBox.Text = "";
                foreach (string arg in Environment.GetCommandLineArgs())
                {
                    argsBox.Text += arg;
                    argsBox.Text += "\n";
                }
                downloadAndInstall(updating);
            }
            else
            {
                checkForUpdates();
            }
        }

        private void checkForUpdates()
        {


            DownloadProgress.Foreground = SourceChord.FluentWPF.AccentColors.ImmersiveSystemAccentBrush;
            DownloadProgress.IsIndeterminate = true;
            status.Text = "Checking for Updates";
            new Thread(() =>
            {
                Thread.Sleep(500);

                if (isSelfUpdateAvailable())
                {

                    DownloadProgress.Dispatcher.Invoke(() =>
                    {
                        status.Text = "Updating...";
                        DownloadProgress.IsIndeterminate = false;
                    });
                    Directory.CreateDirectory(FileSystemManager.getInstallationPath() + "\\UpdaterTemp");
                    var downloader = new Downloader(new Uri(Registry.selfLink), FileSystemManager.getInstallationPath() + "\\UpdaterTemp\\" + Registry.appName + ".exe");
                    downloader.setOnDownloadCompleteEventHandler(new Downloader.DownloadCompleteEventHandler(((Action<Object>)((_) => {
                        Registry.setJustUpdated();
                        Process.Start(FileSystemManager.getInstallationPath() + "\\UpdaterTemp\\" + Registry.appName + ".exe");
                        Registry.setUpdaterVersion(newSelfVersion);
                        Environment.Exit(0);
                    }))));
                    downloader.setOnDownloadProgressEventHandler(new Downloader.DownloadProgressChangedEventHandler((Action<object, Downloader.DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs>)((s, e) => {
                        window.Dispatcher.Invoke(() =>
                        {
                            DownloadProgress.Dispatcher.Invoke(() =>
                            {
                                DownloadProgress.Foreground = SourceChord.FluentWPF.AccentColors.ImmersiveSystemAccentBrush;
                                DownloadProgress.Value = e.BytesReceived;
                            });
                        });
                    })));
                    downloader.setOnDownloadFailedEventHandler(new Downloader.DownloadFailedEventHandler((Action<object>)((e) => {
                        DownloadProgress.Dispatcher.Invoke(() =>
                        {
                            DownloadProgress.Foreground = Brushes.Red;
                            DownloadProgress.Value = 100;
                            int i = 3;
                            for (; i >= 0; i--)
                            {
                                status.Text = "Failed... Exiting in " + i.ToString();
                                Thread.Sleep(1000);
                                if (i == 0)
                                {
                                    Environment.Exit(0);
                                }
                            }
                        });
                    })));
                }
                else
                {
                    checkForApplicationUpdates();
                }
                
            }).Start();

        }
        private void checkForApplicationUpdates()
        {
            if (isUpdateAvailable())
            {
                window.Dispatcher.Invoke(() =>
                {
                    DownloadProgress.IsIndeterminate = false;
                    DownloadProgress.Value = 0;
                    if (window.IsVisible)
                    {

                        downloadAndInstall(true);
                    }
                    else
                    {
                        window.Dispatcher.Invoke(() =>
                        {
                            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Do you want to update? The application will be updated and restarted if you select yes.", "New Update Available", MessageBoxButtons.YesNo);
                            if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                            {
                                window.Show();
                                try
                                {
                                    foreach (var p in Process.GetProcessesByName(Registry.appName))
                                    {
                                        if (!p.MainModule.FileName.Equals(Process.GetCurrentProcess().MainModule.FileName))
                                        {
                                            p.Kill();
                                            p.WaitForExit();
                                        }
                                    }


                                }
                                catch { }
                                downloadAndInstall(true);
                            }
                            else if (dialogResult == System.Windows.Forms.DialogResult.No)
                            {
                                Environment.Exit(0);
                            }
                        });

                    }
                });

            }
            else
            {
                window.Dispatcher.Invoke(() =>
                {
                    if (window.IsVisible)
                    {
                        Process.Start(FileSystemManager.getSoftwareExePath());

                    }
                    Environment.Exit(0);
                });

            }
        }

        private bool isUpdateAvailable()
        {
            try
            {
                WebClient web = new WebClient();
                string version = web.DownloadString(Registry.appVersionLink);
                newVersion = version;
                if (version == null)
                    return false;
                else
                    return !Registry.getInstalledVersion().Equals(version);
            }
            catch
            {
                return false;
            }
        }
        private bool isSelfUpdateAvailable()
        {
            try
            {
                WebClient web = new WebClient();
                string version = web.DownloadString(Registry.selfVersionLink);
                newSelfVersion = version;
                if (version == null)
                    return false;
                else
                {
                    if (Registry.getUpdaterVersion() == null)
                    {
                        Registry.setUpdaterVersion(version);
                    }
                    return !Registry.getUpdaterVersion().Equals(version);
                }
            }
            catch
            {
                return false;
            }
        }

        private void setLightColors()
        {
            backgroundImage.Source = new BitmapImage(new Uri("Resources/background.jpeg", UriKind.Relative));
            logo.Source = new BitmapImage(new Uri("icon.ico", UriKind.Relative));
        }

        private void setDarkColors()
        {
            backgroundImage.Source = new BitmapImage(new Uri("Resources/background_dark.jpg", UriKind.Relative));
            logo.Source = new BitmapImage(new Uri("icon.ico", UriKind.Relative));
        }
        private void downloadAndInstall(bool updating)
        {
            if (updating)
                status.Text = "Downloading Update...";
            else 
                status.Text = "Downloading...";

            version.Text += "v";
            version.Text += newVersion;
            Directory.CreateDirectory(FileSystemManager.getInstallationPath().Substring(0, FileSystemManager.getInstallationPath().LastIndexOf("\\")) + "\\temp");
            using (var dl = new Downloader(new Uri(Registry.appLink), FileSystemManager.getInstallationPath().Substring(0, FileSystemManager.getInstallationPath().LastIndexOf("\\")) + "\\temp\\" + Registry.appName + ".exe"))
            {
                dl.setOnDownloadProgressEventHandler(new Downloader.DownloadProgressChangedEventHandler((Action<object, Downloader.DownloadProgressChangedEventHandler.DownloadProgressChangedEventArgs>)((s, e) => {
                window.Dispatcher.Invoke(() =>
                {
                    DownloadProgress.Dispatcher.Invoke(() =>
                    {
                        DownloadProgress.Foreground = SourceChord.FluentWPF.AccentColors.ImmersiveSystemAccentBrush;
                        DownloadProgress.Value = e.BytesReceived;
                    });
                });
                })));
                dl.setOnDownloadCompleteEventHandler(new Downloader.DownloadCompleteEventHandler((Action<object>)((e) => {
                    DownloadProgress.Dispatcher.Invoke(() =>
                    {
                        DownloadProgress.IsIndeterminate = true;
                        if (updating)
                        status.Text = "Updating...";
                        else
                        status.Text = "Installing...";
                    });
                    try
                    {
                        FileSystemManager.DeleteApp(false); ;
                        //Registry.DeleteApp();
                    }
                    catch
                    {

                    }
                    //var versionInfo = FileVersionInfo.GetVersionInfo(FileSystemManager.getInstallationPath().Substring(0, FileSystemManager.getInstallationPath().LastIndexOf("\\")) + "\\temp\\" + Registry.appName + ".exe");
                    string version = newVersion;
                    Directory.CreateDirectory(FileSystemManager.getInstallationPath() + "\\" + version + "\\");
                    if (File.Exists(FileSystemManager.getInstallationPath() + "\\" + version + "\\" + Registry.appName + ".exe"))
                    {
                        File.Delete(FileSystemManager.getInstallationPath() + "\\" + version + "\\" + Registry.appName + ".exe");
                    }
                    File.Move(FileSystemManager.getInstallationPath().Substring(0, FileSystemManager.getInstallationPath().LastIndexOf("\\")) + "\\temp\\" + Registry.appName + ".exe", FileSystemManager.getInstallationPath() + "\\" + version + "\\" + Registry.appName + ".exe");
                    Registry.setVersion(version);
                    Registry.setInstallationIcon();
                    Registry.setInstallationName();
                    Registry.setInstallationSize();
                    Registry.setUninstallString();
                    Registry.setPublisher();
                    Registry.setInstallDate();
                    string shortcutLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Registry.appName + ".lnk");
                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutLocation);

                    shortcut.Description = "Starts Application";   
                    shortcut.IconLocation = FileSystemManager.getInstallationPath() + "\\icon.ico";
                    shortcut.TargetPath = FileSystemManager.getUpdaterExePath(); 
                    shortcut.Save();
                    Process.Start(FileSystemManager.getSoftwareExePath());

                    isSelfUpdateAvailable();
                    Environment.Exit(0);
                    //Install
                })));
                dl.setOnDownloadFailedEventHandler(new Downloader.DownloadFailedEventHandler((Action<object>)((e) => {
                    DownloadProgress.Dispatcher.Invoke(() =>
                    {
                        DownloadProgress.Foreground = Brushes.Red;
                        DownloadProgress.Value = 100;
                        int i = 3;
                        for (; i >= 0; i--)
                        {
                            status.Text = "Failed... Exiting in " + i.ToString();
                            Thread.Sleep(1000);
                            if (i == 0)
                            {
                                Environment.Exit(0);
                            }
                        }
                    });
                })));
            }
        }
    }
}
