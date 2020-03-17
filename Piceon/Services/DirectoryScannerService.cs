using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

using Windows.Storage;

using Piceon.Models;

namespace Piceon.Services
{
    public static class DirectoryScannerService
    {
        public static async Task<DirectoryItem> GetLibraryFolderUnder(StorageFolder library)
        {
            DirectoryItem ret = new DirectoryItem()
            { Folder = library };

            IReadOnlyList<StorageFolder> folders = await library.GetFoldersAsync();

            foreach (StorageFolder f in folders)
            {
                ret.Subdirectories.Add(await GetLibraryFolderUnder(f));
            }

            return ret;
        }

        
    }
}
