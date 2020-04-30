﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.System;

using Piceon.DatabaseAccess;
using Windows.Storage.Search;
using Windows.UI.Popups;
using System.Threading;

namespace Piceon.Models
{
    public abstract class FolderItem
    {
        public int DatabaseId;

        public List<FolderItem> Subfolders { get; protected set; } = new List<FolderItem>();

        public FolderItem ParentFolder { get; protected set; }

        public string Name { get; set; }

        public List<string> TagsToFilter { get; set; } = new List<string>();

        public abstract Task<IReadOnlyList<StorageFile>> GetStorageFilesRangeAsync(int firstIndex, int length);

        public abstract Task<IReadOnlyList<ImageItem>> GetImageItemsRangeAsync(int firstIndex, int length, CancellationToken ct);

        public abstract int GetFilesCount();

        public abstract Task RenameAsync(string newName);

        public  abstract Task SetParentAsync(FolderItem parent);

        public abstract Task DeleteAsync();

        public abstract Task CheckContentAsync();

        public abstract Task<List<int>> AddFilesToFolder(IReadOnlyList<StorageFile> files);

        public abstract void InvokeContentsChanged();

        public abstract Task UpdateQueryAsync();

        protected abstract Task<List<FolderItem>> GetSubfoldersAsync();

        public abstract event EventHandler ContentsChanged;

        protected FolderItem() { }

    }
}
