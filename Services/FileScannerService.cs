using FileScanner.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;

namespace FileScanner.Services
{
    public class FileScannerService
    {
        public ScanResult Scan
        (
            string folderPath,
            DateTime targetDate,
            bool beforeDate
        )
        {
            var results = new List<FileResult>();
            var stopwatch = Stopwatch.StartNew();
            try 
            {
                foreach (var filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    try 
                    {
                        DateTime lastWrite = File.GetLastWriteTime(filePath);
                        bool matches = beforeDate ? lastWrite < targetDate : lastWrite > targetDate;

                        if ( !matches ) 
                            continue;

                        string owner = GetFileOwner(filePath);

                        results.Add(new FileResult
                        {
                            FilePath = filePath,
                            LastModified = lastWrite,
                            Owner = owner
                        });
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // No acceess - we skip it
                    }
                    catch (PathTooLongException)
                    {
                        // Too long path - we skip it
                    }
                    catch (IOException)
                    {
                        // The file may have been deleted during scanning - we skip it
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // No access to the root folder
            }

            stopwatch.Stop();

            return new ScanResult
            {
                Results = results,
                Duration = stopwatch.Elapsed
            };
        }

        public async Task<ScanResult> ScanAsync(
            string folderPath,
            DateTime targetDate,
            bool beforeDate
        )
        {
            return await Task.Run(() => Scan(folderPath, targetDate, beforeDate));
        }

        private string GetFileOwner(string path) 
        {
            try 
            {
                var fileInfo = new FileInfo(path);
                var security = fileInfo.GetAccessControl();
                var owner = security.GetOwner(typeof(NTAccount));

                return owner?.ToString() ?? "Unknown";
            }
            
            catch 
            {
                return "Unknown"; 
            }
        }
    }
}
