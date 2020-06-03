using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace Piceon.Models
{
    public class TagFilterPageTagItem : INotifyPropertyChanged
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            set { Set(ref _text, value); }
        }

        private SolidColorBrush _color;
        public SolidColorBrush Color
        {
            get { return _color; }
            set { Set(ref _color, value); }
        }

        private bool _deletable;
        public bool Deletable
        {
            get { return _deletable; }
            set { Set(ref _deletable, value); }
        }

        public TagFilterPageTagItem(string text, SolidColorBrush color, bool deletable = false)
        {
            Text = text;
            Color = color;
            Deletable = deletable;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
