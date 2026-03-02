using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Reporting
{
    public class ReportSummary
    {
        public int TotalFiles { get; set; }
        public int TotalErrors { get; set; }
        public DateTime? OldestFile { get; set; }
        public DateTime? NewestFile { get; set; }
        public double DurationSeconds { get; set; }
    }
}
