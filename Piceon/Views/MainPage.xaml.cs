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
using Windows.UI.Core.Preview;
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
        
        public ObservableCollection<FolderItem> Directories { get; } = new ObservableCollection<FolderItem>();


        public MainPage()
        {
            InitializeComponent();

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += App_CloseRequested;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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

        private void TreeViewPage_ItemSelected(object sender, TreeViewItemSelectedEventArgs e)
        {
            imageGalleryPage.AccessFolder(e.Parameter);
        }

        private async void imageGalleryPage_ImageClicked(object sender, EventArgs e)
        {
            await imageDetailPage.ShowAsync();
        }

        private void imageGalleryPage_AccessedFolderContetsChanged(object sender, EventArgs e)
        {

        }

        private void App_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            BackendConctroller.SendCloseApp();
        }
    }
}
