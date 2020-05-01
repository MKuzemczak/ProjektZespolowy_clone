﻿using System;
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

        private List<DatabaseImage> AllImages { get; set; } = new List<DatabaseImage>();
        private List<DatabaseImage> FilteredImages { get; set; } = new List<DatabaseImage>();

        public static async Task<VirtualFolderItem> FromDatabaseVirtualFolder(DatabaseVirtualFolder virtualFolder)
        {
            VirtualFolderItem result = new VirtualFolderItem
            {
                Name = virtualFolder.Name,
                DatabaseId = virtualFolder.Id
            };
            result.Subfolders = await result.GetSubfoldersAsync();
            await result.UpdateQueryAsync();

            return result;
        }

        public static async Task<VirtualFolderItem> GetNew(string name)
        {
            var dbvf = await DatabaseAccessService.InsertVirtualFolderAsync(name);

            return await FromDatabaseVirtualFolder(dbvf);
        }

        public override async Task<IReadOnlyList<StorageFile>> GetStorageFilesRangeAsync(int firstIndex, int length)
        {
            var result = new List<StorageFile>();

            foreach (var item in FilteredImages)
            {
                try
                {
                    result.Add(await StorageFile.GetFileFromPathAsync(item.Path));
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
            }

            return result;
        }

        public override async Task<IReadOnlyList<ImageItem>> GetImageItemsRangeAsync(int firstIndex, int length, CancellationToken ct = new CancellationToken())
        {
            var result = new List<ImageItem>();
            ct.ThrowIfCancellationRequested();

            var selectedRangeFiles = FilteredImages.GetRange(firstIndex, length);
            var storageFiles = new List<Tuple<int, StorageFile>>();

            for (int i = 0; i < selectedRangeFiles.Count(); i++)
            {
                ct.ThrowIfCancellationRequested();
                StorageFile storageFile = null;
                try
                {
                    storageFile = await StorageFile.GetFileFromPathAsync(selectedRangeFiles[i].Path);
                }
                catch (FileNotFoundException)
                {
                    continue;
                }
                storageFiles.Add(new Tuple<int, StorageFile>(i, storageFile));
            }

            int prevGroupId = -1;
            if (firstIndex != 0)
                prevGroupId = FilteredImages[firstIndex - 1].Group.Id;

            for (int i = 0; i < storageFiles.Count(); i++)
            {
                ct.ThrowIfCancellationRequested();
                int currentGroupId = selectedRangeFiles[storageFiles[i].Item1].Group.Id;
                var image = await ImageItem.FromStorageFile(storageFiles[i].Item2, storageFiles[i].Item1 + firstIndex, ct, ImageItem.Options.Thumbnail);
                image.DatabaseId = selectedRangeFiles[storageFiles[i].Item1].Id;

                bool nextGroupDifferent = ((firstIndex + storageFiles[i].Item1) == FilteredImages.Count - 1 ||
                    currentGroupId != FilteredImages[firstIndex + storageFiles[i].Item1 + 1].Group.Id);

                if (currentGroupId < 0)
                {
                    image.PotitionInGroup = Helpers.GroupPosition.None;
                }
                else if (prevGroupId != currentGroupId)
                {
                    if (nextGroupDifferent)
                    {
                        image.PotitionInGroup = Helpers.GroupPosition.Only;
                    }
                    else
                    {
                        image.PotitionInGroup = Helpers.GroupPosition.Start;
                    }
                }
                else if (nextGroupDifferent)
                {
                    image.PotitionInGroup = Helpers.GroupPosition.End;
                }
                else
                {
                    image.PotitionInGroup = Helpers.GroupPosition.Middle;
                }

                prevGroupId = currentGroupId;

                result.Add(image);
            }

            return result;
        }

        public override int GetFilesCount()
        {
            return FilteredImages.Count;
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

        public override async Task UpdateQueryAsync()
        {
            var raw = await DatabaseAccessService.GetVirtualfolderImagesWithGroupsAndTags(DatabaseId);
            AllImages = raw.OrderByDescending(i => i.Group.Id).ToList();
            FilteredImages = AllImages.
                Where(i => { return (TagsToFilter is null || TagsToFilter.Count == 0) ?
                    true : TagsToFilter.Intersect(i.Tags).Count() > 0; }).ToList();
            ContentsChanged?.Invoke(this, new EventArgs());
        }


        public override async Task SetTagsToFilter(List<string> tags)
        {
            TagsToFilter = tags;
            await UpdateQueryAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <returns>List of database IDs</returns>
        public override async Task<List<int>> AddFilesToFolder(IReadOnlyList<StorageFile> files)
        {
            var result = new List<int>();
            foreach (var file in files)
            {
                result.Add(await DatabaseAccessService.InsertImageAsync(file.Path, DatabaseId));
            }
            await UpdateQueryAsync();
            ContentsChanged?.Invoke(this, new EventArgs());
            return result;
        }

        public override async Task<List<string>> GetTagsOfImagesAsync()
        {
            return await DatabaseAccessService.GetVirtualfolderTags(DatabaseId);
        }

        public override void InvokeContentsChanged()
        {
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
