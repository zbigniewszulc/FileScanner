using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FileScanner.Models;
using FileScanner.Services;
using Microsoft.Win32;


namespace FileScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ScanResult? _lastScanResult;

        // This variable is needed to make sure that Main Windows definitely was Initialised
        private bool _isLoaded;

        // Helper methods 
        // Display messages in form of popup
        private void ShowInfo(string message)
        {
            MessageBox.Show(message, "Message:", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Display messages in form of popup
        private void ShowError(string message)
        {
            MessageBox.Show(message, "Message:", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Display messages in form of popup
        private void ShowWarning(string message)
        {
            MessageBox.Show(message, "Message:", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            // No point scanning if the folder dosn't exist. It might happen that folder was removed by other user after added via Browse button 
            if (!Directory.Exists(folderPath))
            {
                ShowWarning("Folder does not exist.");
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

            try 
            {
                // Create the scanning service (all file logic lives there)
                var service = new FileScannerService();

                // Clear old results before running the scan
                ResultsDataGrid.ItemsSource = null;

                // Run the scan in background to prevent the UI from freeze
                var result = await service.ScanAsync(
                    folderPath,
                    selectedDate,
                    beforeDate,
                    includeHiddenFiles,
                    includeSystemFiles
                );

                _lastScanResult = result;

                // Show the results in the grid
                ResultsDataGrid.ItemsSource = result.Results;

                // Count how many matching files we found 
                int fileCount = result.Results.Count;

                // Show summary popup
                MessageBox.Show(
                    $"Scan completed successfully!\n\n" +
                    $"Total files found: {fileCount}\n" +
                    $"Execution time: {result.Duration.TotalSeconds:F2} sec",
                    "Scan Results",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                // Show export button only if we have results
                ExportButton.Visibility = result.Results.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            // Catch and display error if any 
            catch (Exception ex) 
            {
                ShowError($"Scan failed: {ex.Message}");
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

            // Build a default file name based on scan mode and selected date
            string defaultFileName = $"FileAudit_{mode}_{formattedDate}.csv";

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
            // Any filter change means that previous results are no longer valid
            ResetResultsState();
        }

        // Reset last scan results and hides Export Button in the UI
        private void ResetResultsState()
        {
            _lastScanResult = null;
            ResultsDataGrid.ItemsSource = null;
            ExportButton.Visibility = Visibility.Collapsed;
        }
    }
}