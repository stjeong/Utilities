using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TailViewer
{
    public class TextLine : INotifyPropertyChanged
    {
        int _lineNumber;
        public int LineNumber
        {
            get { return _lineNumber; }
            set
            {
                if (_lineNumber == value)
                {
                    return;
                }

                _lineNumber = value;
                OnPropertyChanged("LineNumber");
            }
        }

        string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                {
                    return;
                }

                _text = value;
                OnPropertyChanged("Text");
            }
        }

        public override bool Equals(object obj)
        {
            TextLine line = obj as TextLine;
            if (line == null)
            {
                return false;
            }

            return line.LineNumber == this.LineNumber;
        }

        public override int GetHashCode()
        {
            return LineNumber.GetHashCode();
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
