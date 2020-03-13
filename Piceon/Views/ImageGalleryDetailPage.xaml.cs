using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Toolkit.Uwp.UI.Animations;

using Piceon.Models;
using Piceon.Services;
using Piceon.Helpers;

using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;

namespace Piceon.Views
{
    public sealed partial class ImageGalleryDetailPage : Page, INotifyPropertyChanged
    {
        private object _selectedImage;

        public object SelectedImage
        {
            get => _selectedImage;
            set
            {
                Set(ref _selectedImage, value);
                //ImagesNavigationHelper.UpdateImageId(
                //    ImageGalleryPage.ImageGallerySelectedIdKey, ((ImageItem)SelectedImage).Key);
            }
        }

        public ObservableCollection<ImageItem> Source { get; } = new ObservableCollection<ImageItem>();

        private ItemIndexRange _currentTrackedItemsRange;

        public ItemIndexRange CurrentTrackedItemsRange
        {
            get => _currentTrackedItemsRange;
            set
            {
                Set(ref _currentTrackedItemsRange, value);
            }
        }

        public ImageGalleryDetailPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Source.Clear();

            // TODO WTS: Replace this with your actual data
            ImageDataSource data =
                await ImageLoaderService.GetImageGalleryDataAsync(ImageLoaderService.PreviouslyAccessedFolder);

            if (data != null)
            {
                flipView.ItemsSource = data;
            }

            ImageItem parameter = e.Parameter as ImageItem;

            SelectedImage = parameter;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
            {
                NavigationService.Frame.SetListDataItemForNextConnectedAnimation(SelectedImage);
                ImagesNavigationHelper.RemoveImageId(ImageGalleryPage.ImageGallerySelectedIdKey);
            }
        }

        private void OnPageKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Escape && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
                e.Handled = true;
            }
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
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

        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        // Makes sure that only one FlipView_SelectionChanged is running at a time.
        // Otherwise, during fast flipping, some unexpected behavior might occur as the selection is changing rapidly
        private SemaphoreSlim FlipView_SelectionChanging = new SemaphoreSlim(1);

        private async void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (SelectedImage == null)
            //{
            //    var list = e.AddedItems.Count;
            //    return;
            //}

            await FlipView_SelectionChanging.WaitAsync();

            if (e.RemovedItems != null && e.RemovedItems.Count == 1 && e.RemovedItems[0] != null)
            {
                try
                {
                    await (e.RemovedItems[0] as ImageItem).ToThumbnail();

                }
                catch (System.Threading.Tasks.TaskCanceledException exception)
                {
                    Console.WriteLine("Exception in FlipView_SelectionChanged: " + exception.Message);
                }
            }

            if (e.AddedItems != null && e.AddedItems.Count == 1 && e.AddedItems[0] != null)
            {
                try
                {
                    await (e.AddedItems[0] as ImageItem).ToImage();
                }
                catch (System.Threading.Tasks.TaskCanceledException exception)
                {
                    Console.WriteLine("Exception in FlipView_SelectionChanged: " + exception.Message);
                }


                var it = (sender as FlipView).Items.IndexOf(e.AddedItems[0]);
                var count = (sender as FlipView).Items.Count;
                var halfRange = 3;
                UpdateFlipViewRanges(
                    (sender as FlipView).Items.IndexOf(e.AddedItems[0]), (sender as FlipView).Items.Count, halfRange);
            }

            FlipView_SelectionChanging.Release();
        }

        private void UpdateFlipViewRanges(int currentIndex, int cnt, int hlfRng)
        {
            int firstIndex = Math.Max(0, currentIndex - hlfRng);
            int length = hlfRng + Math.Min(cnt - 1, currentIndex + hlfRng) - currentIndex;
            CurrentTrackedItemsRange = new ItemIndexRange(firstIndex, (uint)length);
            (flipView.ItemsSource as ImageDataSource).RangesChanged(
                new ItemIndexRange(currentIndex, 1), new List<ItemIndexRange>() { CurrentTrackedItemsRange });
        }
    }
}
