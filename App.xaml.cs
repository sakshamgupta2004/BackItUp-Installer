using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp1
{

    public partial class App : Application
    {


        bool checkForUpdateOnStartup = true;
        bool checkForUpdateAfterStartup = true;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            
            var args = parseArgmuents();
           /* 
            if (args.ContainsKey("justselfupdated"))
            {
                if (File.Exists(FileSystemManager.getUpdaterExePath()))
                {
                    File.Delete(FileSystemManager.getUpdaterExePath());
                }
                File.Copy(Process.GetCurrentProcess().MainModule.FileName, FileSystemManager.getUpdaterExePath());
                Process.Start(FileSystemManager.getUpdaterExePath(), "--donotautostart true");
                Process.Start(new ProcessStartInfo()
                {
                    Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" + Process.GetCurrentProcess().MainModule.FileName + "\"",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    FileName = "cmd.exe"
                });
                Environment.Exit(0);
            }*/
            if ((args.ContainsKey("startnumber") && args.Count == 1) || (args.Count == 0))
            {
                AdminHandler.StartAsAdmin();
                FileSystemManager.RelaunchUpdaterFromLocation();

                bool startWindow = true;
                if (Registry.isJustUpdated())
                {
                    startWindow = false;
                    Registry.clearJustUpdated();
                }
                if (Registry.isInstalled())
                {
                    
                    if (checkForUpdateAfterStartup)
                    {
                        if (startWindow)
                            Process.Start(FileSystemManager.getSoftwareExePath());
                        new MainWindow(false);
                    }
                    else
                    {
                        new MainWindow(false).Show();
                    }
                }
                else
                {
                    new MainWindow(true).Show();
                }
            }
            else if (args.ContainsKey("uninstall"))
            {
                AdminHandler.StartAsAdmin();
                FileSystemManager.DeleteApp();
                Registry.DeleteApp();
                Environment.Exit(0);
            }
            else if (args.ContainsKey("update"))
            {
                AdminHandler.StartAsAdmin();
                FileSystemManager.RelaunchUpdaterFromLocation();
                new MainWindow(true, true).Show();
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private Dictionary<string, string> parseArgmuents()
        {
            string[] args = Environment.GetCommandLineArgs();
            var arguments = new Dictionary<string, string>();

            for (int i = 1; i < args.Length; i += 2)
            {

                try
                {
                    string arg = args[i].Substring(args[i].IndexOf("--") + 2);
                    arguments.Add(arg, args[i + 1]);
                }
                catch
                {

                }
            }
            return arguments;
        }
    }
}
