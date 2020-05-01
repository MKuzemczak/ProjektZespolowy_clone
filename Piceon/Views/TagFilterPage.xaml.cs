using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Piceon.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TagFilterPage : Page
    {
        public List<string> AllTags { get; private set; } = new List<string>();

        public ObservableCollection<string> FilteredTags = new ObservableCollection<string>();
        public ObservableCollection<string> SelectedTags = new ObservableCollection<string>();

        public event EventHandler<SelectedTagsChangedEventArgs> SelectedTagsChanged;

        public TagFilterPage()
        {
            this.InitializeComponent();
            PopulateFiltered(AllTags);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFiltered();
        }

        public void SetTagList(List<string> list)
        {
            AllTags = list;
            UpdateFiltered();
        }

        private void UpdateFiltered()
        {
            if (searchBox.Text.Any())
            {
                var list = AllTags.Where(i => i.Contains(searchBox.Text)).ToList();
                var sorted = list.OrderByDescending(i => i.StartsWith(searchBox.Text)).ToList();
                PopulateFiltered(sorted);
            }
            else
            {
                PopulateFiltered(AllTags);
            }
        }

        private void PopulateFiltered(List<string> list)
        {
            FilteredTags.Clear();
            foreach (var item in list)
            {
                FilteredTags.Add(item);
            }
        }

        private void SelectedTagsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedTags.Remove(e.ClickedItem as string);
            SelectedTagsChanged?.Invoke(this, new SelectedTagsChangedEventArgs(SelectedTags.ToList()));
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (SelectedTags.Contains(e.ClickedItem as string))
                return;

            SelectedTags.Add(e.ClickedItem as string);
            SelectedTagsChanged?.Invoke(this, new SelectedTagsChangedEventArgs(SelectedTags.ToList()));
        }
    }

    public class SelectedTagsChangedEventArgs : EventArgs
    {
        public List<string> SelectedTags { get; }

        public SelectedTagsChangedEventArgs(List<string> selectedTags)
        {
            SelectedTags = selectedTags;
        }
    }
}
