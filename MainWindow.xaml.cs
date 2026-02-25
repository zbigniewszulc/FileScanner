using FileScanner.Models;
using FileScanner.Services;
using Microsoft.Win32;
using System.IO;
using System.Windows;


namespace FileScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ScanResult? _lastScanResult;

        // This field is required to singal cancellation of ongoing folders/files scan
        private CancellationTokenSource _cancellationTokenSource;

        // This variable is needed to make sure that Main Windows definitely was Initialised
        private bool _isLoaded;

        // Helper methods 
        // Display messages in form of popup
        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "File Scanner: Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Display messages in form of popup
        private void ShowError(string message)
        {
            MessageBox.Show(message, "File Scanner: Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Display messages in form of popup
        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "File Scanner: Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // Intialise UI components defined in XAML
        public MainWindow()
        {
            InitializeComponent();
            _isLoaded = true;
        }

        // Runs when user clicks the "Start Scan" button 
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Read folder path from the textbox
            string folderPath = FolderTextBox.Text;

            // If no folder has been selected
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                ShowWarning("Please select a folder first.");
                return;
            }

            // No point scanning if the folder dosn't exist.
            // It might happen that folder was removed by other user after added via Browse button 
            if (!Directory.Exists(folderPath))
            {
                ShowWarning("Selected folder does not exist.");
                return;
            }

            // Make sure user selected a date
            if (DatePickerControl.SelectedDate == null)
            {
                ShowWarning("Please select a date.");
                return;
            }

            // Get selected date value
            DateTime selectedDate = DatePickerControl.SelectedDate.Value;

            // Determine if we're looking for files before or after the selected date
            bool beforeDate = BeforeRadio.IsChecked == true;

            // Check if user selected hidden files
            bool includeHiddenFiles = IncludeHiddenFilesCheckBox.IsChecked == true;

            //Check if use selected system proteced files
            bool includeSystemFiles = IncludeSystemFilesCheckBox.IsChecked == true;

            // Hide Export Button 
            ExportButton.Visibility = Visibility.Hidden;

            try
            {
                // Intialise cancellation - create cancellation source for this scan session
                _cancellationTokenSource = new CancellationTokenSource();

                // Create the scanning service (all file logic lives there)
                var service = new FileScannerService();

                // Clear old results before running the scan
                ResultsDataGrid.ItemsSource = null;

                // Lock UI so user will not be able to click any buttons 
                LockUI();

                // Run the scan in background to prevent the UI from freeze
                // Among multiple passed arguments, also pas cancellation token to the service 
                // ..so user can easly and safely stop the scanning process
                var result = await service.ScanAsync(
                    folderPath,
                    selectedDate,
                    beforeDate,
                    includeHiddenFiles,
                    includeSystemFiles,
                    _cancellationTokenSource.Token
                );

                _lastScanResult = result;

                // Show the results in the grid
                ResultsDataGrid.ItemsSource = result.Results;

                // Count how many matching files we found 
                int fileCount = result.Results.Count;

                // Unlock UI
                UnlockUI();

                // Show summary popup
                MessageBox.Show(
                    $"Scan completed successfully!\n\n" +
                    $"Total files found: {fileCount}\n" +
                    $"Execution time: {result.Duration.TotalSeconds:F2} sec\n" + "" +
                    $"                          ( {result.Duration.TotalMilliseconds:F0} ms )",
                    "File Scanner: Summary",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Show export button only if we have results
                ExportButton.Visibility = result.Results.Count > 0 ? Visibility.Visible : Visibility.Hidden;

                // Show Delete All button only if we have results
                //DeleteButton.Visibility = result.Results.Count > 0 ? Visibility.Visible: Visibility.Hidden;
            }

            // OperationCanceledException is thrown when user cancels the scanning process
            catch (OperationCanceledException)
            {
                UnlockUI();
                ShowWarning("Scan was cancelled by user.");
            }

            // Catch and display error if any 
            catch (Exception ex)
            {
                UnlockUI();
                ShowError($"Scan failed: {ex.Message}");
            }

            finally
            {
                // Clean cancellation source reference
                _cancellationTokenSource = null;
            }
        }

        // When "Browse..." button is clicked, this method sets the folder text box and tooltip content for the selected folder
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                FolderTextBox.Text = dialog.FolderName;
                FolderTextBox.ToolTip = dialog.FolderName;
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            // Nothing to export if there are no results
            if (_lastScanResult == null || _lastScanResult.Results.Count == 0)
                return;

            // If the SelectedDate is null or is not DateTime type display apporpriate message
            // ..otherwise create selectedDate variable so it can be used further to generate meaningful file name
            if (DatePickerControl.SelectedDate is not DateTime selectedDate)
            {
                ShowWarning("Please select a date.");
                return;
            }

            bool beforeDate = BeforeRadio.IsChecked == true;

            string mode = beforeDate ? "Before" : "After";
            string formattedDate = selectedDate.ToString("dd-MM-yyyy");

            // This variable stores singular or plural form of the file name ending
            string fileEnding = _lastScanResult.Results.Count == 1 ? "file" : "files";

            // Build a default file name based on scan mode and selected date
            string defaultFileName = $"FileAudit_{mode}_{formattedDate}_{_lastScanResult.Results.Count}{fileEnding}.csv";

            // Create and configure Save File Dialog and save in memory for further use
            // The filter limits the selection to CSV File only and
            // .. File Name is the name used for the saved file
            var dialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = defaultFileName
            };

            // Show Save As window
            // Continue if the user confirmed the Save As dialog by clicking "Save"
            // If user clicked Cancel the dialog closes and nothing happens
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Create the service responsible for exporting the CSV file
                    var exportService = new ExportService();

                    // Delegate the actual file writing to the service
                    await exportService.ExportToCsvAsync(_lastScanResult, dialog.FileName);

                    // If the export completed successfuly, show appropriate message
                    ShowInfo("Export completed successfully.");
                }
                // If something goes wrong during export show an error to the user
                catch (Exception ex)
                {
                    ShowError($"Export failed: {ex.Message}");
                }
            }
        }

        // This method will trigger when user changed any search criteria/filter
        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            // If the MainWindow has not been loaded yet do not reset results state
            if (!_isLoaded)
                return;

            // Hide Delete All button
            //DeleteButton.Visibility = Visibility.Collapsed;

            // Any filter change means that previous results are no longer valid
            ResetResultsState();
        }

        // Reset last scan results and hides Export Button in the UI
        private void ResetResultsState()
        {
            _lastScanResult = null;
            ResultsDataGrid.ItemsSource = null;
            ExportButton.Visibility = Visibility.Hidden;
        }

        // Send cancelation signal to the runnng scanning process 
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        // Lock the UI (i.e. Disable Start Scan button, Collapse the Export Results button )
        private void LockUI()
        {
            UIFilterSection.IsEnabled = false;
            StartButton.IsEnabled = false;
            StopButton.Visibility = Visibility.Visible;
            ScanProgressBar.Visibility = Visibility.Visible;
        }

        // Unlock UI (i.e. Enable Start Scan button).
        // Additionally disable progress bar as the scanning process finalised at this stage
        private void UnlockUI()
        {
            UIFilterSection.IsEnabled = true;
            StartButton.IsEnabled = true;
            StopButton.Visibility = Visibility.Hidden;
            ScanProgressBar.Visibility = Visibility.Hidden;
        }

        // Handles the click event for "Delete All" button 
        //private void DeleteButton_Click(object sender, RoutedEventArgs e)
        //{
        //    // If there are no scan results there is nothing to delete 
        //    if (_lastScanResult == null || _lastScanResult.Results.Count == 0 )
        //            return;

        //    // Ask user to confirm deletion
        //    var confirmation = MessageBox.Show
        //        (
        //        $"Are you sure you want to permanently delete {_lastScanResult.Results.Count} files?\n\n " +
        //        $"This action cannot be undone!",
        //        "File Scanner: Warning",
        //        MessageBoxButton.YesNo,
        //        MessageBoxImage.Warning
        //        );

        //    // Do nothing if user responded "No" in MessageBox
        //    if (confirmation != MessageBoxResult.Yes)
        //        return;

        //    // Counters to track how many files were deleted and hom many failed
        //    int deletedCount = 0; 
        //    int failedCount = 0;

        //    // Interate through all files
        //    foreach (var file in _lastScanResult.Results) 
        //    {
        //        try 
        //        {
        //            // Make sure the files still exists 
        //            if (File.Exists(file.FilePath)) 
        //            {
        //                File.Delete(file.FilePath);
        //                deletedCount++;
        //            }
        //        }

        //        catch 
        //        {
        //            failedCount++;
        //        }
        //    }

        //    // Display popup with deletion summary
        //    ShowInfo($"Deletion completed.\n\nDeleted: {deletedCount}\nFailed: {failedCount}");

        //    // Hide Delete All button
        //    DeleteButton.Visibility = Visibility.Collapsed;

        //    // Refresh UI after deletion
        //    ResetResultsState();
        //}
    }
}