using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class CheckMark : UserControl
    {
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set
            {
                SetValue(IsVisibleProperty, value);
                UpdateCheckMarkVisibility();
            }
        }

        // Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(CheckMark), new PropertyMetadata(0));



        public CheckMark()
        {
            this.InitializeComponent();
        }

        private void UpdateCheckMarkVisibility()
        {
            checkMarkImage.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
