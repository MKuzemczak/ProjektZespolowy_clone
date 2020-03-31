using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

using Piceon.DatabaseAccess;
using Windows.Storage.Search;

namespace Piceon.Models
{
    public class FolderItem
    {
        //public StorageFolder Folder { get; set; }
        public List<FolderItem> Subfolders
        {
            get
            {
                return GetSubfolders();
            }
        }

        public FolderItem ParentFolder
        {
            get
            {
                return GetParentFolder();
            }
        }

        public string Name { get; set; }

        private int DatabaseId { get; set; }

        public Func<int, int, Task<IReadOnlyList<StorageFile>>> GetStorageFilesRangeAsync { get; private set; }

        public Func<Task<int>> GetFilesCountAsync { get; private set; }

        private Func<List<FolderItem>> GetSubfolders { get; set; }

        private Func<FolderItem> GetParentFolder { get; set; }

        private StorageFolder SourceStorageFolder { get; set; }

        private StorageFileQueryResult SourceStorageFolderImageQuery { get; set; }

        private FolderItem() { }

        public static FolderItem FolderItemFromDatabaseVirtualFolder(DatabaseVirtualFolder virtualFolder)
        {
            FolderItem result = new FolderItem();

            result.GetStorageFilesRangeAsync = result.GetStorageFilesRangeFromDatabaseAsync;
            result.GetFilesCountAsync = result.GetFilesCountFromDatabaseAsync;
            result.GetSubfolders = result.GetSubfoldersFromDatabase;
            result.GetParentFolder = result.GetParentFolderFromDatabase;
            result.Name = virtualFolder.Name;
            result.DatabaseId = virtualFolder.Id;

            return result;
        }

        public static async Task<FolderItem> FolderItemFromStorageFolder(StorageFolder folder)
        {
            FolderItem result = new FolderItem();

            result.GetStorageFilesRangeAsync = result.GetStorageFilesRangeFromStorageFolderAsync;
            result.GetFilesCountAsync = result.GetFilesCountFromStorageFolderAsync;
            result.GetSubfolders = result.GetSubfoldersFromStorageFolder;
            result.GetParentFolder = result.GetParentFolderFromStorageFolder;
            result.SourceStorageFolder = folder;
            result.Name = folder.Name;

            List<string> fileTypeFilter = new List<string>
            {
                ".jpg", ".png", ".bmp", ".gif"
            };
            var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, fileTypeFilter);

            // Create query and retrieve files
            result.SourceStorageFolderImageQuery = result.SourceStorageFolder.CreateFileQueryWithOptions(queryOptions);

            return result;
        }


        private async Task<IReadOnlyList<StorageFile>> GetStorageFilesRangeFromDatabaseAsync(int firstIndex, int length)
        {
            var virtualFolders = DatabaseAccessService.GetImagesInFolder(DatabaseId);

            if (firstIndex + length > virtualFolders.Count)
                throw new IndexOutOfRangeException();

            var selectedRangeVirtualFolders = virtualFolders.GetRange(firstIndex, length);

            var result = new List<StorageFile>();

            foreach (var item in selectedRangeVirtualFolders)
            {
                result.Add(await StorageFile.GetFileFromPathAsync(item));
            }

            return result;
        }

        private async Task<IReadOnlyList<StorageFile>> GetStorageFilesRangeFromStorageFolderAsync(int firstIndex, int length)
        {
            if (SourceStorageFolder == null || SourceStorageFolderImageQuery == null)
            {
                throw new MemberAccessException();
            }

            return await SourceStorageFolderImageQuery.GetFilesAsync((uint)firstIndex, (uint)length);
        }

        private async Task<int> GetFilesCountFromDatabaseAsync()
        {
            return DatabaseAccessService.GetImagesCountInFolder(DatabaseId);
        }

        private async Task<int> GetFilesCountFromStorageFolderAsync()
        {
            if (SourceStorageFolder == null || SourceStorageFolderImageQuery == null)
            {
                throw new MemberAccessException();
            }

            return (int)await SourceStorageFolderImageQuery.GetItemCountAsync();
        }

        private List<FolderItem> GetSubfoldersFromDatabase()
        {
            var virtualFolders = DatabaseAccessService.GetChildrenOfFolder(DatabaseId);

            var result = new List<FolderItem>();

            foreach (var item in virtualFolders)
            {
                result.Add(FolderItemFromDatabaseVirtualFolder(item));
            }

            return result;
        }

        private List<FolderItem> GetSubfoldersFromStorageFolder()
        {
            if (SourceStorageFolder == null)
            {
                throw new MemberAccessException();
            }

            var subfolders = SourceStorageFolder.GetFoldersAsync().AsTask().GetAwaiter().GetResult();

            var result = new List<FolderItem>();

            foreach (var item in subfolders)
            {
                result.Add(FolderItemFromStorageFolder(item).GetAwaiter().GetResult());
            }

            return result;
        }

        private FolderItem GetParentFolderFromDatabase()
        {
            var virtualFolder = DatabaseAccessService.GetParentOfFolder(DatabaseId);

            return FolderItemFromDatabaseVirtualFolder(virtualFolder);
        }

        private FolderItem GetParentFolderFromStorageFolder()
        {
            if (SourceStorageFolder == null)
            {
                throw new MemberAccessException();
            }
            return FolderItemFromStorageFolder(SourceStorageFolder.GetParentAsync().AsTask().GetAwaiter().GetResult()).GetAwaiter().GetResult();
        }
    }
}
