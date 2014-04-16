using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TailViewer
{
    public class FileTextWatcher : IDisposable
    {
        FileSystemWatcher _watcherFile;
        FileSystemWatcher _watcherFolder;
        bool _exitThread;

        int _position;

        string _filter;

        int _line;
        public int Line
        {
            get { return _line; }
            set { _line = value; }
        }

        Encoding _encoding;
        string _filePath;

        public event EventHandler LineAdded;
        public event EventHandler FolderChanged;

        byte[] _newLineBuf;

        public FileTextWatcher(string pathToMonitor, string filter)
        {
            _filePath = pathToMonitor;

            _filter = filter;
            _position = 0;
            _line = 0;
            _encoding = GetFileEncoding(_filePath);

            _newLineBuf = _encoding.GetBytes("\n");
        }

        Task _touchTask;
        CancellationTokenSource _cancelToken;
        EventWaitHandle _ewhExit;
        DateTime _oldTime;

        public void Start()
        {
            ReadLines();

            string folder = Path.GetDirectoryName(_filePath);
            string fileName = Path.GetFileName(_filePath);

            string ext = Path.GetExtension(fileName);

            _oldTime = File.GetLastWriteTime(_filePath);

            _watcherFile = new FileSystemWatcher(folder, fileName);
            _watcherFile.IncludeSubdirectories = false;
            _watcherFile.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            _watcherFile.Changed += _watcher_FileChanged;
            _watcherFile.EnableRaisingEvents = true;

            _watcherFolder = new FileSystemWatcher(folder);
            _watcherFolder.IncludeSubdirectories = false;
            _watcherFolder.Created += _watcher_FolderChanged;
            _watcherFolder.Deleted += _watcher_FolderChanged;
            _watcherFolder.Renamed += _watcher_FolderChanged;
            _watcherFolder.EnableRaisingEvents = true;

            if (Environment.OSVersion.Version.Major < 6)
            {
                return; // Vista or later.
            }

            _cancelToken = new CancellationTokenSource();

            CancellationToken ct = _cancelToken.Token;

            _touchTask = new Task(
                () =>
                {
                    _ewhExit = new EventWaitHandle(false, EventResetMode.ManualReset);

                    while (true)
                    {
                        if (ct.IsCancellationRequested == true)
                        {
                            break;
                        }

                        DateTime current = File.GetLastWriteTime(_filePath);
                        if (_oldTime != current)
                        {
                            _oldTime = current;

                            _watcher_FileChanged(_watcherFile, new FileSystemEventArgs(WatcherChangeTypes.Changed, folder, fileName));
                        }

                        _ewhExit.WaitOne(500);
                        if (_exitThread == true)
                        {
                            break;
                        }
                    }
                }, _cancelToken.Token);

            _touchTask.Start();
        }

        void _watcher_FolderChanged(object sender, FileSystemEventArgs e)
        {
            OnFolderChanged();
        }

        private Encoding GetFileEncoding(string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader sr = new StreamReader(fs, true))
            {
                if (sr.CurrentEncoding.CodePage == 65001) // In case: Unicode without BOM
                {
                    char[] contents = new char[1024];
                    sr.Read(contents, 0, 1024);

                    bool unicodeEncoding = true;
                    for (int i = 1; i < contents.Length; i += 2)
                    {
                        if (contents[i] != '\0')
                        {
                            unicodeEncoding = false;
                            break;
                        }
                    }

                    if (unicodeEncoding == true)
                    {
                        return Encoding.Unicode;
                    }
                }

                return sr.CurrentEncoding;
            }
        }

        private void _watcher_FileChanged(object sender, FileSystemEventArgs e)
        {
            ReadLines();
        }

        private void ReadLines()
        {
            using (FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BinaryReader br = new BinaryReader(fs))
            {
                bool clearAll = false;
                long fileLength = fs.Length;

                if (fileLength > Int32.MaxValue)
                {
                    return;
                }

                if (fileLength < _position)
                {
                    _position = 0;
                    clearAll = true;
                }

                fs.Position = _position;

                List<byte> byteBuf = new List<byte>((int)fileLength);

                int lastNewLinePosition = _position;
                List<string> lines = new List<string>();

                for (int i = _position; i < fileLength; i++)
                {
                    byteBuf.Add(br.ReadByte());

                    if (_newLineBuf.Length == 1 && byteBuf[byteBuf.Count - 1] == _newLineBuf[0])
                    {
                        lastNewLinePosition = i + 1;
                    }
                    else if (_newLineBuf.Length == 2 && byteBuf.Count >= 2 && byteBuf[byteBuf.Count - 2] == _newLineBuf[0]
                        && byteBuf[byteBuf.Count - 1] == _newLineBuf[1])
                    {
                        lastNewLinePosition = i + 1;
                    }

                    if (_position != lastNewLinePosition)
                    {
                        byte[] arrLine = byteBuf.ToArray();

                        using (MemoryStream ms = new MemoryStream(arrLine, 0, arrLine.Length))
                        {
                            using (StreamReader sr = new StreamReader(ms, _encoding))
                            {
                                while (sr.EndOfStream == false)
                                {
                                    string txt = sr.ReadLine();
                                    bool add = true;

                                    if (string.IsNullOrEmpty(this._filter) == false)
                                    {
                                        if (txt.IndexOf(this._filter, StringComparison.OrdinalIgnoreCase) == -1)
                                        {
                                            add = false;
                                        }
                                    }

                                    if (add == true)
                                    {
                                        lines.Add(txt);
                                    }
                                }
                            }
                        }

                        byteBuf.Clear();
                        _position = lastNewLinePosition;
                    }
                }


                OnLinedAdded(lines, lastNewLinePosition, clearAll);
            }
        }

        public void Dispose()
        {
            try
            {
                _cancelToken.Cancel();
            }
            catch { }

            try
            {
                _exitThread = true;
                _ewhExit.Set();
            }
            catch { }

            try
            {
                _watcherFile.Dispose();
            }
            catch { }

            try
            {
                _touchTask.Wait();
            }
            catch { }
        }

        protected virtual void OnFolderChanged()
        {
            if (FolderChanged == null)
            {
                return;
            }

            FolderChanged(this, EventArgs.Empty);
        }

        protected virtual void OnLinedAdded(List<string> lines, long position, bool clearAll)
        {
            if (LineAdded == null)
            {
                return;
            }

            FileChangedArgs args = new FileChangedArgs { Lines = lines, Position = position, ClearAll = clearAll };
            LineAdded(this, args);
        }

        public class FileChangedArgs : EventArgs
        {
            List<string> _lines = new List<string>();
            public List<string> Lines
            {
                get { return _lines; }
                set { _lines = value; }
            }

            long _position;
            public long Position
            {
                get { return _position; }
                set { _position = value; }
            }

            bool _clearAll;
            public bool ClearAll
            {
                get { return _clearAll; }
                set { _clearAll = value; }
            }
        }
    }
}
