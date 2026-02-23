using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Models
{
    public class FileResult
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public string Owner { get; set; } = string.Empty;
    }
}