using FileScanner.Models;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;

namespace FileScanner.Services
{
    public class FileScannerService
    {
        // Main method resposible for scanning the selected folder
        // Passing cancellation token which allows the scan to be stopped safely
        public async Task<ScanResult> ScanAsync
        (
            string folderPath,
            DateTime targetDate,
            bool beforeDate,
            bool includeHiddenFiles,
            bool includeSystemFiles,
            CancellationToken cancellationToken
        )
        {
            // We measure execution time to show later in the summary popup
            var stopwatch = Stopwatch.StartNew();

            // This list will store all matching files
            var results = new List<FileResult>();

            // Run the scan in the background so the UI stays responsive
            await Task.Run(() =>
            {
                // Go through all files in the folder (including subfolders)
                foreach (var filePath in Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    // Check whether cancellation was requested and stop immediately if so
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        // We using FileInfo to access attributes and metadata
                        var fileInfo = new FileInfo(filePath);

                        // Skip hidden files if the user didn't include them
                        if (!includeHiddenFiles && fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                            continue;

                        // Skip system files if the user didn't include them
                        if (!includeSystemFiles && fileInfo.Attributes.HasFlag(FileAttributes.System))
                            continue;

                        // Get last modified date of the file
                        DateTime lastWrite = fileInfo.LastWriteTime;

                        // Check if the file matches the slected date condition
                        bool matches = beforeDate ? lastWrite < targetDate : lastWrite > targetDate;

                        if (!matches)
                            continue;

                        // Try to get file owner (may fail if no permission) 
                        string owner = GetFileOwner(filePath);

                        // Add matching file to results
                        results.Add(new FileResult
                        {
                            FilePath = filePath,
                            LastModified = lastWrite,
                            Owner = owner
                        });
                    }

                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we do not have permission to access
                        continue;
                    }

                    catch (PathTooLongException)
                    {
                        // Skip problematic paths
                        continue;
                    }
                }
            }, cancellationToken);

            stopwatch.Stop();

            //Return results along with execution duration
            return new ScanResult
            {
                Results = results,
                Duration = stopwatch.Elapsed
            };
        }

        // Try to get the file owner
        // Return "Unknown" if access is denied or retrival failed
        private string GetFileOwner(string path)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                // Get file security information (permission, owner, etc.)
                var security = fileInfo.GetAccessControl();
                // Retrieve the file owner in readalble Windows user name
                var owner = security.GetOwner(typeof(NTAccount));

                // Return owner name if available, otherwise return "Unknown"
                return owner?.ToString() ?? "Unknown";
            }

            catch
            {
                return "Unknown";
            }
        }
    }
}
