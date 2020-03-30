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
        public static FolderItem PreviouslyAccessedFolder = null;

        public static async Task<ImageDataSource> GetImageGalleryDataAsync(FolderItem folder)
        {
            //if (PreviouslyAccessedFolder.Name == folder && previouslyExtractedDataSource != null)
            //{
            //    return previouslyExtractedDataSource;
            //}

            ImageDataSource ds = await ImageDataSource.GetDataSource(folder);
            previouslyExtractedDataSource = ds;
            PreviouslyAccessedFolder = folder;
            return ds;
        }
    }
}
