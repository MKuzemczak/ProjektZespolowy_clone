using System;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

using Piceon.Helpers;

namespace Piceon.Models
{
    
    public class ImageItem
    {
        public enum Options : int
        {
            Image = 0,
            Thumbnail = 1
        }


        public string Filename { get; set; }
        public BitmapImage ImageData { get; set; }

        public string Key { get; private set; }

        public int GalleryIndex { get; set; }

        public int DatabaseId { get; set; }

        // Needed for displaying single image in the flip view
        public StorageFile File { get; set; }

        public Options ViewMode;

        public GroupPosition PotitionInGroup { get; set; } = GroupPosition.None;

        public async Task ToImage()
        {
            if (File == null)
                return;
            if (ImageData == null)
                ImageData = new BitmapImage();
            using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                await File.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                await ImageData.SetSourceAsync(fileStream);
            }

            ViewMode = Options.Image;
        }

        public async Task ToThumbnail()
        {
            if (File == null)
                return;
            if (ImageData == null)
                ImageData = new BitmapImage();
            StorageItemThumbnail thumb = await File.GetThumbnailAsync(ThumbnailMode.SingleItem);
            await ImageData.SetSourceAsync(thumb);
            ViewMode = Options.Thumbnail;
        }

        // Needed to ensure only one request is in progress at once
        private static SemaphoreSlim gettingFileProperties = new SemaphoreSlim(1);

        // Fetches all the data for the specified file
        public async static Task<ImageItem> FromStorageFile(
            StorageFile f, int index, CancellationToken ct, Options options = Options.Image)
        {
            ImageItem item = new ImageItem()
            {
                Filename = f.Name,
                File = f
            };

            ct.ThrowIfCancellationRequested();

            item.Key = f.FolderRelativeId;
            item.GalleryIndex = index;

            BitmapImage img = new BitmapImage();

            if (options == Options.Image)
            {
                using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                await f.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    img.SetSource(fileStream);
                }
            }
            else if (options == Options.Thumbnail)
            {
                StorageItemThumbnail thumb = await f.GetThumbnailAsync(ThumbnailMode.SingleItem).AsTask(ct);
                ct.ThrowIfCancellationRequested();
                await img.SetSourceAsync(thumb).AsTask(ct);
            }

            item.ViewMode = options;
            item.ImageData = img;
            return item;
        }

        public async static Task<ImageItem> FromStorageFile(StorageFile f, int index, Options options = Options.Image)
        {
            return await FromStorageFile(f, index, new CancellationToken(), options);
        }

        public async Task DeleteFromDiskAsync()
        {
            await File?.DeleteAsync();
        }
    }
}
