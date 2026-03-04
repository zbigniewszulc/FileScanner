using ByteSizeLib;

namespace FileScanner.Reporting
{
    public class FolderReportItem
    {
        public string Folder { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public DateTime OldestFile { get; set; }
        public DateTime NewestFile { get; set; }
        public long TotalSizeInBytes { get; set; }

        // Expression-bodied property to convert total size in bytes to a human-readable format using Using ByteSizeLib
        public string TotalSizeReadableFormat => ByteSize.FromBytes(TotalSizeInBytes).ToString();
    }
}
