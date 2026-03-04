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

            // This will store any errors that occur during the scan (e.g. access denied)
            var errors = new List<ScanError>();


            // Becaue scanning is CPU-bound process, run the scanning in a background task to keep the UI responsive
            await Task.Run(() =>
            {
                ScanDirectory(
                    folderPath,
                    targetDate,
                    beforeDate,
                    includeHiddenFiles,
                    includeSystemFiles,
                    results,
                    errors,
                    cancellationToken
                );
            });

            // Stop the stopwatch to get the total duration of the scan
            stopwatch.Stop();

            // Return the results, errors, and duration as a ScanResult object
            return new ScanResult
            {
                Results = results,
                Errors = errors,
                Duration = stopwatch.Elapsed
            };
        }


        // Recursive directory scanner with error handling and cancellation support
        // This method is called for each directory, starting with the root folder selected by the user
        private void ScanDirectory(
            string directoryPath,
            DateTime targetDate,
            bool beforeDate,
            bool includeHiddenFiles,
            bool includeSystemFiles,
            List<FileResult> results,
            List<ScanError> errors,
            CancellationToken cancellationToken)
        {
            // Check whether cancellation was requested and stop immediately if so
            cancellationToken.ThrowIfCancellationRequested();

            IEnumerable<string> files;

            // Try reading files in the current directory usign built-in Directory.EnumerateFiles method
            try
            {
                files = Directory.EnumerateFiles(directoryPath);
            }

            // If we don't have permission to access the directory, log the error and skip it
            catch (UnauthorizedAccessException)
            {
                errors.Add(new ScanError
                {
                    Path = directoryPath,
                    Reason = "Access Denied"
                });

                return;
            }

            // Catch any other unexpected exceptions and log them, but continue scanning other directories
            catch (Exception ex)
            {
                errors.Add(new ScanError
                {
                    Path = directoryPath,
                    Reason = ex.Message
                });

                return;
            }


            foreach (var filePath in files)
            {
                // Check whether cancellation was requested and stop immediately if so
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    // Get file information using FileInfo class
                    // ..which provides properties like Attributes and LastWriteTime
                    var fileInfo = new FileInfo(filePath);

                    if (!includeHiddenFiles && fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
                        continue;

                    if (!includeSystemFiles && fileInfo.Attributes.HasFlag(FileAttributes.System))
                        continue;

                    DateTime lastWrite = fileInfo.LastWriteTime;

                    // Check if the file matches the date criteria (before or after the target date)
                    bool matches = beforeDate ? lastWrite < targetDate : lastWrite > targetDate;

                    if (!matches)
                        continue;

                    // Get the file owner, if access is denied or retrival fails, it will return "Unknown"
                    string owner = GetFileOwner(filePath);

                    results.Add(new FileResult
                    {
                        FilePath = filePath,
                        LastModified = lastWrite,
                        Owner = owner,
                        SizeInBytes = fileInfo.Length
                    });
                }
                // Catch any exceptions that occur while processing individual files
                // .. (e.g. access denied, file deleted during scan) and log them
                catch (Exception ex)
                {
                    errors.Add(new ScanError
                    {
                        Path = filePath,
                        Reason = ex.Message
                    });
                }
            }

            // Now we need to scan subdirectories
            IEnumerable<string> subdirectories;

            // Try reading subdirectories in the current directory
            try
            {
                subdirectories = Directory.EnumerateDirectories(directoryPath);
            }

            // Catch any access errors
            catch (UnauthorizedAccessException)
            {
                errors.Add(new ScanError
                {
                    Path = directoryPath,
                    Reason = "Access Denied"
                });
                return;
            }

            // Catch any other unexpected exceptions and log them
            catch (Exception ex)
            {
                errors.Add(new ScanError
                {
                    Path = directoryPath,
                    Reason = ex.Message
                });
                return;
            }

            // We call the same ScanDirectory method for each subdirectory
            // .. which allows us to traverse the entire directory tree
            foreach (var subDir in subdirectories)
            {
                ScanDirectory(
                    subDir,
                    targetDate,
                    beforeDate,
                    includeHiddenFiles,
                    includeSystemFiles,
                    results,
                    errors,
                    cancellationToken
                );
            }

        }

        // Try to get the file owner of a file using its access control information
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
