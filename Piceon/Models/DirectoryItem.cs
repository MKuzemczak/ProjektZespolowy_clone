using System;
using System.Collections.Generic;
using System.Text;

using Windows.Storage;

namespace Piceon.Models
{
    public class DirectoryItem
    {
        public StorageFolder Folder { get; set; }
        public List<DirectoryItem> Subdirectories { get; set; } = new List<DirectoryItem>();

    }
}
