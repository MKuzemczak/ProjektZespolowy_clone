using System;
using System.Collections.Generic;
using System.Text;

namespace Piceon.Core.Models
{
    public class DirectoryItem
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public ICollection<DirectoryItem> Subdirectories { get; set; }
    }
}
