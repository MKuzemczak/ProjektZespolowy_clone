using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piceon.Models;

namespace Piceon.Models
{
    public class TreeViewItemSelectedEventArgs : EventArgs
    {
        public FolderItem Parameter;

        public TreeViewItemSelectedEventArgs(FolderItem param) => Parameter = param;

    }
}
