using ByteSizeLib;

namespace FileScanner.Reporting
{
    public class OwnerReportItem
    {
        public string Owner { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public double Percentage { get; set; }
        public long TotalSizeInBytes { get; set; }

        // Expression-bodied property to convert total size in bytes to a human-readable format using Using ByteSizeLib
        public string TotalSizeReadableFormat => ByteSize.FromBytes(TotalSizeInBytes).ToString();
    }
}
