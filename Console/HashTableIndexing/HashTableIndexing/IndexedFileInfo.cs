using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashTableIndexing
{
    public class IndexedFileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public double Length { get; set; }
    }
}
