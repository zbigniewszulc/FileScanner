using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Models
{
    public class ScanResult
    {
        public List<FileResult> Results { get; set; } = new List<FileResult>();
        public TimeSpan Duration { get; set; }
    }
}
