namespace FileScanner.Models
{
    public class ScanResult
    {
        public List<FileResult> Results { get; set; } = new List<FileResult>();
        public TimeSpan Duration { get; set; }
    }
}
