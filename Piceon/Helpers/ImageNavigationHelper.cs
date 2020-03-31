using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piceon.Models;

namespace Piceon.Helpers
{
    public static class ImageNavigationHelper
    {
        public static ImageItem SelectedImage { get; set; }
        public static FolderItem ContainingFolder { get; set; }
        public static ImageDataSource ContainingDataSource { get; set; }
    }
}
