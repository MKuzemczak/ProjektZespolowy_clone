using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace Piceon.Models
{
    public class ImageDataSource : INotifyCollectionChanged, System.Collections.IList, IItemsRangeInfo
    {
        // Folder that we are browsing
        private FolderItem _folder;

        // Dispatcher so we can marshal calls back to the UI thread
        private CoreDispatcher _dispatcher;

        // Cache for the file data that is currently being used
        private ItemCacheManager<ImageItem> itemCache;

        // Total number of files available
        private int _count = 1;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private ImageDataSource()
        {
            //Setup the dispatcher for the UI thread
            _dispatcher = Windows.UI.Xaml.Window.Current.Dispatcher;

            // The ItemCacheManager does most of the heavy lifting.
            // We pass it a callback that it will use to actually fetch data, and the max size of a request
            this.itemCache = new ItemCacheManager<ImageItem>(fetchDataCallback, 50);
            this.itemCache.CacheChanged += ItemCache_CacheChanged;
        }

        // Factory method to create the datasource
        // Requires async work which is why it needs a factory rather than being part of the constructor
        public static async Task<ImageDataSource> GetDataSource(FolderItem folder)
        {
            ImageDataSource ds = new ImageDataSource();
            await ds.SetFolder(folder);
            return ds;
        }

        // Set functionality for the folder
        public async Task SetFolder(FolderItem folder)
        {
            // Initialize the query and register for changes
            _folder = folder;
            await UpdateCount();
        }

        // Handler for when the filesystem notifies us of a change to the file list
        private void Folder_ContentsChanged(object sender, EventArgs e)
        {
            // This callback can occur on a different thread so we need to marshal it back to the UI thread
            if (!_dispatcher.HasThreadAccess)
            {
                var t = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, ResetCollection);
            }
            else
            {
                ResetCollection();
            }
        }

        // Handles a change notification for the list of files from the OS
        private void ResetCollection()
        {
            // Unhook the old change notification
            if (itemCache != null)
            {
                this.itemCache.CacheChanged -= ItemCache_CacheChanged;
            }

            // Create a new instance of the cache manager
            this.itemCache = new ItemCacheManager<ImageItem>(fetchDataCallback, 50);
            this.itemCache.CacheChanged += ItemCache_CacheChanged;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        async Task UpdateCount()
        {
            _count = await _folder.GetFilesCountAsync();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #region IList Implementation

        public bool Contains(object value)
        {
            return IndexOf(value) != -1;
        }

        public int IndexOf(object value)
        {
            return (value != null) ? itemCache.IndexOf((ImageItem)value) : -1;
        }

        public object this[int index]
        {
            get
            {
                // The cache will return null if it doesn't have the item. Once the item is fetched it will fire a changed event so that we can inform the list control
                return itemCache[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public int Count
        {
            get { return _count; }
        }

        #endregion

        //Required for the IItemsRangeInfo interface
        public void Dispose()
        {
            itemCache = null;
        }

        /// <summary>
        /// Primary method for IItemsRangeInfo interface
        /// Is called when the list control's view is changed
        /// </summary>
        /// <param name="visibleRange">The range of items that are actually visible</param>
        /// <param name="trackedItems">Additional set of ranges that the list is using, for example the buffer regions and focussed element</param>
        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
#if TRACE_DATASOURCE
            string s = string.Format("* RangesChanged fired: Visible {0}->{1}", visibleRange.FirstIndex, visibleRange.LastIndex);
            foreach (ItemIndexRange r in trackedItems) { s += string.Format(" {0}->{1}", r.FirstIndex, r.LastIndex); }
            Debug.WriteLine(s);
#endif
            // We know that the visible range is included in the broader range so don't need to hand it to the UpdateRanges call
            // Update the cache of items based on the new set of ranges. It will callback for additional data if required
            itemCache.UpdateRanges(trackedItems.ToArray());
        }

        // Callback from itemcache that it needs items to be retrieved
        // Using this callback model abstracts the details of this specific datasource from the cache implementation
        private async Task<ImageItem[]> fetchDataCallback(ItemIndexRange batch, CancellationToken ct)
        {
            IReadOnlyList<ImageItem> results = await _folder.GetImageItemsRangeAsync(
                batch.FirstIndex,
                Math.Min(Math.Max((int)batch.Length, 20), await _folder.GetFilesCountAsync() - batch.FirstIndex),
                ct);
            return results.ToArray();
        }

        // Event fired when items are inserted in the cache
        // Used to fire our collection changed event
        private void ItemCache_CacheChanged(object sender, CacheChangedEventArgs<ImageItem> args)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, args.oldItem, args.newItem, args.itemIndex));
            }
        }

        #region Parts of IList Not Implemented

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

