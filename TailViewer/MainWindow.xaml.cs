using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using TailViewer.Properties;

namespace TailViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string _path;
        public string Path
        {
            get { return _path; }
            set
            {
                if (_path == value)
                {
                    return;
                }

                _path = value;

                Settings.Default.Path = this.Path;
                Settings.Default.Save();

                OnPropertyChanged("Path");
            }
        }

        string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                if (_filter == value)
                {
                    return;
                }

                _filter = value;

                Settings.Default.Filter = this.Filter;
                Settings.Default.Save();

                OnPropertyChanged("Filter");
            }
        }

        ObservableCollection<string> _fileList = new ObservableCollection<string>();
        public ObservableCollection<string> FileList
        {
            get { return _fileList; }
        }

        ObservableCollection<TextLine> _lines = new ObservableCollection<TextLine>();
        public ObservableCollection<TextLine> Lines
        {
            get { return _lines; }
        }

        public MainWindow()
        {
            InitializeComponent();

            this.Path = Settings.Default.Path;
            this.Filter = Settings.Default.Filter;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string name)
        {
            if (PropertyChanged == null)
            {
                return;
            }

            if (name == "Path")
            {
                ListFiles();
            }

            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        FileTextWatcher _watcher;
        private void ListFiles(string path = "", string filePath = "")
        {
            if (string.IsNullOrEmpty(path) == true)
            {
                path = this.Path;
            }

            if (string.IsNullOrEmpty(path) == true)
            {
                return;
            }

            _fileList.Clear();

            foreach (var item in Directory.GetFiles(path))
            {
                string fileName = System.IO.Path.GetFileName(item);
                _fileList.Add(fileName);
            }

            if (string.IsNullOrEmpty(filePath) == false)
            {
                lstFiles.SelectedItem = System.IO.Path.GetFileName(filePath);
            }
        }

        private void ApplyButton_Clicked(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.Path = dialog.FileName;
            }
        }

        private void FileListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Lines.Clear();

            if (e.AddedItems.Count == 0)
            {
                return;
            }

            string text = e.AddedItems[0] as string;
            string filePath = System.IO.Path.Combine(this.Path, text);

            Settings.Default.FilePath = filePath;
            Settings.Default.Save();

            WatcherStart(filePath, this.Filter);
        }
        private void WatcherStart(string filePath, string filter)
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
            }

            if (File.Exists(filePath) == false)
            {
                Settings.Default.FilePath = null;
                Settings.Default.Save();

                ListFiles();
                return;
            }

            _watcher = new FileTextWatcher(filePath, filter);
            _watcher.LineAdded += _watcher_LineAdded;
            _watcher.FolderChanged += _watcher_FolderChanged;
            _watcher.Start();
        }

        void _watcher_FolderChanged(object sender, EventArgs e)
        {
            lock (lstFiles)
            {
                if (Dispatcher.CheckAccess() == false)
                {
                    this.Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        (ThreadStart)(
                        () =>
                        {
                            _watcher_FolderChanged(sender, e);
                        }));
                    return;
                }

                ListFiles(string.Empty, Settings.Default.FilePath);
            }
        }

        void _watcher_LineAdded(object sender, EventArgs e)
        {
            if (Dispatcher.CheckAccess() == false)
            {
                this.Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    (ThreadStart)(
                    () =>
                    {
                        _watcher_LineAdded(sender, e);
                    }));
                return;
            }

            TailViewer.FileTextWatcher.FileChangedArgs args = e as TailViewer.FileTextWatcher.FileChangedArgs;

            if (args.ClearAll == true)
            {
                this.Lines.Clear();
            }

            foreach (var item in args.Lines)
            {
                this.Lines.Add(new TextLine { LineNumber = _watcher.Line++, Text = item });
            }

            if (fileView.Items.Count != 0)
            {
                fileView.SelectedIndex = this.Lines.Count - 1;
                fileView.ScrollIntoView(fileView.Items[fileView.Items.Count - 1]);
            }
        }

        private void DeleteFile_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (MessageBox.Show("Delete it?", "File Delete", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.None) == MessageBoxResult.Yes)
                {
                    lock (lstFiles)
                    {
                        List<string> deleted = new List<string>();

                        foreach (var item in lstFiles.SelectedItems)
                        {
                            string filePath = System.IO.Path.Combine(this.Path, item as string);

                            try
                            {
                                System.IO.File.Delete(filePath);
                                deleted.Add(item as string);
                            }
                            catch
                            {
                            }
                        }

                        foreach (var item in deleted)
                        {
                            _fileList.Remove(item);
                        }
                    }
                }
            }
        }
    }
}
