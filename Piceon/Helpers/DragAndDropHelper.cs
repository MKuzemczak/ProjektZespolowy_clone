using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Piceon.Models;

namespace Piceon.Helpers
{
    public static class DragAndDropHelper
    {
        public static List<object> DraggedItems { get; } = new List<object>();

        public static bool DropSuccessful { get; set; } = false;
    }
}
