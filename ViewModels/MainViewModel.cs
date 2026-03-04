using FileScanner.Models;
using FileScanner.Reporting;
using System.ComponentModel;
using System.Runtime.CompilerServices;

// Reference: https://wellsb.com/csharp/learn/wpf-data-binding-csharp-inotifypropertychanged
// The MainViewModel class is responsible for holding the data that will be displayed in the UI,
namespace FileScanner.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ReportSummary _reportSummary;
        public ReportSummary ReportSummary
        {
            get => _reportSummary;
            set
            {
                _reportSummary = value;

                // Notify the UI that the ReportSummary property has changed, so it can update the display
                OnPropertyChanged();
            }
        }

        private List<FolderReportItem> _folderReport;
        public List<FolderReportItem> FolderReport
        {
            get => _folderReport;
            set
            {
                _folderReport = value;

                // Notify the UI that the FolderReport property has changed, so it can update the display
                OnPropertyChanged();
            }
        }

        private List<OwnerReportItem> _ownerReport;
        public List<OwnerReportItem> OwnerReport
        {
            get => _ownerReport;
            set
            {
                _ownerReport = value;

                // Notify the UI that the OwnerReport property has changed, so it can update the display
                OnPropertyChanged();
            }
        }

        private List<ScanError> _errorReport;
        public List<ScanError> ErrorReport
        {
            get => _errorReport;
            set
            {
                _errorReport = value;

                // Notify the UI that the ErrorReport property has changed, so it can update the display
                OnPropertyChanged();
            }
        }

        // The INotifyPropertyChanged interface allows the ViewModel to notify the UI when a property value changes, so the UI can update accordingly
        public event PropertyChangedEventHandler? PropertyChanged;

        // This method is called whenever a property value changes, and it raises the PropertyChanged event to notify the UI
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}