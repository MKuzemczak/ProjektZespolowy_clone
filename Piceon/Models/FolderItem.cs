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

        public abstract event EventHandler ContentsChanged;

        protected FolderItem() { }

    }
}
