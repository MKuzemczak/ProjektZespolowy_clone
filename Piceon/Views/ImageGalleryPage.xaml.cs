using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.UI.Animations;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Piceon.Models;
using Piceon.Helpers;
using Piceon.Services;


namespace Piceon.Views
{
    public sealed partial class ImageGalleryPage : Page, INotifyPropertyChanged
    {
        public const string ImageGallerySelectedIdKey = "ImageGallerySelectedIdKey";

        public ImageDataSource Source { get; set; }
        public FolderItem SelectedContentFolder { get; set; } = null;

        // needed for marshaling calls back to UI thread
        private CoreDispatcher _uiThreadDispatcher;

        private bool IsItemClickedWithThisClick = false;
        private ImageItem ClickedItem { get; set; }

        public ImageGalleryPage()
        {
            InitializeComponent();
            Loaded += ImageGalleryPage_OnLoaded;

            _uiThreadDispatcher = Window.Current.Dispatcher;
        }

        private async void ImageGalleryPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedContentFolder != null)
            {
                SelectedContentFolder.ContentsChanged += SelectedContentFolder_ContentsChanged;

                Source = await ImageLoaderService.GetImageGalleryDataAsync(SelectedContentFolder);

                if (Source != null)
                {
                    imagesGridView.ItemsSource = Source;
                }
            }
        }


        public async void AccessDirectory(FolderItem folder)
        {
            if (SelectedContentFolder != folder)
            {
                if (SelectedContentFolder is object)
                    SelectedContentFolder.ContentsChanged -= SelectedContentFolder_ContentsChanged;
                SelectedContentFolder = folder;
                SelectedContentFolder.ContentsChanged += SelectedContentFolder_ContentsChanged;
            }
            
            Source = await ImageLoaderService.GetImageGalleryDataAsync(SelectedContentFolder);

            if (Source != null)
            {
                imagesGridView.ItemsSource = Source;
            }
        }

        public void ReloadFolder()
        {
            AccessDirectory(SelectedContentFolder);
        }

        private void SelectedContentFolder_ContentsChanged(object sender, EventArgs e)
        {
            // This callback can occur on a different thread so we need to marshal it back to the UI thread
            if (!_uiThreadDispatcher.HasThreadAccess)
            {
                var t = _uiThreadDispatcher.RunAsync(CoreDispatcherPriority.Normal, ReloadFolder);
            }
            else
            {
                ReloadFolder();
            }
        }

        private void ImagesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            ClickedItem = e.ClickedItem as ImageItem;
            IsItemClickedWithThisClick = true;
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

        private async void CopyImage_Click(object sender, RoutedEventArgs e)
        {
            StorageFile file = ((sender as MenuFlyoutItem).DataContext as ImageItem).File;
            List<StorageFile> storageFiles = new List<StorageFile>(1);
            storageFiles.Add(file);
            var dataPackage = new DataPackage();
            dataPackage.SetStorageItems(storageFiles);
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            try
            {
                Clipboard.SetContent(dataPackage);
            }
            catch (Exception)
            {
                var messageDialog = new MessageDialog("It is filed to copy this file");
                await messageDialog.ShowAsync();
            }
        }
        private async void DeleteImage_Click(object sender, RoutedEventArgs e)
        {
            var file = ((sender as MenuFlyoutItem).DataContext as ImageItem);
            ContentDialog deleteFileDialog = new ContentDialog
            {
                Title = "Delete Image",
                Content = "If you delete this file, you won't be able to recover it. Do you want to delete it?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteFileDialog.ShowAsync();
            // Delete the file if the user clicked the primary button.
            /// Otherwise, do nothing.
            if (result == ContentDialogResult.Primary)
            {
                try
                {
                    await file.File.DeleteAsync();
                }
                catch (Exception)
                {
                    var messageDialog = new MessageDialog("It is filed to delete this file");
                    await messageDialog.ShowAsync();
                }
            }
        }
        private void RenameImage_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            //TODO: change name of an image
        }

        public event EventHandler ImageClicked;

        private void ImagesGridView_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            if (ClickedItem != null)
            {
                ImageNavigationHelper.ContainingDataSource = imagesGridView.ItemsSource as ImageDataSource;
                ImageNavigationHelper.ContainingFolder = SelectedContentFolder;
                ImageNavigationHelper.SelectedImage = ClickedItem;
                ImageClicked?.Invoke(this, new EventArgs());
            }
        }

        private void ImagesGridView_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!IsItemClickedWithThisClick)
            {
                imagesGridView.SelectedItems.Clear();
            }

            IsItemClickedWithThisClick = false;
        }

        private void ImagesGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            DragAndDropHelper.DraggedItems.Clear();
            DragAndDropHelper.DraggedItems.AddRange(imagesGridView.SelectedItems);
        }
    }
}
