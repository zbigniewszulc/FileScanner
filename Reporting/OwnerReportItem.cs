using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Reporting
{
    public class OwnerReportItem
    {
        public string Owner { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public double Percentage { get; set; }
    }
}
