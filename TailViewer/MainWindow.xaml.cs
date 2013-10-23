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

        private void ListFiles(string path = "")
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

            int number = 0;
            foreach (var item in File.ReadAllLines(filePath))
            {
                this.Lines.Add(new TextLine { LineNumber = number++, Text = item });
            }

            if (this.Lines.Count == 0)
            {
                return;
            }

            fileView.ScrollIntoView(fileView.Items[fileView.Items.Count - 1]);
        }
    }
}
