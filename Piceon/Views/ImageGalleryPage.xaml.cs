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
    }
}
