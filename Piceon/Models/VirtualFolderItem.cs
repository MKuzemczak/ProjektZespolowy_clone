using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

using Piceon.DatabaseAccess;
using System.Threading;
using System.IO;

namespace Piceon.Models
{
    public class VirtualFolderItem : FolderItem
    {
        public const string NameInvalidCharacters = "\\/:*?\"<>|";

        public override event EventHandler ContentsChanged;

        public static async Task<VirtualFolderItem> FromDatabaseVirtualFolder(DatabaseVirtualFolder virtualFolder)
        {
            VirtualFolderItem result = new VirtualFolderItem
            {
                Name = virtualFolder.Name,
                DatabaseId = virtualFolder.Id
            };
            result.Subfolders = await result.GetSubfoldersAsync();

            return result;
        }

        public static async Task<VirtualFolderItem> GetNew(string name)
        {
            var dbvf = await DatabaseAccessService.InsertVirtualFolderAsync(name);

            return await FromDatabaseVirtualFolder(dbvf);
        }

        public override async Task<IReadOnlyList<StorageFile>> GetStorageFilesRangeAsync(int firstIndex, int length)
        {
            var allFilePaths = await DatabaseAccessService.GetImagesInFolderAsync(DatabaseId);

            if (firstIndex + length > allFilePaths.Count)
                throw new IndexOutOfRangeException();

            var selectedRangeFilePaths = allFilePaths.GetRange(firstIndex, length);

            var result = new List<StorageFile>();

            foreach (var item in selectedRangeFilePaths)
            {
                result.Add(await StorageFile.GetFileFromPathAsync(item.Item2));
            }

            return result;
        }

        public override async Task<IReadOnlyList<ImageItem>> GetImageItemsRangeAsync(int firstIndex, int length, CancellationToken ct = new CancellationToken())
        {
            ct.ThrowIfCancellationRequested();
            var allFiles = await DatabaseAccessService.GetImagesInFolderAsync(DatabaseId);

            if (firstIndex + length > allFiles.Count)
                throw new IndexOutOfRangeException();

            var selectedRangeFiles = allFiles.GetRange(firstIndex, length);

            var result = new List<ImageItem>();

            for (int i = 0; i < selectedRangeFiles.Count(); i++)
            {
                ct.ThrowIfCancellationRequested();
                var storageFile = await StorageFile.GetFileFromPathAsync(selectedRangeFiles[i].Item2);
                var image = await ImageItem.FromStorageFile(storageFile, i + firstIndex, ct, ImageItem.Options.Thumbnail);
                image.DatabaseId = selectedRangeFiles[i].Item1;
                result.Add(image);
            }

            return result;
        }

        public override async Task<int> GetFilesCountAsync()
        {
            return await DatabaseAccessService.GetImagesCountInFolderAsync(DatabaseId);
        }

        protected override async Task<List<FolderItem>> GetSubfoldersAsync()
        {
            var virtualFolders = await DatabaseAccessService.GetChildrenOfFolderAsync(DatabaseId);

            var result = new List<FolderItem>();

            foreach (var item in virtualFolders)
            {
                var newFolder = await FromDatabaseVirtualFolder(item);
                newFolder.ParentFolder = this;
                result.Add(newFolder);
            }

            return result;
        }

        public override async Task RenameAsync(string newName)
        {
            if (newName.IndexOfAny(NameInvalidCharacters.ToCharArray()) != -1)
            {
                throw new FormatException();
            }
            await DatabaseAccessService.RenameVirtualFolderAsync(DatabaseId, newName);
            Name = newName;
        }

        public override async Task SetParentAsync(FolderItem folder)
        {
            if (folder is VirtualFolderItem)
            {
                await DatabaseAccessService.SetParentOfFolderAsync(DatabaseId, (folder as VirtualFolderItem).DatabaseId);
                ParentFolder?.Subfolders?.Remove(this);
                ParentFolder = folder;
                folder.Subfolders.Add(this);
            }
        }

        public override async Task DeleteAsync()
        {
            if (Subfolders is object)
            {
                int subCount = Subfolders.Count;
                for (int i = subCount - 1; i >= 0; i--)
                {
                    await Subfolders[i].DeleteAsync();
                }
            }
            await DatabaseAccessService.DeleteVirtualFolderAsync(DatabaseId);
            ParentFolder?.Subfolders?.Remove(this);
            DatabaseId = -1;
            ParentFolder = null;
            Subfolders = null;
            Name = null;
        }

        public override async Task CheckContentAsync()
        {
            var dbFiles = await DatabaseAccessService.GetImagesInFolderAsync(DatabaseId);

            foreach (var file in dbFiles)
            {
                try
                {
                    StorageFile f = await StorageFile.GetFileFromPathAsync(file.Item2);
                }
                catch (FileNotFoundException)
                {
                    await DatabaseAccessService.DeleteImageAsync(file.Item1);
                }
            }
        }

        public override async Task AddFilesToFolder(IReadOnlyList<StorageFile> files)
        {
            foreach (var file in files)
            {
                await DatabaseAccessService.InsertImageAsync(file.Path, DatabaseId);
            }

            ContentsChanged?.Invoke(this, new EventArgs());
        }

        public static bool operator ==(VirtualFolderItem f1, VirtualFolderItem f2)
        {
            if ((f1 is object && f2 is null) ||
                (f1 is null && f2 is object))
                return false;

            if (f1 is null && f2 is null)
                return true;

            return (f1.Name == f2.Name &&
                f1.DatabaseId == f2.DatabaseId);
        }

        public static bool operator !=(VirtualFolderItem f1, VirtualFolderItem f2)
        {
            if ((f1 is object && f2 is null) ||
                (f1 is null && f2 is object))
                return true;

            if (f1 is null && f2 is null)
                return false;

            return !(f1.Name == f2.Name &&
                f1.DatabaseId == f2.DatabaseId);
        }

        public override bool Equals(object obj)
        {
            return obj is VirtualFolderItem item &&
                   Name == item.Name &&
                   DatabaseId == item.DatabaseId;
        }

        public override int GetHashCode()
        {
            int hashCode = 1010871291;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + DatabaseId.GetHashCode();
            return hashCode;
        }
    }
}
