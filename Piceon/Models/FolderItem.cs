using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.System;

using Piceon.DatabaseAccess;
using Windows.Storage.Search;
using Windows.UI.Popups;

namespace Piceon.Models
{
    public abstract class FolderItem
    {
        public List<FolderItem> Subfolders { get; protected set; } = new List<FolderItem>();

        public FolderItem ParentFolder { get; protected set; }

        public string Name { get; set; }

        public abstract Task<IReadOnlyList<StorageFile>> GetStorageFilesRangeAsync(int firstIndex, int length);

        public abstract Task<int> GetFilesCountAsync();

        public abstract Task RenameAsync(string newName);

        public  abstract Task SetParentAsync(FolderItem parent);

        public abstract Task DeleteAsync();

        protected abstract Task<List<FolderItem>> GetSubfoldersAsync();

        protected FolderItem() { }

        // TODO: delete dis
        //public static FolderItem FolderItemFromDatabaseVirtualFolder(DatabaseVirtualFolder virtualFolder)
        //{
        //    FolderItem result = new FolderItem();

        //    result.GetStorageFilesRangeAsync = result.GetStorageFilesRangeFromDatabaseAsync;
        //    result.GetFilesCountAsync = result.GetFilesCountFromDatabaseAsync;
        //    result.Name = virtualFolder.Name;
        //    result.DatabaseId = virtualFolder.Id;
        //    result.RenameAsync = result.RenameDatabaseVirtualFolderAsync;
        //    result.SetParentAsync = result.SetParentDatabase;
        //    result.DeleteAsync = result.DeleteFromDatabaseAsync;
        //    result.Subfolders = result.GetSubfoldersFromDatabase();

        //    return result;
        //}

        //public static async Task<FolderItem> FolderItemFromStorageFolder(StorageFolder folder)
        //{
        //    FolderItem result = new FolderItem();

        //    result.GetStorageFilesRangeAsync = result.GetStorageFilesRangeFromStorageFolderAsync;
        //    result.GetFilesCountAsync = result.GetFilesCountFromStorageFolderAsync;
        //    result.SourceStorageFolder = folder;
        //    result.Name = folder.Name;
        //    result.RenameAsync = result.RenameStorageFolderAsync;
        //    result.SetParentAsync = result.SetParentStorageAsync;
        //    result.DeleteAsync = result.DeleteStorageAsync;
        //    result.Subfolders = await result.GetSubfoldersFromStorageFolder();

        //    List<string> fileTypeFilter = new List<string>
        //    {
        //        ".jpg", ".png", ".bmp", ".gif"
        //    };
        //    var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);

        //    // Create query and retrieve files
        //    result.SourceStorageFolderImageQuery = result.SourceStorageFolder.CreateFileQueryWithOptions(queryOptions);

        //    return result;
        //}

    }
}
