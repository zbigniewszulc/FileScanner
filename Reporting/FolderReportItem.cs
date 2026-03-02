using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Reporting
{
    public class FolderReportItem
    {
        public string Folder { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public DateTime OldestFile { get; set; }
        public DateTime NewestFile { get; set; }
    }
}
