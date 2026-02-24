using FileScanner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileScanner.Services
{
    class ExportService
    {
        // Export scan results to CSV file 
        public async Task ExportToCsvAsync(ScanResult scanResult, string filePath)
        {
            // Prevent exporting empty data
            if (scanResult == null || scanResult.Results.Count == 0)
                throw new InvalidOperationException("No results to export.");

            // Use the built-in StringBuilder class to effciently build the CSV content line by line 
            var builder = new StringBuilder();
            
            //CSV headers row
            builder.AppendLine("File Path, Last modified, Owner");

            foreach (var file in scanResult.Results)
            {
                // Escape file path in case it contains commas as CSV file is comma delimited
                string safePath = $"\"{file.FilePath}\"";

                builder.AppendLine($"{safePath},{file.LastModified:dd-MM-yyyy HH:mm},{file.Owner}");
            }

            //Write the CSV content to disk using UTF-8 encoding 
            await File.WriteAllTextAsync(filePath, builder.ToString(), Encoding.UTF8);
        }
    }
}
