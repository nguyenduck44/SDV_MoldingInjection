using EQX.Core.Common;
using EQX.Core.LogHistory;
using Microsoft.Extensions.Configuration;
using SDV_MoldingInjection.Defines.ErrorLog;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class LogViewModel : ViewModelBase
    {
        protected readonly IConfiguration _configuration;
        private string _selectedDay;
        protected virtual string LogFolder => _configuration["Folders:LogFolder"] ?? "";
        public LogViewModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ObservableCollection<FileSystemNode> LogFiles { get; set; }

        public string SelectedDay
        {
            get { return _selectedDay; }
            set
            {
                _selectedDay = value;
                OnPropertyChanged(nameof(SelectedDay));
            }
        }

        public List<LogEntry> LoadLogEntries(string filePath)
        {
            var logEntries = new List<LogEntry>();

            foreach (var line in File.ReadAllLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(new[] { ',' }, 4);

                if (parts.Length < 4)
                    continue;

                var time = parts[0].Trim().Trim('[', ']');
                var type = parts[1].Trim();
                var source = parts[2].Trim();
                var description = parts[3].Trim();

                logEntries.Add(new LogEntry
                {
                    Time = time,
                    Type = type,
                    Source = source,
                    Description = description
                });
            }

            logEntries.Reverse();
            return logEntries;
        }

        public List<LogEntry> LoadLogEntries(string folderDayPath, string filterType)
        {
            var logEntries = new List<LogEntry>();

            if (string.IsNullOrWhiteSpace(folderDayPath))
                return logEntries;

            // yyyy-MM
            var month = folderDayPath.Substring(0, 7);

            // D:\...\Log\yyyy-MM\yyyy-MM-dd
            var dayFolder = Path.Combine(LogFolder, month, folderDayPath);

            if (!Directory.Exists(dayFolder))
                return logEntries;

            foreach (var file in Directory.GetFiles(dayFolder, "*.txt"))
            {
                foreach (var line in File.ReadAllLines(file))
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split(new[] { ',' }, 4);

                    if (parts.Length < 4)
                        continue;

                    var time = parts[0].Trim().Trim('[', ']');
                    var type = parts[1].Trim();
                    var source = parts[2].Trim();
                    var description = parts[3].Trim();

                    if (type != filterType || description.Contains("exception")) continue;

                    logEntries.Add(new LogEntry
                    {
                        Time = time,
                        Type = type,
                        Source = source,
                        Description = description
                    });
                }
            }
            logEntries.Reverse();
            return logEntries;
        }


        public void LoadLogFiles()
        {
            try
            {
                var rootNode = new FileSystemNode
                {
                    Name = "Log",
                    Path = LogFolder,
                    IsDirectory = true
                };

                PopulateTreeView(rootNode, LogFolder);

                LogFiles = new ObservableCollection<FileSystemNode> { rootNode };

                OnPropertyChanged(nameof(LogFiles));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading directory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateTreeView(FileSystemNode parentNode, string path)
        {
            try
            {
                var latestDayFolders = GetLatestLogDayFolders(path);

                foreach (var dayPath in latestDayFolders)
                {
                    var dayInfo = new DirectoryInfo(dayPath);
                    var dayNode = new FileSystemNode
                    {
                        Name = dayInfo.Name,
                        Path = dayPath,
                        IsDirectory = true
                    };

                    // Load file .txt
                    foreach (var file in Directory.GetFiles(dayPath, "*.txt"))
                    {
                        var fileInfo = new FileInfo(file);
                        dayNode.Children.Add(new FileSystemNode
                        {
                            Name = fileInfo.Name,
                            Path = file,
                            IsDirectory = false
                        });
                    }

                    parentNode.Children.Add(dayNode);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error populating TreeView for {path}: {ex}");
            }
        }

        private List<string> GetLatestLogDayFolders(string logRootPath, int dayCount = 10)
        {
            var dayFolders = new List<(DateTime date, string path)>();

            foreach (var monthDir in Directory.GetDirectories(logRootPath))
            {
                foreach (var dayDir in Directory.GetDirectories(monthDir))
                {
                    var dayName = Path.GetFileName(dayDir);

                    if (DateTime.TryParseExact(
                        dayName,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date))
                    {
                        dayFolders.Add((date, dayDir));
                    }
                }
            }

            return dayFolders
                .OrderByDescending(x => x.date)
                .Take(dayCount)
                .Select(x => x.path)
                .ToList();
        }
    }
}
