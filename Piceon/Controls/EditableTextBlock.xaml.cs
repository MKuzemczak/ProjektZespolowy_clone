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

using Piceon.Models;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Piceon.Controls
{
    public sealed partial class EditableTextBlock : UserControl
    {
        public EditableTextBlock()
        {
            this.InitializeComponent();
        }



        public string TextBlockText
        {
            get { return (string)GetValue(TextBlockTextProperty); }
            set
            {
                SetValue(TextBlockTextProperty, value);
                textBlock.Text = value;
            }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextBlockTextProperty =
            DependencyProperty.Register("TextBlockText", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(0));



        public FolderItem AssociatedFolderItem
        {
            get { return (FolderItem)GetValue(AssociatedFolderItemProperty); }
            set { SetValue(AssociatedFolderItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AssociatedFolderItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssociatedFolderItemProperty =
            DependencyProperty.Register("AssociatedFolderItem", typeof(FolderItem), typeof(EditableTextBlock), new PropertyMetadata(0));



        public void EnableEditMode()
        {
            textBox.Text = TextBlockText;
            textBlock.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Visible;
            textBox.Focus(FocusState.Programmatic);
            textBox.SelectAll();
        }

        public event EventHandler<EditableTextBlockTextChangedEventArgs> TextChanged;

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextChanged?.Invoke(this, new EditableTextBlockTextChangedEventArgs((sender as TextBox).Text));
            textBox.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Visible;
        }
    }
}
