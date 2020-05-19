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
    public sealed partial class ImageQualityDisplay : UserControl
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
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(ImageQualityDisplay), new PropertyMetadata(0));



        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                SetValue(ValueProperty, value);
                SetQualityValueText(value);
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(ImageQualityDisplay), new PropertyMetadata(0));




        public ImageQualityDisplay()
        {
            this.InitializeComponent();
        }

        private void UpdateCheckMarkVisibility()
        {
            textBlock.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetQualityValueText(int value)
        {
            textBlock.Text = $"{value} %";
        }

        private void UserControl_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            textBlock.Visibility = Visibility.Visible;
            e.Handled = false;
        }

        private void UserControl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            textBlock.Visibility = Visibility.Collapsed;
            e.Handled = false;
        }
    }
}
