using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Model
{
    internal class FileResult
    {
        public string FilePath { get; set; }
        public DateTime LastModified { get; set; }
        public string Owner {  get; set; }
    }
}
