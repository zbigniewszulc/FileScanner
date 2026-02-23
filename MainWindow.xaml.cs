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
using FileScanner.Services;


namespace FileScanner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Intialise UI components defined in XAML
        public MainWindow()
        {
            InitializeComponent();
        }

        // Runs when user clicks the "Start Scan" button 
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Read folder path from the textbox
            string folderPath = FolderTextBox.Text;

            // No point scanning if the folder dosn't exist
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Folder does not exist.");
                return;
            }

            // Make sure user selected a date
            if (DatePickerControl.SelectedDate == null)
            {
                MessageBox.Show("Please select a date.");
                return;
            }

            // Get selected date value
            DateTime selectedDate = DatePickerControl.SelectedDate.Value;

            // Determine if we're looking for files before or after the selected date
            bool beforeDate = BeforeRadio.IsChecked == true;

            // Create the scanning service (all file logic lives there)
            var service = new FileScannerService();

            // Clear old results before running the scan
            ResultsDataGrid.ItemsSource = null;

            // Run the scan in background to prevent the UI from freeze
            var result = await service.ScanAsync(
                folderPath,
                selectedDate,
                beforeDate
            );

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

        }
    }
}