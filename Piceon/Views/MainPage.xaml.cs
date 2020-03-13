using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Piceon.Models;
using Piceon.Services;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace Piceon.Views
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private object _selectedItem;

        public object SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }
        
        public ObservableCollection<DirectoryItem> Directories { get; } = new ObservableCollection<DirectoryItem>();


        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            DirectoryItem data = await DirectoryScannerService.GetLibraryFolderUnder(KnownFolders.PicturesLibrary);
            foreach (var item in data.Subdirectories)
            {
                Directories.Add(item);
            }
        }

        private void OnItemInvoked(WinUI.TreeView sender, WinUI.TreeViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem.GetType() == typeof(DirectoryItem))
            {
                SelectedItem = (args.InvokedItem as DirectoryItem).Folder.Path;
                imageGalleryPage.AccessDirectory(SelectedItem as string);
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
