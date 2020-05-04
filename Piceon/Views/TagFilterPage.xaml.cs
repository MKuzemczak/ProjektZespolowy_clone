using Piceon.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

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

        public async Task AccessFolder(FolderItem folder)
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
