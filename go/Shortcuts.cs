using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace go
{
    public class Shortcuts
    {
        public class ShortcutItem
        {
            string _key, _value;

            public string Key { get { return _key; } set { _key = value; } }
            public string Value { get { return _value; } set { _value = value; } }
        }

        List<ShortcutItem> _map = new List<ShortcutItem>();
        public List<ShortcutItem> Map
        {
            get { return _map; }
            set { _map = value; }
        }

        string _dataPath;
        string DataPath
        {
            get
            {
                if (string.IsNullOrEmpty(_dataPath) == true)
                {
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    _dataPath = System.IO.Path.Combine(path, "sysnet_go.dat");
                }

                return _dataPath;
            }
        }

        public void Save()
        {
            using (FileStream fs = new FileStream(this.DataPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlSerializer xs = new XmlSerializer(typeof(Shortcuts));
                xs.Serialize(fs, this);
            }
        }

        public void Load()
        {
            if (File.Exists(this.DataPath) == false)
            {
                return;
            }

            try 
            { 
                using (FileStream fs = new FileStream(this.DataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(Shortcuts));
                    this.Map = (xs.Deserialize(fs) as Shortcuts).Map;
                }
            } 
            catch (InvalidOperationException)
            {
            }
        }

        internal bool ContainsKey(string key)
        {
            return _map.Exists( (e) => e.Key == key);
        }

        internal void Remove(string key)
        {
            _map.RemoveAll((e) => e.Key == key);
        }

        internal void Add(string key, string value)
        {
            _map.RemoveAll( (e) => e.Key == key);
            _map.Add(new ShortcutItem { Key = key, Value = value });
        }

        internal string Get(string key)
        {
            var item = _map.Find( (e) => e.Key == key);

            if (item == null)
            {
                return string.Empty;
            }

            return item.Value;
        }
    }
}
