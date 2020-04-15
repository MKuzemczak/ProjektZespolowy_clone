using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.Toolkit.Uwp.UI.Animations;

using Piceon.Models;
using Piceon.Helpers;
using Piceon.Services;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using System.Collections.Generic;
using System.IO;


namespace Piceon.Views
{
    public sealed partial class ImageGalleryPage : Page, INotifyPropertyChanged
    {
        public const string ImageGallerySelectedIdKey = "ImageGallerySelectedIdKey";

        public ObservableCollection<ImageItem> Source { get; } = new ObservableCollection<ImageItem>();
        public FolderItem SelectedContentFolder { get; set; } = null;

        public ImageGalleryPage()
        {
            InitializeComponent();
            Loaded += ImageGalleryPage_OnLoaded;
        }

        private async void ImageGalleryPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Source.Clear();

            if (SelectedContentFolder != null)
            {
                var data = await ImageLoaderService.GetImageGalleryDataAsync(SelectedContentFolder);

                if (data != null)
                {
                    imagesGridView.ItemsSource = data;
                }
            }
        }

        public async void AccessDirectory(FolderItem folder)
        {
            SelectedContentFolder = folder;
            
            var data = await ImageLoaderService.GetImageGalleryDataAsync(SelectedContentFolder);

            if (data != null)
            {
                imagesGridView.ItemsSource = data;
            }
        }

        private void ImagesGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selected = e.ClickedItem as ImageItem;
            if (selected != null)
            {
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(selected);
                ImageNavigationHelper.ContainingDataSource = imagesGridView.ItemsSource as ImageDataSource;
                ImageNavigationHelper.ContainingFolder = SelectedContentFolder;
                ImageNavigationHelper.SelectedImage = selected;

                // to test new flip view, change here to:
                NavigationService.Navigate<ImageDetailPage>(selected);
                // NavigationService.Navigate<ImageGalleryDetailPage>(selected);
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

        private void ShareImage_Click(object sender, RoutedEventArgs e)
        {

        }

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

                }
            }
            else
            {
                // The user clicked the CLoseButton, pressed ESC, Gamepad B, or the system back button.
                // Do nothing.
            }
        }

        private void RenameImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
