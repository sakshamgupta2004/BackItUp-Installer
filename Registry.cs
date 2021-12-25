using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    internal class Registry
    {
        public static String appName = "BackItUp";
        private static String baselink = "http://backitup.sugarsnooper.com/mainexe/";
        public static String appLink = baselink + "software.exe";
        public static String appVersionLink = baselink + "version.txt";
        public static String selfLink = baselink + "setup.exe";
        public static String selfVersionLink = baselink + "updaterversion.txt";
        public static String publisher = "Sugarsnooper";


        public static bool isInstalled()
        {

            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            if (key != null)
            {
                if (!key.GetValue("DisplayName", "").ToString().Equals(appName))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
        public static String getInstalledVersion()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            if (key != null)
            {
                try
                {
                    return key.GetValue("DisplayVersion").ToString();
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public static String getUpdaterVersion()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            if (key != null)
            {
                try
                {
                    return key.GetValue("UpdaterVersion").ToString();
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static void DeleteApp()
        {
            try
            {
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
                key.DeleteSubKeyTree(appName + "-PC");
            }
            catch
            {

            }
        }

        public static void setInstallationSize()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            key.SetValue("EstimatedSize", FileSystemManager.getInstallationSize(), RegistryValueKind.DWord);
        }
        public static void setInstallationIcon()
        {
            var fs = File.Create(FileSystemManager.getInstallationPath() + "\\icon.ico");
            Backitup_Installer.Properties.Resources.icon.Save(fs);
            fs.Close();
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            key.SetValue("DisplayIcon", FileSystemManager.getInstallationPath() + "\\icon.ico");
        }
        public static void setInstallationName()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            key.SetValue("DisplayName", appName);
        }

        public static void setJustUpdated()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            key.SetValue("justupdated", 1);
        }
        public static void clearJustUpdated()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            key.SetValue("justupdated", 0);
        }
        public static bool isJustUpdated()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            if (int.Parse(key.GetValue("justupdated", 0).ToString()) == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void setUninstallString()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            key.SetValue("UninstallString", FileSystemManager.getUpdaterExePath() + " --uninstall true");

        }
        public static void setPublisher()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            key.SetValue("Publisher", publisher);
        }
        public static void setInstallDate()
        {
            RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
            if (key.GetValue("InstallDate", null) == null)
            {
                key.SetValue("InstallDate", DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString(), RegistryValueKind.String);
            }
        }
        public static void setVersion(string version)
        {
            try
            {
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
                key.SetValue("DisplayVersion", version);
            }
            catch { }
        }
        public static void setUpdaterVersion(string version)
        {
            try
            {
                RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + appName + "-PC");
                key.SetValue("UpdaterVersion", version);
            }
            catch { }
        }

    }
}
