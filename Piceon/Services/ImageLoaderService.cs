using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Search;

using Piceon.Models;

namespace Piceon.Services
{
    public static class ImageLoaderService
    {
        private static ImageDataSource previouslyExtractedDataSource = null;
        public static StorageFolder PreviouslyAccessedFolder = null;
        //async void initdata()
        //{
        //    StorageLibrary pictures = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
        //    string path = pictures.SaveFolder.Path;

        //    ImageDataSource ds = await ImageDataSource.GetDataSoure(path);
        //    if (ds.Count > 0)
        //    {
        //        Grid1.ItemsSource = ds;
        //    }
        //    else
        //    {
        //        MainPage.Current.NotifyUser("Error: The pictures folder doesn't contain any files", NotifyType.ErrorMessage);
        //    }
        //}

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Safety", "UWP003:UWP-only", Justification = "<Pending>")]
        public static async Task<ImageDataSource> GetImageGalleryDataAsync(StorageFolder folder)
        {
            if (PreviouslyAccessedFolder == folder && previouslyExtractedDataSource != null)
            {
                return previouslyExtractedDataSource;
            }

            StorageLibrary pictures = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            string saveFolderPath = pictures.SaveFolder.Path;

            ImageDataSource ds = await ImageDataSource.GetDataSoure(folder.Path);
            previouslyExtractedDataSource = ds;
            PreviouslyAccessedFolder = folder;
            return ds;
            //else
            //{
            //    MainPage.Current.NotifyUser("Error: The pictures folder doesn't contain any files", NotifyType.ErrorMessage);
            //}


            //var ret = new List<ImageItem>();

            //int cntr = 0;

            //Windows.Storage.StorageFolder picturesFolder =
            //    Windows.Storage.KnownFolders.PicturesLibrary;
            //IReadOnlyList<StorageFolder> folders =
            //    await picturesFolder.GetFoldersAsync(CommonFolderQuery.GroupByType);

            //// Process file folders
            //foreach (StorageFolder folder in folders)
            //{
            //    // Get and process files in folder
            //    IReadOnlyList<StorageFile> fileList = await folder.GetFilesAsync();
            //    foreach (StorageFile file in fileList)
            //    {
            //        // Open a stream for the selected file.
            //        // The 'using' block ensures the stream is disposed
            //        // after the image is loaded.
            //        using (Windows.Storage.Streams.IRandomAccessStream fileStream =
            //            await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            //        {
            //            // Set the image source to the selected bitmap.
            //            Windows.UI.Xaml.Media.Imaging.BitmapImage bitmapImage =
            //                new Windows.UI.Xaml.Media.Imaging.BitmapImage();
            //            bitmapImage.SetSource(fileStream);


            //            ret.Add(new ImageItem()
            //            {
            //                ID = $"{cntr++}",
            //                Name = file.Name,
            //                Source = bitmapImage
            //            });
            //        }
            //    }
            //}

            //foreach (var file in files)
            //{
            //    using (var fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            //    {
            //        if (fileStream.CanRead)
            //        {
            //            var bitmapImage = new BitmapImage();
            //            bitmapImage.SetSource(fileStream);

            //            ret.Add(new ImageItem()
            //            {
            //                ID = $"{cntr++}",
            //                Name = file.Name,
            //                Source = bitmapImage
            //            });
            //        }
            //    }
            //}


            //foreach (string f in Directory.GetFiles(path))
            //{
            //    ret.Add(new ImageItem()
            //    {
            //        ID = $"{cntr}",
            //        Source = f,
            //        Name = f.Split('\\').Last()
            //    });
            //}

            //await Task.CompletedTask;

            //return ret;
        }
    }
}
