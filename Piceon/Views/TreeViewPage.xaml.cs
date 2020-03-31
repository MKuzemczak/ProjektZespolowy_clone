using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Piceon.DatabaseAccess;
using Piceon.Models;
using Piceon.Services;

using Windows.Storage;
using Windows.Storage.Pickers;
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
        private object _selectedItem;

        public object SelectedItem
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
            var virtualFoldersRootNodes = DatabaseAccessService.GetRootVirtualFolders();

            foreach (var item in virtualFoldersRootNodes)
            {
                Directories.Add(FolderItem.FolderItemFromDatabaseVirtualFolder(item));
            }

            var saveFolder = (await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures)).SaveFolder;

            Directories.Add(await FolderItem.FolderItemFromStorageFolder(saveFolder));

            // wait for treeview to load data
            if (Directories.Count == 1)
            {
                await Task.Delay(500);
                treeView.Expand(treeView.RootNodes[0]);
            }
        }


        public event EventHandler<TreeViewItemSelectedEventArgs> ItemSelected;

        private void OnItemInvoked(WinUI.TreeView sender, WinUI.TreeViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem.GetType() == typeof(FolderItem))
            {
                SelectedItem = args.InvokedItem as FolderItem;
                ItemSelected?.Invoke(this, new TreeViewItemSelectedEventArgs(SelectedItem as FolderItem));
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

    }
}
