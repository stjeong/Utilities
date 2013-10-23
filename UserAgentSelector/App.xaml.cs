using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using Microsoft.Win32;

namespace UserAgentSelector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        App()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 3)
            {
                IEMode mode = new IEMode { Version = args[2] };

                UserAgentSelector.MainWindow.RegistryMode regMode;
                if (Enum.TryParse<UserAgentSelector.MainWindow.RegistryMode>(args[1], out regMode) == false)
                {
                    return;
                }

                SetIEModeInRegistry(regMode, mode);
            }
        }

        internal static void SetIEMode(UserAgentSelector.MainWindow.RegistryMode regMode, IEMode mode)
        {
            string exePath = typeof(App).Assembly.Location;

            if (IsAdministrator() == false)
            {
                try
                {
                    ProcessStartInfo procInfo = new ProcessStartInfo();
                    procInfo.UseShellExecute = true;
                    procInfo.FileName = exePath;
                    procInfo.WorkingDirectory = Environment.CurrentDirectory;
                    procInfo.Verb = "runas";
                    procInfo.Arguments = string.Format("{0} \"{1}\"", regMode, mode.Version);
                    Process.Start(procInfo);
                }
                catch (Exception)
                {
                }

                Application.Current.Shutdown(0);

                return;
            }

            SetIEModeInRegistry(regMode, mode);
        }

        private static void SetIEModeInRegistry(UserAgentSelector.MainWindow.RegistryMode regMode, IEMode mode)
        {
            string userAgentPath = UserAgentSelector.MainWindow.Wow64Path;
            if (regMode == UserAgentSelector.MainWindow.RegistryMode.Normal)
            {
                userAgentPath = UserAgentSelector.MainWindow.NormalPath;
            }

            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(userAgentPath, true);

            if (regKey == null)
            {
                regKey = Registry.LocalMachine.CreateSubKey(userAgentPath);
            }

            using (regKey)
            {

                if (string.IsNullOrEmpty(mode.Version) == true)
                {
                    regKey.DeleteValue("Version");
                }
                else
                {
                    regKey.SetValue("Version", mode.Version);
                }
            }
        }

        static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (null != identity)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }

            return false;
        }

    }
}
