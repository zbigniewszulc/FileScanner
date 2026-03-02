using FileScanner.Models;
using FileScanner.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Services
{
    public  class ReportBuilder
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
    }
}
