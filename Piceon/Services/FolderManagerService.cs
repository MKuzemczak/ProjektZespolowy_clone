using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piceon.DatabaseAccess;
using Piceon.Models;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Piceon.Services
{
    public static class FolderManagerService
    {
        public static async Task<FolderItem> OpenFolderAsync()
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            string token = StorageApplicationPermissions.FutureAccessList.Add(folder);
            int tokenId = await DatabaseAccessService.InsertAccessedFolderAsync(token);
            return await StorageFolderItem.FromStorageFolderAsync(folder, tokenId);
        }

        public static async Task<List<FolderItem>> GetAllFolders()
        {
            var result = new List<FolderItem>();

            var virtualFoldersRootNodes = await DatabaseAccessService.GetRootVirtualFoldersAsync();

            foreach (var item in virtualFoldersRootNodes)
            {
                result.Add(await VirtualFolderItem.FromDatabaseVirtualFolder(item));
            }

            var tokenTupleList = await DatabaseAccessService.GetAccessedFoldersAsync();

            foreach (var tokenTuple in tokenTupleList)
            {
                var storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tokenTuple.Item2);
                result.Add(await StorageFolderItem.FromStorageFolderAsync(storageFolder, tokenTuple.Item1));
            }

            return result;
        }
    }
}
