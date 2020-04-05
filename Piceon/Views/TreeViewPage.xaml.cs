using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Piceon.Controls;
using Piceon.DatabaseAccess;
using Piceon.Models;
using Piceon.Services;

using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Piceon.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TreeViewPage : Page, INotifyPropertyChanged
    {
        private static bool AlreadyLoaded = false;
        private static ObservableCollection<FolderItem> PreviouslyAccessedDirectories = new ObservableCollection<FolderItem>();

        private FolderItem _selectedItem;

        private EditableTextBlock RightClickedTreeViewItemEditableTextBlock;

        private bool ItemInvokedWithThisClick = false;

        public FolderItem SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }

        public ObservableCollection<FolderItem> Directories { get; } = new ObservableCollection<FolderItem>();

        public TreeViewPage()
        {
            this.InitializeComponent();
            Loaded += TreeViewPage_OnLoaded;
        }

        private async void TreeViewPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (AlreadyLoaded)
            {
                foreach (var item in PreviouslyAccessedDirectories)
                    Directories.Add(item);
                return;
            }

            await ReloadFolders();

            AlreadyLoaded = true;
        }

        public async Task AddFolder(StorageFolder folder)
        {
            var folderItem = await StorageFolderItem.FromStorageFolder(folder);
            if (FolderTreeContains(Directories, folderItem))
                return;
            Directories.Add(folderItem);
            PreviouslyAccessedDirectories.Add(Directories.Last());
        }




        public async Task ReloadFolders()
        {
            Directories.Clear();
            PreviouslyAccessedDirectories.Clear();

            var virtualFoldersRootNodes = await DatabaseAccessService.GetRootVirtualFoldersAsync();

            foreach (var item in virtualFoldersRootNodes)
            {
                Directories.Add(await VirtualFolderItem.FromDatabaseVirtualFolder(item));
            }

            var tokenList = await DatabaseAccessService.GetAccessedFoldersAsync();

            foreach (var token in tokenList)
            {
                var storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                Directories.Add(await StorageFolderItem.FromStorageFolder(storageFolder));
            }

            // wait for treeview to load data
            if (Directories.Count == 1)
            {
                await Task.Delay(500);
                treeView.Expand(treeView.RootNodes[0]);
            }

            foreach (var item in Directories)
                PreviouslyAccessedDirectories.Add(item);
        }

        public event EventHandler<TreeViewItemSelectedEventArgs> ItemSelected;

        private void OnItemInvoked(WinUI.TreeView sender, WinUI.TreeViewItemInvokedEventArgs args)
        {
            if (typeof(FolderItem).IsAssignableFrom(args.InvokedItem.GetType()))
            {
                ItemInvokedWithThisClick = true;
                SelectedItem = args.InvokedItem as FolderItem;
                ItemSelected?.Invoke(this, new TreeViewItemSelectedEventArgs(SelectedItem));
            }
        }

        private void OnCollapseAll(object sender, RoutedEventArgs e)
            => CollapseNodes(treeView.RootNodes);

        private void CollapseNodes(IList<WinUI.TreeViewNode> nodes)
        {
            foreach (var node in nodes)
            {
                CollapseNodes(node.Children);
                treeView.Collapse(node);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private async void OnOpenFolder(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                string token = StorageApplicationPermissions.FutureAccessList.Add(folder);
                await AddFolder(folder);
                DatabaseAccessService.AddAccessedFolderAsync(token);
            }
        }

        private async void OnAddFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                var folderItem = await VirtualFolderItem.GetNew("New Album");

                if (treeView.SelectedItem != null)
                {
                    try
                    {
                        await folderItem.SetParentAsync(treeView.SelectedItem as FolderItem);
                    }
                    catch (Exception exception)
                    {
                        var messageDialog = new MessageDialog("Error: " + exception.Message);
                        messageDialog.Commands.Add(new UICommand("Close"));
                        messageDialog.DefaultCommandIndex = 0;
                        messageDialog.CancelCommandIndex = 0;
                        await messageDialog.ShowAsync();
                        await folderItem.DeleteAsync();
                        return;
                    }
                }

                var newNode = new WinUI.TreeViewNode() { Content = folderItem };

                if (treeView.SelectedNode != null)
                {
                    treeView.SelectedNode.Children.Add(newNode);
                    treeView.SelectedNode.HasUnrealizedChildren = true;
                    treeView.Expand(treeView.SelectedNode);
                }
                else
                {
                    treeView.RootNodes.Add(newNode);
                }

                treeView.SelectedNodes.Clear();
                treeView.SelectedNodes.Add(newNode);
                await Task.Delay(100);
                var container = treeView.ContainerFromNode(newNode);

                if (container != null)
                {
                    var tvi = container as WinUI.TreeViewItem;
                    (tvi?.Content as EditableTextBlock)?.EnableEditMode();
                }
            }
            catch (Exception exception)
            {
                var messageDialog = new MessageDialog("Error: " + exception.Message);
                messageDialog.Commands.Add(new UICommand("Close"));
                messageDialog.DefaultCommandIndex = 0;
                messageDialog.CancelCommandIndex = 0;
                await messageDialog.ShowAsync();
                return;
            }
            
        }

        private void TreeViewItemMenuFlyout_Rename(object sender, RoutedEventArgs e)
        {
            RightClickedTreeViewItemEditableTextBlock.EnableEditMode();
        }

        private async void TreeViewItemEditableTextBlock_TextChanged(object sender, EditableTextBlockTextChangedEventArgs e)
        {
            var folderItem = (sender as EditableTextBlock).DataContext as FolderItem;

            if (e.Text != folderItem.Name && e.Text.Any())
            {
                try
                {
                    await folderItem.RenameAsync(e.Text);
                }
                catch (FormatException)
                {
                    var messageDialog = new MessageDialog("Error: Album name cannot contain any of these characters: \\/:*?\"<>|");
                    messageDialog.Commands.Add(new UICommand("Close"));
                    messageDialog.DefaultCommandIndex = 0;
                    messageDialog.CancelCommandIndex = 0;
                    await messageDialog.ShowAsync();
                    (sender as EditableTextBlock).EnableEditMode();
                    return;
                }
                catch (Exception exception)
                {
                    var messageDialog = new MessageDialog("Error: " + exception.Message);
                    messageDialog.Commands.Add(new UICommand("Close"));
                    messageDialog.DefaultCommandIndex = 0;
                    messageDialog.CancelCommandIndex = 0;
                    await messageDialog.ShowAsync();
                    return;
                }
                var cont = treeView.ContainerFromItem(folderItem);
                var node = treeView.NodeFromContainer(cont);
                if (node.Depth == 0)
                {
                    await ReloadFolders();
                }
                else
                {
                    treeView.Collapse(node.Parent);
                    await Task.Delay(50);
                    treeView.Expand(node.Parent);
                }
            }
        }

        private void TreeViewItemMenuFlyout_Opened(object sender, object e)
        {
            RightClickedTreeViewItemEditableTextBlock = ((sender as MenuFlyout).Target as WinUI.TreeViewItem).Content as EditableTextBlock;
        }

        private void Grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ItemInvokedWithThisClick)
            {
                ItemInvokedWithThisClick = false;
                return;
            }

            treeView.SelectedItem = null;
        }

        private bool FolderTreeContains(ICollection<FolderItem> tree, FolderItem folder)
        {
            foreach (var item in tree)
            {
                if (item == folder)
                    return true;

                if (item.Subfolders.Any())
                    FolderTreeContains(item.Subfolders, folder);
            }

            return false;
        }

        private async void OnDeleteFolder(object sender, RoutedEventArgs e)
        {
            var folderItem = (sender as MenuFlyoutItem).DataContext as FolderItem;
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Delete folder permanently?",
                Content = "All subfolders will be deleted. Continue?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };

            ContentDialogResult result = await deleteFileDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await folderItem.DeleteAsync();
                Directories.Remove(folderItem);
            }
            
        }
    }
}
