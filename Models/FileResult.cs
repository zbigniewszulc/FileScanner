namespace FileScanner.Models
{
    public class FileResult
    {
        public string FilePath { get; set; } = string.Empty;
        public DateTime LastModified { get; set; }
        public string Owner { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
    }
}