using Piceon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Piceon.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TagFilterPage : Page
    {
        private SolidColorBrush NotHighlightedColorBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorDark3"]);
        private SolidColorBrush HighlightedColorBrush = new SolidColorBrush((Color)Application.Current.Resources["SystemAccentColorLight1"]);
        public List<string> AllTags { get; private set; } = new List<string>();

        public ObservableCollection<TagFilterPageTagItem> FilteredTags = new ObservableCollection<TagFilterPageTagItem>();
        public ObservableCollection<TagFilterPageTagItem> SelectedTags = new ObservableCollection<TagFilterPageTagItem>();

        public event EventHandler<SelectedTagsChangedEventArgs> SelectedTagsChanged;

        private FolderItem AccessedFolder { get; set; }

        public TagFilterPage()
        {
            this.InitializeComponent();
            PopulateFiltered(AllTags);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFiltered();
        }

        public async Task AccessFolderAsync(FolderItem folder)
        {
            if (folder is null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (AccessedFolder is object)
                AccessedFolder.ContentsChanged -= AccessedFolderContentsChangedHandler;

            AccessedFolder = folder;
            AccessedFolder.ContentsChanged += AccessedFolderContentsChangedHandler;
            await ScanAccessedFolderForTags();
        }

        private async void AccessedFolderContentsChangedHandler(object sender, EventArgs e)
        {
            await ScanAccessedFolderForTags();
        }

        private async Task ScanAccessedFolderForTags()
        {
            AllTags = await AccessedFolder.GetTagsOfImagesAsync();
            UpdateFiltered();
        }

        private void UpdateFiltered()
        {
            if (searchBox.Text.Any())
            {
                var list = AllTags.Where(i => i.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase)).ToList();
                var sorted = list.OrderByDescending(i => i.StartsWith(searchBox.Text, StringComparison.OrdinalIgnoreCase)).ToList();
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
                FilteredTags.Add(CreateTagItem(item));
            }
        }

        private void SelectedTagsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedTags.Remove(e.ClickedItem as TagFilterPageTagItem);
            SelectedTagsChanged?.Invoke(this, new SelectedTagsChangedEventArgs(SelectedTags.Select(i => i.Text).ToList()));
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (SelectedTags.Contains(e.ClickedItem as TagFilterPageTagItem))
                return;

            SelectedTags.Add(e.ClickedItem as TagFilterPageTagItem);
            SelectedTagsChanged?.Invoke(this, new SelectedTagsChangedEventArgs(SelectedTags.Select(i => i.Text).ToList()));
        }

        public void HighlightTags(List<string> tags)
        {
            ClearAllTagHighlighting();
            var filtered = FilteredTags.OrderByDescending(
                i =>
                {
                    if (tags.Contains(i.Text))
                    {
                        i.Color = HighlightedColorBrush;
                        return true;
                    }
                    return false;
                }).ToList();
            FilteredTags.Clear();
            foreach (var item in filtered)
            {
                FilteredTags.Add(item);
            }
            var selected = SelectedTags.OrderByDescending(
                i =>
                {
                    if (tags.Contains(i.Text))
                    {
                        i.Color = HighlightedColorBrush;
                        return true;
                    }
                    return false;
                }).ToList();
            SelectedTags.Clear();
            foreach (var item in selected)
            {
                SelectedTags.Add(item);
            }
        }

        public void ClearAllTagHighlighting()
        {
            foreach (var item in FilteredTags)
            {
                item.Color = NotHighlightedColorBrush;
            }
            foreach (var item in SelectedTags)
            {
                item.Color = NotHighlightedColorBrush;
            }
        }

        private TagFilterPageTagItem CreateTagItem(string text)
        {
            return new TagFilterPageTagItem(text, NotHighlightedColorBrush);
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
