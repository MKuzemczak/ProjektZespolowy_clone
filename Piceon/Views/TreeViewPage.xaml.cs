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
using Piceon.Helpers;
using Piceon.Models;
using Piceon.Services;

using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Piceon.Views
{
    public sealed partial class TreeViewPage : Page, INotifyPropertyChanged
    {
        #region PROPERTIES
        private static bool AlreadyLoaded = false;
        private static ObservableCollection<FolderItem> PreviouslyAccessedDirectories = new ObservableCollection<FolderItem>();

        private FolderItem _selectedItem;

        private EditableTextBlock RightClickedTreeViewItemEditableTextBlock;

        private bool ItemInvokedWithThisClick = false;
        private bool IsDragOverItem = false;

        public FolderItem SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }

        public WinUI.TreeViewNode SelectedNode { get; set; }

        public ObservableCollection<FolderItem> Directories { get; } = new ObservableCollection<FolderItem>();
        #endregion
        public TreeViewPage()
        {
            this.InitializeComponent();
            Loaded += TreeViewPage_OnLoaded;
        }

        #region EVENTS
        public event EventHandler<TreeViewItemSelectedEventArgs> ItemSelected;

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region METHODS
        public void AddFolder(FolderItem folderItem)
        {
            if (FolderTreeContains(Directories, folderItem))
                return;
            Directories.Add(folderItem);
            PreviouslyAccessedDirectories.Add(Directories.Last());
        }

        public async Task ReloadFoldersAsync()
        {
            ShowTreeViewLoadingIndicator();
            Directories.Clear();

            var data = await FolderManagerService.GetAllFolders();

            foreach (var item in data)
            {
                Directories.Add(item);
            }

            // wait for treeview to load data
            if (Directories.Count == 1)
            {
                await Task.Delay(500);
                treeView.Expand(treeView.RootNodes[0]);
            }

            foreach (var item in Directories)
                PreviouslyAccessedDirectories.Add(item);

            loadingTextBlock.Visibility = Visibility.Collapsed;
            HideTreeViewLoadingIndicator();
        }

        private void CollapseNodes(IList<WinUI.TreeViewNode> nodes)
        {
            foreach (var node in nodes)
            {
                CollapseNodes(node.Children);
                treeView.Collapse(node);
            }
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

        private void ShowTreeViewLoadingIndicator()
        {
            loadingTextBlock.Visibility = Visibility.Visible;
        }

        private void HideTreeViewLoadingIndicator()
        {
            loadingTextBlock.Visibility = Visibility.Collapsed;
        }

        private FolderItem GetSelectedFolder()
        {
            return SelectedItem;
        }

        private void DeselectAll()
        {
            treeView.SelectionMode = WinUI.TreeViewSelectionMode.None;
            treeView.SelectionMode = WinUI.TreeViewSelectionMode.Single;
            SelectedItem = null;
            SelectedNode = null;
        }

        private void SelectNode(WinUI.TreeViewNode node)
        {
            treeView.SelectedNodes.Clear();
            treeView.SelectedNodes.Add(node);
            SelectedNode = node;
        }

        private WinUI.TreeViewNode GetSelectedNode()
        {
            return SelectedNode;
        }

        private async Task<FolderItem> CreateFolderWithSelectedAsParentAsync(string name)
        {
            try
            {
                var folderItem = await VirtualFolderItem.GetNew(name);

                if (GetSelectedFolder() != null)
                {
                    try
                    {
                        await folderItem.SetParentAsync(GetSelectedFolder());
                    }
                    catch (Exception exception)
                    {
                        var messageDialog = new MessageDialog("Error setting folder parent: " + exception.Message);
                        messageDialog.Commands.Add(new UICommand("Close"));
                        messageDialog.DefaultCommandIndex = 0;
                        messageDialog.CancelCommandIndex = 0;
                        await messageDialog.ShowAsync();
                        await folderItem.DeleteAsync();
                        return null;
                    }
                }

                var newNode = new WinUI.TreeViewNode() { Content = folderItem };

                if (GetSelectedNode() != null)
                {
                    GetSelectedNode().Children.Add(newNode);
                    treeView.Expand(GetSelectedNode());
                }
                else
                {
                    treeView.RootNodes.Add(newNode);
                }

                SelectNode(newNode);
                await Task.Delay(100);
                var container = treeView.ContainerFromNode(newNode);

                if (container != null)
                {
                    var tvi = container as WinUI.TreeViewItem;
                    (tvi?.Content as EditableTextBlock)?.EnableEditMode();
                }

                return folderItem;
            }
            catch (Exception exception)
            {
                var messageDialog = new MessageDialog("Error: " + exception.Message);
                messageDialog.Commands.Add(new UICommand("Close"));
                messageDialog.DefaultCommandIndex = 0;
                messageDialog.CancelCommandIndex = 0;
                await messageDialog.ShowAsync();
                return null;
            }
        }

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        #endregion

        #region PAGE EVENT HANDLERS
        private async void TreeViewPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (AlreadyLoaded)
            {
                foreach (var item in PreviouslyAccessedDirectories)
                    Directories.Add(item);
                return;
            }

            await ReloadFoldersAsync();

            AlreadyLoaded = true;
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region BUTTON EVENT HANDLERS

        private void CollapseAllButton_Click(object sender, RoutedEventArgs e)
            => CollapseNodes(treeView.RootNodes);

        private async void CreateFolderButton_Click(object sender, RoutedEventArgs e)
        {
            await CreateFolderWithSelectedAsParentAsync("New album");
        }

        private async void ImportImagesButton_Click(object sender, RoutedEventArgs e)
        {
            FolderItem folder = null;
            if (GetSelectedFolder() != null)
                folder = GetSelectedFolder();
            else
                folder = await CreateFolderWithSelectedAsParentAsync("Imported images");

            await FolderManagerService.PickAndImportImagesToFolder(folder);
        }

        #endregion

        #region RIGHT CLICK EVENT HANDLERS

        private void TreeViewItemMenuFlyout_Opened(object sender, object e)
        {
            RightClickedTreeViewItemEditableTextBlock = ((sender as MenuFlyout).Target as WinUI.TreeViewItem).Content as EditableTextBlock;
        }

        private void TreeViewItemMenuFlyout_Rename(object sender, RoutedEventArgs e)
        {
            RightClickedTreeViewItemEditableTextBlock.EnableEditMode();
        }

        private async void TreeViewItemMenuFlyout_Delete(object sender, RoutedEventArgs e)
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
                var node = GetSelectedNode();
                await folderItem.DeleteAsync();
                if (node.Depth > 1)
                {
                    var parentNode = node.Parent;
                    parentNode.Children.Remove(node);
                    if (parentNode.Children.Count == 0)
                    {
                        treeView.Collapse(parentNode.Parent);
                        await Task.Delay(100);
                        treeView.Expand(parentNode.Parent);
                    }
                }
                else
                {
                    if (node.Depth == 1)
                        node.Parent.Children.Remove(node);
                    else
                        treeView.RootNodes.Remove(node);
                    await ReloadFoldersAsync();
                }
                DeselectAll();
            }

        }

        private async void TreeViewItemMenuFlyout_ImportImages(object sender, RoutedEventArgs e)
        {
            await FolderManagerService.PickAndImportImagesToFolder(GetSelectedFolder());
        }

        #endregion

        #region TREE VIEW EVENT HANDLERS

        private void Grid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (ItemInvokedWithThisClick)
            {
                ItemInvokedWithThisClick = false;
                return;
            }

            DeselectAll();
        }

        private void TreeViewItem_Invoked(WinUI.TreeView sender, WinUI.TreeViewItemInvokedEventArgs args)
        {
            ItemInvokedWithThisClick = true;
            SelectedItem = args.InvokedItem as FolderItem;
            var cont = treeView.ContainerFromItem(SelectedItem);
            SelectedNode = treeView.NodeFromContainer(cont);
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
                    await ReloadFoldersAsync();
                }
                else
                {
                    treeView.Collapse(node.Parent);
                    await Task.Delay(50);
                    treeView.Expand(node.Parent);
                }
            }
        }

        private void TreeViewItem_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var data = (sender as WinUI.TreeViewItem).DataContext;
            if (typeof(FolderItem).IsAssignableFrom(data.GetType()))
            {
                ItemInvokedWithThisClick = true;
                ItemSelected?.Invoke(this, new TreeViewItemSelectedEventArgs(SelectedItem));
            }
        }

        private void TreeViewItem_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            e.DragUIOverride.Caption = "Add";
            e.DragUIOverride.IsCaptionVisible = true;
            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = false;
            IsDragOverItem = true;
        }

        private void TreeView_DragOver(object sender, DragEventArgs e)
        {
            if (IsDragOverItem)
            {
                e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                e.DragUIOverride.Caption = "Add";
                e.DragUIOverride.IsCaptionVisible = true;
                e.DragUIOverride.IsContentVisible = true;
                e.DragUIOverride.IsGlyphVisible = false;
            }

            IsDragOverItem = false;
        }

        private async void TreeViewItem_Drop(object sender, DragEventArgs e)
        {
            foreach (var item in DragAndDropHelper.DraggedItems)
            {
                if (item is ImageItem imageItem)
                {
                    await DatabaseAccessService.MoveImageToVirtualfolderAsync(imageItem.DatabaseId, ((sender as WinUI.TreeViewItem).DataContext as FolderItem).DatabaseId);
                }
            }
        }

        private void TreeViewItem_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            SelectedItem = (sender as WinUI.TreeViewItem).DataContext as FolderItem;
            var cont = treeView.ContainerFromItem(SelectedItem);
            SelectedNode = treeView.NodeFromContainer(cont);
        }

        #endregion

    }
}
