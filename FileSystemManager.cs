using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{
    internal class FileSystemManager
    {
        public static String getInstallationPath()
        {
            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + Registry.appName + "\\Installation"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + Registry.appName + "\\Installation");
            }
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + Registry.appName + "\\Installation";
        }

        public static void RelaunchUpdaterFromLocation()
        {
            if (!Debugger.IsAttached)
            {
                if (!getOperatingExePath().Equals(getUpdaterExePath()))
                {
                    copyUpdaterToLocation();
                    string fileName = getUpdaterExePath();
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.FileName = fileName;
                    processInfo.Arguments = "";
                    int count = 0;
                    foreach (string arg in Environment.GetCommandLineArgs())
                    {
                        if (count != 0)
                        {
                            processInfo.Arguments += arg + " ";
                        }
                        count++;
                    }
                    processInfo.Arguments += "--startnumber two";

                    try
                    {
                        Process.Start(processInfo);
                    }
                    catch (Win32Exception e)
                    {
                        // This will be thrown if the user cancels the prompt
                    }
                    Environment.Exit(Environment.ExitCode);
                }
            }
        }

        internal static void DeleteApp(bool deleteself = true)
        {
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
            try
            {
                File.Delete(getInstallationPath() + "\\icon.ico");
            }
            catch { }
            try
            {

                foreach (var f in Directory.GetDirectories(getInstallationPath()))
                {
                    try
                    {

                            Directory.Delete(f, true);
                    }
                    catch
                    {

                    }
                }
                
            }
            catch { }
            try
            {
                foreach (var f in Directory.GetFiles(getInstallationPath()))
                {
                    File.Delete(f);
                }
            }
            catch
            {

            }
            try
            {
                string shortcutLocation = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), Registry.appName + ".lnk");
                File.Delete(shortcutLocation);
            }
            catch { }
            if (deleteself)
            {
                Process.Start(new ProcessStartInfo()
                {
                    Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Process.GetCurrentProcess().MainModule.FileName + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                });
            }

        }

        public static String getSoftwareExePath()
        {
            return getInstallationPath() + "\\" + Registry.getInstalledVersion() + "\\" + Registry.appName + ".exe";
        }
        public static String getUpdaterExePath()
        {
            return getInstallationPath() + "\\Updater\\" + Registry.appName.Replace(" ", "") + ".exe";
        }
        public static String getOperatingExePath()
        {
            return Process.GetCurrentProcess().MainModule.FileName;
        }
        public static void copyUpdaterToLocation()
        {
            if (!getOperatingExePath().Equals(getUpdaterExePath()))
            {
                createFile(getUpdaterExePath());
                
                File.Copy(getOperatingExePath(), getUpdaterExePath(), true);
            }
        }

        public static long getInstallationSize()
        {
            return DirSize(new DirectoryInfo(FileSystemManager.getInstallationPath())) / 1024;
        }

        private static void createFile(string v)
        {
            if (!Directory.Exists(v.Substring(0, v.LastIndexOf("\\"))))
            {
                Directory.CreateDirectory(v.Substring(0, v.LastIndexOf("\\")));
            }
            File.Create(v).Close();

        }
        private static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            try
            {
                FileInfo[] fis = d.GetFiles();
                foreach (FileInfo fi in fis)
                {
                    size += fi.Length;
                }
                // Add subdirectory sizes.
                DirectoryInfo[] dis = d.GetDirectories();
                foreach (DirectoryInfo di in dis)
                {
                    size += DirSize(di);
                }

            }
            catch (UnauthorizedAccessException)
            {

            }
            return size;
        }
    }
}
