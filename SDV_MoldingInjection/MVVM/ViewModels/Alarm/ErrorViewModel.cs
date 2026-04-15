using EQX.Core.Common;
using Microsoft.Extensions.Configuration;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.ErrorLog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class ErrorViewModel : ViewModelBase
    {
        private readonly IConfiguration _configuration;
        private string selectedDaySearch;
        private ErrorLogCount selectedErrorLogCount;

        private string ErrorFolder => _configuration["Folders:ErrorFolder"] ?? "";

        public ErrorViewModel(IConfiguration configuration)
        {
            _configuration = configuration;

            if (LogDays != null && LogDays.Count > 0)
            {
                SelectedDaySearch = LogDays.First();
            }
        }

        public string SelectedDaySearch
        {
            get { return selectedDaySearch; }
            set
            {
                selectedDaySearch = value;
                OnPropertyChanged();

                LoadErrorData(ErrorFilePathList);
            }
        }

        public ObservableCollection<string> LogDays
        {
            get
            {
                List<string> logDays = GetLatestLogDayFolders(ErrorFolder);

                var logs = new ObservableCollection<string>();
                foreach (var day in logDays)
                {
                    logs.Add(Path.GetFileName(day));
                }

                return logs;
            }
        }

        public ObservableCollection<ErrorLogEntry> ErrorLogEntries { get; set; }

        public ObservableCollection<ErrorLogCount> ErrorLogCounts { get; set; }

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

        public List<string> ErrorFilePathList
        {
            get
            {
                List<string> filterFilePathList = new();

                if (string.IsNullOrEmpty(SelectedDaySearch)) return filterFilePathList;

                // yyyy-MM
                var month = SelectedDaySearch.Substring(0, 7);

                // D:\...\Log\yyyy-MM\yyyy-MM-dd
                var dayFolder = Path.Combine(ErrorFolder, month, SelectedDaySearch);

                if (Directory.Exists(dayFolder) == false) return filterFilePathList;

                var lastFile = new DirectoryInfo(dayFolder).GetFiles("*.txt");

                foreach (var file in lastFile)
                {
                    if (file != null)
                    {
                        filterFilePathList.Add(file.FullName);
                    }
                }

                return filterFilePathList;
            }
        }

        public async Task LoadErrorData(List<string> filePaths)
        {
            await Task.Run(() =>
            {
                ErrorLogEntries = LoadErrorEntries(filePaths);
                ErrorLogCounts = LoadLogCounts(ErrorLogEntries);

                OnPropertyChanged(nameof(ErrorLogEntries));
                OnPropertyChanged(nameof(ErrorLogCounts));
            });

            return;
        }

        public ErrorLogCount SelectedErrorLogCount
        {
            get => selectedErrorLogCount;
            set
            {
                selectedErrorLogCount = value;
                OnPropertyChanged();

                ErrorLogEntries = LoadErrorEntries(ErrorFilePathList);
                OnPropertyChanged(nameof(ErrorLogEntries));
            }
        }
        private ObservableCollection<ErrorLogEntry> LoadErrorEntries(List<string> path)
        {
            ObservableCollection<ErrorLogEntry> errorLogEntries = new ObservableCollection<ErrorLogEntry>();
            try
            {
                List<ErrorLogEntry> errorList = new List<ErrorLogEntry>();
                foreach (var file in path)
                {
                    errorList.AddRange(LoadErrorEntries(file));
                }

                foreach (var item in errorList)
                {

                    errorLogEntries.Add(item);
                }
                return errorLogEntries;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return errorLogEntries;
            }
        }

        private List<ErrorLogEntry> LoadErrorEntries(string filePath)
        {
            var logEntries = new List<ErrorLogEntry>();
            if (File.Exists(filePath) == false) return logEntries;
            var lines = File.ReadAllLines(filePath);

            var regex = new Regex(@"\[(?<time>[0-9:\.]+)\],(?<type>\w+)\s*,(?<source>.{0,180}),\[(?<errorcode>\d+)\]\s*(?<message>.+)");

            foreach (var line in lines)
            {
                var match = regex.Match(line);

                if (int.TryParse(match.Groups["errorcode"].Value.Trim(), out int errCode) == false)
                {
                    continue;
                }

                if (match.Success && (match.Groups["type"].Value.Trim() == "ERROR" || match.Groups["type"].Value.Trim() == "WARN"))
                {
                    var logEntry = new ErrorLogEntry()
                    {
                        Type = match.Groups["type"].Value.Trim(),
                        Timestamp = DateTime.Parse(match.Groups["time"].Value.Trim()).ToString("HH:mm:ss"),
                        ErrorCode = errCode,
                        Message = match.Groups["message"].Value.Trim(),
                        IOName = GetIoNameFromErrorCode(errCode),
                    };

                    if (SelectedErrorLogCount != null)
                    {
                        logEntry.IsHightlight = logEntry.ErrorCode == SelectedErrorLogCount.ErrorCode;
                    }

                    logEntries.Add(logEntry);
                }
            }
            return logEntries;
        }

        private static string GetIoNameFromErrorCode(int errorCode)
        {
            if (Enum.IsDefined(typeof(EWarning), errorCode))
            {
                return GetEnumDescription((EWarning)errorCode);
            }

            if (Enum.IsDefined(typeof(EAlarm), errorCode))
            {
                return GetEnumDescription((EAlarm)errorCode);
            }

            return string.Empty;
        }

        private static string GetEnumDescription<TEnum>(TEnum enumValue) where TEnum : struct, Enum
        {
            MemberInfo[] memberInfo = typeof(TEnum).GetMember(enumValue.ToString());
            if (memberInfo.Length == 0)
            {
                return string.Empty;
            }

            DescriptionAttribute? descriptionAttribute = memberInfo[0].GetCustomAttribute<DescriptionAttribute>();
            return descriptionAttribute?.Description ?? string.Empty;
        }

        private ObservableCollection<ErrorLogCount> LoadLogCounts(ObservableCollection<ErrorLogEntry> errorLogEntries)
        {
            var logCounts = new ObservableCollection<ErrorLogCount>();
            var result = errorLogEntries
                .GroupBy(e => new { e.ErrorCode, e.Message , e.IOName })
                .Select(g => new ErrorLogCount
                {
                    ErrorCode = g.Key.ErrorCode,
                    Message = g.Key.Message,
                    IOName = g.Key.IOName,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count);
            foreach (var logEntry in result)
            {
                logCounts.Add(logEntry);
            }
            return logCounts;
        }
    }
}
