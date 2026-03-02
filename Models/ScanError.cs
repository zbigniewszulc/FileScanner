using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Models
{
    public class ScanError
    {
        public string Path { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
