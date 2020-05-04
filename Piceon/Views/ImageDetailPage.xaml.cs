using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

using Piceon.Models;
using Piceon.Services;
using Piceon.Helpers;

namespace Piceon.Views
{
    public sealed partial class ImageDetailPage : Page
    {
        private int CurrentIndexInFolder = -1;
        private ImageItem CurrentlyDisplayedImageItem { get; set; }
        private CancellationTokenSource FlipCancellationTokenSource;
        private CancellationToken FlipCancellationToken;

        public ImageDetailPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        public async Task ShowAsync()
        {
            this.Visibility = Visibility.Visible;
            CurrentIndexInFolder = ImageNavigationHelper.SelectedImage.GalleryIndex;
            CurrentlyDisplayedImageItem = ImageNavigationHelper.SelectedImage;
            UpdateArrowsVisibility();
            await CurrentlyDisplayedImageItem.ToImage();
            displayedImage.Source = CurrentlyDisplayedImageItem.ImageData;
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private async Task UpdateImageToIndex(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            var list = await ImageNavigationHelper.ContainingFolder.GetStorageFilesRangeAsync(CurrentIndexInFolder, 1);

            if (list.Count == 0)
            {
                return;
            }

            if (ct.IsCancellationRequested)
                return;

            await CurrentlyDisplayedImageItem.ToThumbnail();

            if (ct.IsCancellationRequested)
                return;

            try
            {
                CurrentlyDisplayedImageItem = await ImageItem.FromStorageFile(list[0], CurrentIndexInFolder, ct, ImageItem.Options.Thumbnail);
            }
            catch(TaskCanceledException)
            {
                return;
            }

            if (ct.IsCancellationRequested)
                return;

            displayedImage.Source = CurrentlyDisplayedImageItem.ImageData;

            if (ct.IsCancellationRequested)
                return;

            try
            {
                await CurrentlyDisplayedImageItem.ToImage(ct);
            }
            catch(TaskCanceledException)
            {
                return;
            }
        }

        private void UpdateArrowsVisibility()
        {
            if (CurrentIndexInFolder == 0)
            {
                previousArrow.Visibility = Visibility.Collapsed;
            }
            else
            {
                previousArrow.Visibility = Visibility.Visible;
            }

            if (CurrentIndexInFolder == ImageNavigationHelper.ContainingFolder?.GetFilesCount() - 1)
            {
                nextArrow.Visibility = Visibility.Collapsed;
            }
            else
            {
                nextArrow.Visibility = Visibility.Visible;
            }
        }

        private async void Previous_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentIndexInFolder == 0)
            {
                return;
            }

            CurrentIndexInFolder--;

            FlipCancellationTokenSource?.Cancel();

            FlipCancellationTokenSource = new CancellationTokenSource();
            FlipCancellationToken = FlipCancellationTokenSource.Token;
            UpdateArrowsVisibility();
            await UpdateImageToIndex(FlipCancellationToken);
        }

        private async void Next_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (CurrentIndexInFolder == ImageNavigationHelper.ContainingFolder.GetFilesCount() - 1)
            {
                return;
            }

            CurrentIndexInFolder++;

            FlipCancellationTokenSource?.Cancel();

            FlipCancellationTokenSource = new CancellationTokenSource();
            FlipCancellationToken = FlipCancellationTokenSource.Token;
            UpdateArrowsVisibility();
            await UpdateImageToIndex(FlipCancellationToken);
        }

    }
}
