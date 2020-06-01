using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Piceon.Controls
{
    public sealed partial class TagItem : UserControl, INotifyPropertyChanged
    {
        private string _text;
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set
            {
                SetValue(TextProperty, value);
                Set(ref _text, value);
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TagItem), new PropertyMetadata(0));


        public bool Deletable
        {
            get { return (bool)GetValue(DeletableProperty); }
            set
            {
                SetValue(DeletableProperty, value);
                deleteTagButton.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // Using a DependencyProperty as the backing store for Deletable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeletableProperty =
            DependencyProperty.Register("Deletable", typeof(bool), typeof(TagItem), new PropertyMetadata(0));



        public event EventHandler DeleteClicked;

        public TagItem()
        {
            this.InitializeComponent();
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

        private void DeleteTagButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteClicked?.Invoke(this, new EventArgs());
        }
    }
}
