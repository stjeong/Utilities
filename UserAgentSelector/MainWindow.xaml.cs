using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace UserAgentSelector
{
    /// <summary>
    /// http://res.sysnet.pe.kr/bin/UserAgentSelector/publish.htm
    /// http://res.sysnet.pe.kr/bin/UserAgentSelector/UserAgentSelector.application
    /// http://res.sysnet.pe.kr/bin/UserAgentSelector/UserAgentSelector.zip
    /// Interaction logic for MainWindow.xaml
    /// Free icon from: https://www.iconfinder.com/icons/63469/domain_email_host_hosting_mysql_php_phpmyadmin_icon
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public enum RegistryMode
        {
            WoW64,
            Normal,
        }

        public const string Wow64Path = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Internet Settings\5.0\User Agent";
        public const string NormalPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\5.0\User Agent";
        public const string AutoStartPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public const string StartupKey = @"www.sysnet.pe.kr.UserAgentSelector";

        ObservableCollection<IEMode> _version32List = new ObservableCollection<IEMode>();
        public ObservableCollection<IEMode> Version32List
        {
            get { return _version32List; }
        }

        ObservableCollection<IEMode> _version64List = new ObservableCollection<IEMode>();
        public ObservableCollection<IEMode> Version64List
        {
            get { return _version64List; }
        }

        bool _runWhenWindowsStarts;
        public bool RunWhenWindowsStarts
        {
            get { return _runWhenWindowsStarts; }
            set
            {
                if (_runWhenWindowsStarts == value)
                {
                    return;
                }

                _runWhenWindowsStarts = value;
                OnPropertyChanged("RunWhenWindowsStarts");
            }
        }

        public bool Is64Bit
        {
            get
            {
                return Environment.Is64BitOperatingSystem;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            MinimizeToTray.Enable(this);
            this.RunWhenWindowsStarts = GetAutoStartup();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AddItem("Internet Explorer 8", "MSIE 8.0");
            AddItem("Internet Explorer 9", "MSIE 9.0");
            AddItem("Internet Explorer 10", "MSIE 10.0");
            AddItem("Internet Explorer 11", null);

            CheckCurrentMode();
        }

        private void CheckCurrentMode()
        {
            if (Environment.Is64BitOperatingSystem == true)
            {
                int selectedIndex = GetSelectedIEMode(Wow64Path);
                _version32List[selectedIndex].Selected = true;
            }

            {
                int selectedIndex = GetSelectedIEMode(NormalPath);
                if (Environment.Is64BitOperatingSystem == true)
                {
                    _version64List[_version64List.Count - 1].Selected = true;
                }
                else
                {
                    _version32List[_version32List.Count - 1].Selected = true;
                }
            }
        }

        private int GetSelectedIEMode(string userAgentPath)
        {
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(userAgentPath))
            {
                int selectedIndex = _version32List.Count - 1;

                if (regKey != null)
                {
                    string value = regKey.GetValue("Version") as string;
                    if (string.IsNullOrEmpty(value) == false)
                    {
                        for (int idx = 0; idx < _version32List.Count - 1; idx ++)
                        {
                            IEMode item = _version32List[idx];

                            if (item.Version.Equals(value, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                selectedIndex = idx;
                            }
                        }
                    }
                }

                return selectedIndex;
            }
        }

        private void AddItem(string ieItem, string versionText)
        {
            this._version32List.Add(new IEMode { Selected = false, Name = ieItem, Version = versionText });
            this._version64List.Add(new IEMode { Selected = false, Name = ieItem, Version = versionText });
        }

        private void githubLink_Clicked(object sender, RequestNavigateEventArgs e)
        {
            Hyperlink source = sender as Hyperlink;

            if (source != null)
            {
                System.Diagnostics.Process.Start(source.NavigateUri.ToString());
            }
        }

        private void IEModeRadio_Changed(object sender, RoutedEventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio == null)
            {
                return;
            }

            IEMode mode = radio.Tag as IEMode;
            if (mode == null)
            {
                return;
            }

            switch (radio.GroupName)
            {
                case "IEMode32":
                    if (Environment.Is64BitOperatingSystem == true)
                    {
                        App.SetIEMode(RegistryMode.WoW64, mode);
                    }
                    else
                    {
                        App.SetIEMode(RegistryMode.Normal, mode);
                    }
                    break;

                case "IEMode64":
                    App.SetIEMode(RegistryMode.Normal, mode);
                    break;
            }
        }

        private void RunStartup_Checked(object sender, RoutedEventArgs e)
        {
            SetAutoStartup();
        }

        private void SetAutoStartup()
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(AutoStartPath, true))
            {
                if (regKey == null)
                {
                    return;
                }

                if (this.RunWhenWindowsStarts == true)
                {
                    string exePath = typeof(App).Assembly.Location;
                    regKey.SetValue(StartupKey, exePath);
                }
                else
                {
                    regKey.DeleteValue(StartupKey);
                }
            }
        }
 
        private bool GetAutoStartup()
        {
            using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(AutoStartPath))
            {
                string path = regKey.GetValue(StartupKey) as string;
                if (string.IsNullOrEmpty(path) == false)
                {
                    string exePath = typeof(App).Assembly.Location;
                    return exePath.Equals(path, StringComparison.OrdinalIgnoreCase);
                }

                return false;
            }
        }

        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
