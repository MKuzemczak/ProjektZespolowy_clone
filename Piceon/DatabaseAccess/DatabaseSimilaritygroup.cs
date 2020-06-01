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

        public static bool operator ==(DatabaseSimilaritygroup f1, DatabaseSimilaritygroup f2)
        {
            if ((f1 is object && f2 is null) ||
                (f1 is null && f2 is object))
                return false;

            if (f1 is null && f2 is null)
                return true;

            return (f1.Name == f2.Name &&
                f1.Id == f2.Id);
        }

        public static bool operator !=(DatabaseSimilaritygroup f1, DatabaseSimilaritygroup f2)
        {
            if ((f1 is object && f2 is null) ||
                (f1 is null && f2 is object))
                return true;

            if (f1 is null && f2 is null)
                return false;

            return !(f1.Name == f2.Name &&
                f1.Id == f2.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is DatabaseSimilaritygroup similaritygroup &&
                   Id == similaritygroup.Id &&
                   Name == similaritygroup.Name;
        }

        public override int GetHashCode()
        {
            int hashCode = -1919740922;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
