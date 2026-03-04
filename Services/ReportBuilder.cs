using FileScanner.Models;
using FileScanner.Reporting;
using System.IO;

namespace FileScanner.Services
{
    public class ReportBuilder
    {
        private readonly ScanResult _scanResult;
        public ReportBuilder(ScanResult scanResult)
        {
            _scanResult = scanResult;
        }


        // This method builds a summary report based on the scan results,
        // .. including total files, errors, and file date range
        public ReportSummary BuildSummary()
        {
            var summary = new ReportSummary
            {
                TotalFiles = _scanResult.Results.Count,
                TotalErrors = _scanResult.Errors.Count,
                DurationSeconds = _scanResult.Duration.TotalSeconds
            };

            // If there are any files in the results, calculate the oldest and newest file dates
            if (_scanResult.Results.Any()) // LINQ method to check if there are any results
            {
                // We use LINQ to find the minimum and maximum LastModified dates from the file results
                summary.OldestFile = _scanResult.Results.Min(r => r.LastModified);
                summary.NewestFile = _scanResult.Results.Max(r => r.LastModified);
            }

            return summary;
        }

        // This method builds a detailed report grouped by folder, showing file count and date range for each folder
        // We use LINQ to group the file results by their folder path, then create a FolderReportItem for each group
        // The report is ordered by file count in descending order, so folders with the most files appear first
        public List<FolderReportItem> BuildFolderReport()
        {
            var folderReport = _scanResult.Results
                .GroupBy(r => Path.GetDirectoryName(r.FilePath)) // Group results by their folder path
                .Select(g => new FolderReportItem
                {
                    Folder = g.Key ?? "Unknown", // Get the folder path (group key), use "Unknown" if null
                    FileCount = g.Count(), // Count how many files are in this folder
                    OldestFile = g.Min(r => r.LastModified), // Find the oldest file date in this folder
                    NewestFile = g.Max(r => r.LastModified) // Find the newest file date in this folder
                })
                .OrderByDescending(i => i.FileCount) // Order the report items by file count, descending
                .ToList();

            return folderReport;
        }

        // This method builds a detailed report grouped by file owner, showing file count and percentage of total files for each owner
        // We use LINQ to group the file results by their owner, then create an OwnerReportItem for each group
        // The report is ordered by file count in descending order, so owners with the most files appear first
        public List<OwnerReportItem> BuildOwnerReport()
        {
            int totalFiles = _scanResult.Results.Count;

            var ownerReport = _scanResult.Results
                .GroupBy(r => r.Owner) // Group results by file owner
                .Select(g => new OwnerReportItem
                {
                    Owner = g.Key ?? "Unknown", // Get the owner name (group key), use "Unknown" if null
                    FileCount = g.Count(), // Count how many files are owned by this owner
                    Percentage = (double)g.Count() / totalFiles * 100 // Calculate the percentage of total files owned by this owner
                })
                .OrderByDescending(i => i.FileCount) // Order the report items by file count, descending
                .ToList();

            return ownerReport;
        }
    }
}
