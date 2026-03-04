namespace FileScanner.Reporting
{
    public class OwnerReportItem
    {
        public string Owner { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public double Percentage { get; set; }
        public long TotalSizeInBytes { get; set; }
    }
}
