using EQX.Core.LogHistory;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.Process;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

public class LogLevelItem
{
    public string Name { get; set; }
    public string Value { get; set; }
    public Brush Color { get; set; }
    public override string ToString() => Name;
}

public enum FilterState
{
    AllSelected,
    MultipleSelected,
    SingleSelected,
    NoneSelected
}

public class SourceItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Name { get; set; }
    public string Value { get; set; }
    public Brush Color { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public override string ToString() => Name;

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

namespace SDV_MoldingInjection.MVVM.Views
{
    public partial class LogView : UserControl
    {
        private List<LogEntry> currentLogEntries;
        private ScrollViewer scrollViewer;

        public LogView() => InitializeComponent();

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is not LogViewModel viewModel) return;

            viewModel.LoadLogFiles();
            InitializeFilterTypeComboBox();
            //InitializeFilterSourceComboBox();
        }

        private void InitializeFilterTypeComboBox()
        {
            var filterTypes = new[]
            {
                new LogLevelItem { Name = "All", Value = "ALL", Color = Brushes.Black },
                new LogLevelItem { Name = "DEBUG", Value = "DEBUG", Color = Brushes.Gray },
                new LogLevelItem { Name = "INFO", Value = "INFO", Color = new SolidColorBrush(Color.FromRgb(33, 150, 243)) },
                new LogLevelItem { Name = "WARN", Value = "WARN", Color = new SolidColorBrush(Color.FromRgb(255, 152, 0)) },
                new LogLevelItem { Name = "ERROR", Value = "ERROR", Color = new SolidColorBrush(Color.FromRgb(244, 67, 54)) },
                new LogLevelItem { Name = "FATAL", Value = "FATAL", Color = new SolidColorBrush(Color.FromRgb(156, 39, 176)) }
            };

            FilterTypeComboBox.ItemsSource = filterTypes;
            FilterTypeComboBox.SelectedIndex = 0;
        }

        private void LogTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (DataContext is not LogViewModel viewModel) return;

            try
            {
                if (LogTreeView.SelectedItem is FileSystemNode selectedNode && !selectedNode.IsDirectory)
                {
                    currentLogEntries = viewModel.LoadLogEntries(selectedNode.Path);
                    LogDataGrid.ItemsSource = currentLogEntries;
                    InitializeFilterSourceComboBox();

                    ApplyFilter();
                }
                else
                {
                    LogDataGrid.ItemsSource = null;
                    FilterSourceComboBox.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void InitializeFilterSourceComboBox()
        {
            List<string> sources = new List<string>();
            foreach (var log in currentLogEntries)
            {
                sources.Add(log.Source);
            }

            var hashSetSources = sources.ToHashSet();

            var sourceItems = new List<SourceItem> { new SourceItem { Name = "All", Value = "All", IsSelected = true } };

            sourceItems.AddRange(hashSetSources
                .Select(source => new SourceItem { Name = source, Value = source, IsSelected = false }));

            FilterSourceComboBox.ItemsSource = sourceItems;
        }

        private void FilterTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

        private void FilterSourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilter();

        private void FilterSourceCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (FilterSourceComboBox?.ItemsSource is not List<SourceItem> sourceItems) return;

            // Tìm ScrollViewer nếu chưa có hoặc ComboBox đang mở
            if (scrollViewer == null || !FilterSourceComboBox.IsDropDownOpen)
            {
                scrollViewer = FindScrollViewer(FilterSourceComboBox);
            }

            var checkbox = sender as CheckBox;
            var sourceItem = checkbox?.DataContext as SourceItem;

            if (sourceItem == null) return;

            // Xử lý logic chọn/bỏ chọn
            ProcessSelectionLogic(sourceItem, sourceItems);

            // Kiểm tra trường hợp đặc biệt: All + items khác
            HandleSpecialCase(sourceItems);

            // Refresh UI và cập nhật hiển thị
            RefreshUI(sourceItems);
            ApplyFilter();
        }

        private void ProcessSelectionLogic(SourceItem sourceItem, List<SourceItem> sourceItems)
        {
            var allItem = sourceItems.FirstOrDefault(s => s.Value == "All");
            var otherItems = sourceItems.Where(s => s.Value != "All").ToList();

            if (sourceItem.Value == "All" && sourceItem.IsSelected)
            {
                // Chọn All -> bỏ chọn tất cả khác
                otherItems.ForEach(item => item.IsSelected = false);
            }
            else if (sourceItem.Value != "All" && sourceItem.IsSelected)
            {
                // Chọn item khác -> bỏ chọn All
                allItem.IsSelected = false;
            }
            else if (sourceItem.Value != "All" && !sourceItem.IsSelected)
            {
                // Bỏ chọn item cuối -> chọn All
                if (!otherItems.Any(item => item.IsSelected))
                    allItem.IsSelected = true;
            }
        }

        private void HandleSpecialCase(List<SourceItem> sourceItems)
        {
            var allItem = sourceItems.FirstOrDefault(s => s.Value == "All");
            var otherSelectedItems = sourceItems.Where(s => s.IsSelected && s.Value != "All").ToList();

            if (allItem?.IsSelected == true && otherSelectedItems.Any())
                allItem.IsSelected = false;
        }

        private void RefreshUI(List<SourceItem> sourceItems)
        {
            // Chỉ cập nhật hiển thị ComboBox, không refresh ItemsSource
            UpdateComboBoxDisplay(sourceItems);
        }

        private FilterState GetFilterState(List<SourceItem> sourceItems)
        {
            var selectedSources = sourceItems.Where(s => s.IsSelected).ToList();
            var hasAllSelected = selectedSources.Any(s => s.Value == "All");

            return selectedSources.Count switch
            {
                _ when hasAllSelected => FilterState.AllSelected,
                > 1 => FilterState.MultipleSelected,
                1 => FilterState.SingleSelected,
                _ => FilterState.NoneSelected
            };
        }

        private void UpdateComboBoxDisplay(List<SourceItem> sourceItems)
        {
            var filterState = GetFilterState(sourceItems);

            // Lưu vị trí scroll hiện tại
            double scrollOffset = 0;
            if (scrollViewer != null)
            {
                scrollOffset = scrollViewer.VerticalOffset;
            }

            // Cập nhật hiển thị ComboBox - không hiển thị Multi và None
            switch (filterState)
            {
                case FilterState.AllSelected:
                    FilterSourceComboBox.SelectedItem = sourceItems.First(s => s.Value == "All");
                    break;

                case FilterState.MultipleSelected:
                    // Hiển thị item đầu tiên được chọn thay vì "Multi"
                    FilterSourceComboBox.SelectedItem = sourceItems.First(s => s.IsSelected && s.Value != "All");
                    break;

                case FilterState.SingleSelected:
                    FilterSourceComboBox.SelectedItem = sourceItems.First(s => s.IsSelected);
                    break;

                case FilterState.NoneSelected:
                    // Hiển thị "All" khi không có item nào được chọn
                    FilterSourceComboBox.SelectedItem = sourceItems.First(s => s.Value == "All");
                    break;
            }

            // Khôi phục vị trí scroll sau khi cập nhật SelectedItem
            if (scrollViewer != null && scrollOffset > 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (scrollViewer != null)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollOffset);
                    }
                }), DispatcherPriority.Background);
            }
        }

        private void ApplyFilter()
        {
            if (currentLogEntries == null || LogDataGrid == null) return;

            var collectionView = CollectionViewSource.GetDefaultView(currentLogEntries);
            collectionView.Filter = item =>
            {
                var entry = item as LogEntry;
                var selectedFilterType = FilterTypeComboBox.SelectedItem as LogLevelItem;

                // Filter theo Type
                bool filterTypeMatch = selectedFilterType?.Value == "ALL" || entry.Type == selectedFilterType?.Value;

                // Filter theo Source
                bool filterSourceMatch = GetSourceFilterMatch(entry);

                return filterTypeMatch && filterSourceMatch;
            };

            LogDataGrid.ItemsSource = collectionView;
        }

        private bool GetSourceFilterMatch(LogEntry entry)
        {
            if (FilterSourceComboBox?.ItemsSource is not List<SourceItem> sourceItems) return true;

            var selectedSources = sourceItems.Where(s => s.IsSelected && s.Value != "Multi" && s.Value != "None").ToList();
            var filterState = GetFilterState(selectedSources);

            return filterState switch
            {
                FilterState.AllSelected => true,
                FilterState.MultipleSelected or FilterState.SingleSelected => selectedSources.Any(s => s.Value == entry.Source),
                FilterState.NoneSelected => false,
                _ => true
            };
        }

        private ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            if (parent == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // Tìm ScrollViewer trong Popup của ComboBox
                if (child is ScrollViewer scrollViewer)
                {
                    return scrollViewer;
                }

                // Đệ quy tìm trong các child elements
                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        private async void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ErrorListHistoryDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {

        }
    }
}