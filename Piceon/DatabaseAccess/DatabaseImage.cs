using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piceon.DatabaseAccess
{
    public class DatabaseImage
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public bool Scanned { get; set; }
        public DatabaseSimilaritygroup Group { get; set; } = new DatabaseSimilaritygroup();
        public List<string> Tags { get; } = new List<string>();

    }
}
