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
        public StorageFolder SelectedContentDirectory { get; set; }

        public ImageGalleryPage()
        {
            InitializeComponent();
            SelectedContentDirectory
                = StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures).AsTask().GetAwaiter().GetResult().SaveFolder;
            Loaded += ImageGalleryPage_OnLoaded;
        }

        private async void ImageGalleryPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Source.Clear();

            var data = await ImageLoaderService.GetImageGalleryDataAsync(SelectedContentDirectory);

            //foreach (var item in data)
            //{
            //    Source.Add(item);
            //}

            if (data != null)
            {
                imagesGridView.ItemsSource = data;
            }
        }

        public async void AccessDirectory(StorageFolder path)
        {
            SelectedContentDirectory = path;
            
            var data = await ImageLoaderService.GetImageGalleryDataAsync(SelectedContentDirectory);

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
                ImagesNavigationHelper.AddImageId(ImageGallerySelectedIdKey, selected.Key);
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(selected);
                NavigationService.Navigate<ImageGalleryDetailPage>(selected);
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
