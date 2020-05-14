using System;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

using Piceon.Helpers;
using Piceon.DatabaseAccess;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using System.IO;

namespace Piceon.Models
{

    public class ImageItem
    {
        public enum Options : int
        {
            None = 0,
            Image = 1,
            Thumbnail = 2
        }


        public string Filename { get; set; }
        public BitmapImage ImageData { get; set; }

        public int DatabaseId { get; set; }

        public string FilePath { get; set; }

        public DatabaseSimilaritygroup Group { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        // Needed for displaying single image in the flip view
        public StorageFile File { get; set; }

        public Options ViewMode { get; set; }

        public GroupPosition PositionInGroup { get; set; } = GroupPosition.None;

        private async Task SetStorageFileFromPathAsync(string path)
        {
            File = await StorageFile.GetFileFromPathAsync(path);
        }

        public async Task ToImageAsync(CancellationToken ct = new CancellationToken())
        {
            if (ViewMode == Options.Image)
                return;
            if (ImageData == null)
                ImageData = new BitmapImage();
            if (File == null)
                await SetStorageFileFromPathAsync(FilePath);
            using (Windows.Storage.Streams.IRandomAccessStream fileStream =
                await File.OpenAsync(FileAccessMode.Read))
            {
                await ImageData.SetSourceAsync(fileStream).AsTask(ct);
            }

            ViewMode = Options.Image;
        }

        public async Task ToThumbnailAsync(CancellationToken ct = new CancellationToken())
        {
            if (ViewMode == Options.Thumbnail)
                return;
            if (ImageData == null)
                ImageData = new BitmapImage();
            if (File == null)
                await SetStorageFileFromPathAsync(FilePath);
            ct.ThrowIfCancellationRequested();
            StorageItemThumbnail thumb = await File.GetThumbnailAsync(ThumbnailMode.SingleItem);
            ct.ThrowIfCancellationRequested();
            if (ImageData is object)
            {
                await ImageData.SetSourceAsync(thumb);
                ViewMode = Options.Thumbnail;
            }
        }

        public void ClearImageData()
        {
            ViewMode = Options.None;
            ImageData = null;
        }

        // Needed to ensure only one request is in progress at once
        private static SemaphoreSlim gettingFileProperties = new SemaphoreSlim(1);

        public static async Task<ImageItem> FromDatabaseImage(
            DatabaseImage dbimage, CancellationToken ct = new CancellationToken(), Options viewMode = Options.Image)
        {
            ImageItem result = new ImageItem()
            {
                DatabaseId = dbimage.Id,
                FilePath = dbimage.Path,
                Filename = Path.GetFileName(dbimage.Path),
                Group = dbimage.Group,
                Tags = dbimage.Tags,
                ViewMode = viewMode
            };
            ct.ThrowIfCancellationRequested();
            switch (viewMode)
            {
                case Options.Image:
                    await result.ToImageAsync(ct);
                    break;
                case Options.Thumbnail:
                    await result.ToThumbnailAsync(ct);
                    break;
                default:
                    break;
            }

            return result;
        }


        // Fetches all the data for the specified file
        public async static Task<ImageItem> FromStorageFile(
            StorageFile f, CancellationToken ct, Options options = Options.Image)
        {
            ImageItem item = new ImageItem()
            {
                Filename = f.Name,
                File = f,
                ViewMode = options
            };

            ct.ThrowIfCancellationRequested();

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

            item.ImageData = img;
            return item;
        }

        public async static Task<ImageItem> FromStorageFile(StorageFile f, Options options = Options.Image)
        {
            return await FromStorageFile(f, new CancellationToken(), options);
        }

        public async Task DeleteFromDiskAsync()
        {
            await File?.DeleteAsync();
        }
    }
}
