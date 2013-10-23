using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace UserAgentSelector
{
    public class IEMode : INotifyPropertyChanged
    {
        bool _selected;
        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected == value)
                {
                    return;
                }

                _selected = value;
                OnPropertyChanged("Selected");
            }
        }

        string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                OnPropertyChanged("Name");
            }
        }

        string _version;
        public string Version
        {
            get { return _version; }
            set
            {
                if (_version == value)
                {
                    return;
                }

                _version = value;
                OnPropertyChanged("Version");
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
