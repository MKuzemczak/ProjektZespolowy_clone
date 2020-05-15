using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piceon.DatabaseAccess
{
    public class DatabaseSimilaritygroup
    {
        public int Id { get; set; } = int.MinValue;
        public string Name { get; set; }

        public DatabaseSimilaritygroup(int id = int.MinValue, string name = "")
        {
            Id = id;
            Name = name;
        }
    }
}
