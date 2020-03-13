using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using Piceon.Core.Models;

namespace Piceon.Core.Services
{
    public static class DirectoryScannerService
    {
        public static async Task<DirectoryItem> GetDirectoryTreeUnder(string parentDir)
        {
            // var library await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);

            DirectoryItem parentDirectory = new DirectoryItem
            {
                Name = parentDir.Split('\\').Last(),
                Path = parentDir,
                Subdirectories = new List<DirectoryItem>()
            };

            string[] directories = Directory.GetDirectories(parentDir);

            foreach(string dir in directories)
            {
                parentDirectory.Subdirectories.Add(GetDirectoryTreeUnder(dir).Result);
            }

            await Task.CompletedTask;
            return parentDirectory;
        }
    }
}
