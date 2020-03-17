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

        public static async Task<ImageDataSource> GetImageGalleryDataAsync(StorageFolder folder)
        {
            if (PreviouslyAccessedFolder == folder && previouslyExtractedDataSource != null)
            {
                return previouslyExtractedDataSource;
            }

            ImageDataSource ds = await ImageDataSource.GetDataSource(folder.Path);
            previouslyExtractedDataSource = ds;
            PreviouslyAccessedFolder = folder;
            return ds;
        }
    }
}
