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
    public class FolderItem
    {
        public const string NameInvalidCharacters = "\\/:*?\"<>|";

        public int DatabaseId;

        public List<FolderItem> Subfolders { get; protected set; } = new List<FolderItem>();

        public FolderItem ParentFolder { get; protected set; }

        public string Name { get; set; }

        public List<string> TagsToFilter { get; protected set; } = new List<string>();

        public event EventHandler ContentsChanged;

        private List<ImageItem> AllImages { get; set; } = new List<ImageItem>();
        private List<ImageItem> FilteredImages { get; set; } = new List<ImageItem>();

        public static async Task<FolderItem> FromDatabaseVirtualFolder(DatabaseVirtualFolder virtualFolder)
        {
            FolderItem result = new FolderItem
            {
                Name = virtualFolder.Name,
                DatabaseId = virtualFolder.Id
            };
            result.Subfolders = await result.GetSubfoldersAsync();
            await result.UpdateQueryAsync();

            return result;
        }

        public static async Task<FolderItem> GetNew(string name)
        {
            var dbvf = await DatabaseAccessService.InsertVirtualFolderAsync(name);

            return await FromDatabaseVirtualFolder(dbvf);
        }

        public async Task<IReadOnlyList<StorageFile>> GetStorageFilesRangeAsync(int firstIndex, int length)
        {
            var result = new List<StorageFile>();

            var range = FilteredImages.GetRange(firstIndex, length);

            foreach (var item in range)
            {
                try
                {
                    result.Add(await StorageFile.GetFileFromPathAsync(item.FilePath));
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
            }

            return result;
        }

        public List<ImageItem> GetRawImageItems()
        {
            var result = new List<ImageItem>();
            result.AddRange(FilteredImages);
            return result;
        }

        //public async Task<IReadOnlyList<ImageItem>> GetImageItemsRangeAsync(int firstIndex, int length, CancellationToken ct = new CancellationToken())
        //{
        //    var result = new List<ImageItem>();
        //    ct.ThrowIfCancellationRequested();

        //    var selectedRangeFiles = FilteredImages.GetRange(firstIndex, length);

        //    for (int i = 0; i < selectedRangeFiles.Count(); i++)
        //    {
        //        ct.ThrowIfCancellationRequested();
        //        StorageFile storageFile = null;
        //        try
        //        {
        //            storageFile = await StorageFile.GetFileFromPathAsync(selectedRangeFiles[i].Path);
        //        }
        //        catch (FileNotFoundException)
        //        {
        //            continue;
        //        }
        //        storageFiles.Add(new Tuple<int, StorageFile>(i, storageFile));
        //    }

        //    int prevGroupId = -1;
        //    if (firstIndex != 0)
        //        prevGroupId = FilteredImages[firstIndex - 1].Group.Id;

        //    for (int i = 0; i < storageFiles.Count(); i++)
        //    {
        //        ct.ThrowIfCancellationRequested();
        //        int currentGroupId = selectedRangeFiles[storageFiles[i].Item1].Group.Id;
        //        var image = await ImageItem.FromStorageFile(storageFiles[i].Item2, storageFiles[i].Item1 + firstIndex, ct, ImageItem.Options.Thumbnail);
        //        image.DatabaseId = selectedRangeFiles[storageFiles[i].Item1].Id;

        //        bool nextGroupDifferent = ((firstIndex + storageFiles[i].Item1) == FilteredImages.Count - 1 ||
        //            currentGroupId != FilteredImages[firstIndex + storageFiles[i].Item1 + 1].Group.Id);

        //        if (currentGroupId < 0)
        //        {
        //            image.PotitionInGroup = Helpers.GroupPosition.None;
        //        }
        //        else if (prevGroupId != currentGroupId)
        //        {
        //            if (nextGroupDifferent)
        //            {
        //                image.PotitionInGroup = Helpers.GroupPosition.Only;
        //            }
        //            else
        //            {
        //                image.PotitionInGroup = Helpers.GroupPosition.Start;
        //            }
        //        }
        //        else if (nextGroupDifferent)
        //        {
        //            image.PotitionInGroup = Helpers.GroupPosition.End;
        //        }
        //        else
        //        {
        //            image.PotitionInGroup = Helpers.GroupPosition.Middle;
        //        }

        //        prevGroupId = currentGroupId;

        //        result.Add(image);
        //    }

        //    return result;
        //}

        public int GetFilesCount()
        {
            return FilteredImages.Count;
        }

        protected async Task<List<FolderItem>> GetSubfoldersAsync()
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

        public async Task RenameAsync(string newName)
        {
            if (newName.IndexOfAny(NameInvalidCharacters.ToCharArray()) != -1)
            {
                throw new FormatException();
            }
            await DatabaseAccessService.RenameVirtualFolderAsync(DatabaseId, newName);
            Name = newName;
        }

        public async Task SetParentAsync(FolderItem folder)
        {
            if (folder is FolderItem)
            {
                await DatabaseAccessService.SetParentOfFolderAsync(DatabaseId, (folder as FolderItem).DatabaseId);
                ParentFolder?.Subfolders?.Remove(this);
                ParentFolder = folder;
                folder.Subfolders.Add(this);
            }
        }

        public async Task DeleteAsync()
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

        public async Task CheckContentAsync()
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

        public async Task UpdateQueryAsync()
        {
            var raw = await DatabaseAccessService.GetVirtualfolderImagesWithGroupsAndTags(DatabaseId);
            var rawGrouped = raw.OrderByDescending(i => i.Group.Id).ToList();

            foreach (var item in AllImages)
            {
                item.ClearImageData();
            }

            AllImages.Clear();

            foreach (var item in rawGrouped)
            {
                AllImages.Add(await ImageItem.FromDatabaseImage(item, viewMode: ImageItem.Options.None));
            }

            FilteredImages.Clear();

            if (TagsToFilter is null || TagsToFilter.Count == 0)
            {
                FilteredImages.AddRange(AllImages);
            }
            else
            {
                FilteredImages = AllImages.
                Where(i => TagsToFilter.Intersect(i.Tags).Count() == TagsToFilter.Count).ToList();
                int prevGroupId = -1;
                int nextGroupId = -1;
                for (int i = 0; i < AllImages.Count; i++)
                {
                    if (TagsToFilter.Intersect(AllImages[i].Tags).Count() == TagsToFilter.Count)
                    {
                        FilteredImages.Add(AllImages[i]);

                        if (FilteredImages.Last().Group is null ||
                            FilteredImages.Last().Group.Id < 0)
                        {
                            FilteredImages.Last().PositionInGroup = Helpers.GroupPosition.None;
                            continue;
                        }

                        int currentGroupId = FilteredImages.Last().Group.Id;

                        if (i + 1 == AllImages.Count)
                            nextGroupId = -1;
                        else
                            nextGroupId = AllImages[i + 1].Group.Id;

                        if (prevGroupId != currentGroupId &&
                            nextGroupId != currentGroupId)
                            FilteredImages.Last().PositionInGroup = Helpers.GroupPosition.Only;
                        else if (prevGroupId != currentGroupId)
                            FilteredImages.Last().PositionInGroup = Helpers.GroupPosition.Start;
                        else if (nextGroupId != currentGroupId)
                            FilteredImages.Last().PositionInGroup = Helpers.GroupPosition.End;
                        else
                            FilteredImages.Last().PositionInGroup = Helpers.GroupPosition.Middle;
                    }
                }
            }
            ContentsChanged?.Invoke(this, new EventArgs());
        }


        public async Task SetTagsToFilter(List<string> tags)
        {
            TagsToFilter = tags;
            await UpdateQueryAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns>List of database IDs</returns>
        public async Task<List<Tuple<int, StorageFile>>> AddFilesToFolder(IReadOnlyList<StorageFile> files)
        {
            var result = new List<Tuple<int, StorageFile>>();
            foreach (var file in files)
            {
                result.Add(new Tuple<int, StorageFile>(
                    await DatabaseAccessService.InsertImageAsync(file.Path, DatabaseId), file));
            }
            await UpdateQueryAsync();
            ContentsChanged?.Invoke(this, new EventArgs());
            return result;
        }

        public async Task<List<string>> GetTagsOfImagesAsync()
        {
            return await DatabaseAccessService.GetVirtualfolderTags(DatabaseId);
        }

        public void InvokeContentsChanged()
        {
            ContentsChanged?.Invoke(this, new EventArgs());
        }

        public static bool operator ==(FolderItem f1, FolderItem f2)
        {
            if ((f1 is object && f2 is null) ||
                (f1 is null && f2 is object))
                return false;

            if (f1 is null && f2 is null)
                return true;

            return (f1.Name == f2.Name &&
                f1.DatabaseId == f2.DatabaseId);
        }

        public static bool operator !=(FolderItem f1, FolderItem f2)
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
            return obj is FolderItem item &&
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
