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
        public DirectoryItem Parameter;

        public TreeViewItemSelectedEventArgs(DirectoryItem param) => Parameter = param;

    }
}
