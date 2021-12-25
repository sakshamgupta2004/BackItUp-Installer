using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace WpfApp1
{
    internal class AdminHandler
    {
        private static void AdminRelauncher()
        {
            if (Debugger.IsAttached)
                return;
            if (!IsRunAsAdmin())
            {
                // relaunch the application with admin rights
                string fileName = Process.GetCurrentProcess().MainModule.FileName;
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.CreateNoWindow = false;
                //processInfo.WindowStyle = ProcessWindowStyle.Maximized;
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runAs";
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
                return;
            }
        }

        private static bool IsRunAsAdmin()
        {
            try
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(id);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (Exception)
            {
                return false;
            }

        }
        internal static void StartAsAdmin()
        {
            string[] args = Environment.GetCommandLineArgs();
            string allargs = "";
            foreach (string arg in args)
            {
                allargs += args;
            }
            if (!allargs.ToLower().Contains("startnumber"))
            {
                AdminRelauncher();
            }
            else
            {
                if (!IsRunAsAdmin())
                {
                    Environment.Exit(Environment.ExitCode);
                }
            }
        }
    }
}