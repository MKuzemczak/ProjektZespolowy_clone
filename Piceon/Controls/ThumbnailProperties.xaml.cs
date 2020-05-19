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
    public sealed partial class ThumbnailProperties : UserControl
    {
        public bool IsCheckMarkVisible
        {
            get { return (bool)GetValue(IsCheckMarkVisibleProperty); }
            set
            {
                SetValue(IsCheckMarkVisibleProperty, value);
                UpdateCheckMarkVisibility();
            }
        }

        // Using a DependencyProperty as the backing store for IsVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsCheckMarkVisibleProperty =
            DependencyProperty.Register("IsCheckMarkVisible", typeof(bool), typeof(ThumbnailProperties), new PropertyMetadata(0));

        public int QualityValue
        {
            get { return (int)GetValue(QualityValueProperty); }
            set
            {
                SetValue(QualityValueProperty, value);
                SetQualityValueText(value);
            }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QualityValueProperty =
            DependencyProperty.Register("QualityValue", typeof(int), typeof(ThumbnailProperties), new PropertyMetadata(0));


        public ThumbnailProperties()
        {
            this.InitializeComponent();
        }

        private void UpdateCheckMarkVisibility()
        {
            checkMarkImage.Visibility = IsCheckMarkVisible ? Visibility.Visible : Visibility.Collapsed;
        }


        private void SetQualityValueText(int value)
        {
            textBlock.Text = $"{value} %";
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            parentGrid.Visibility = Visibility.Visible;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            parentGrid.Visibility = Visibility.Collapsed;
        }

    }
}
