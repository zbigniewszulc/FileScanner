namespace FileScanner.Reporting
{
    public class FolderReportItem
    {
        public string Folder { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public DateTime OldestFile { get; set; }
        public DateTime NewestFile { get; set; }
        public long TotalSizeInBytes { get; set; }
    }
}
