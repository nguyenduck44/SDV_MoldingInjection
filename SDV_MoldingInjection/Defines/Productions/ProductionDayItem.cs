using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.Defines.Productions
{
    public class ProductionDayItem : ObservableObject
    {
        public DateTime Date { get; set; }
        public ObservableCollection<ProductionHourItem> ProductionHours { get; set; }
        public ObservableCollection<ProductionHourItem> ProductionHoursLeft { get; set; }
        public ObservableCollection<ProductionHourItem> ProductionHoursRight { get; set; }

        private static ObservableCollection<ProductionHourItem> CreateHourCollection()
        {
            var hours = new ObservableCollection<ProductionHourItem>();
            for (int i = 8; i <= 23; i++) hours.Add(new ProductionHourItem(i));
            for (int i = 0; i <= 7; i++) hours.Add(new ProductionHourItem(i));
            return hours;
        }

        public ProductionDayItem()
        {
            Date = DateTime.Now;
            ProductionHours = CreateHourCollection();
            ProductionHoursLeft = CreateHourCollection();
            ProductionHoursRight = CreateHourCollection();
            SubscribeCountChanged();
        }

        [JsonConstructor]
        public ProductionDayItem(DateTime date,
            ObservableCollection<ProductionHourItem> productionHours,
            ObservableCollection<ProductionHourItem>? productionHoursLeft,
            ObservableCollection<ProductionHourItem>? productionHoursRight)
        {
            Date = date;
            ProductionHours = productionHours ?? CreateHourCollection();
            ProductionHoursLeft = productionHoursLeft ?? CreateHourCollection();
            ProductionHoursRight = productionHoursRight ?? CreateHourCollection();
        }

        public void SubscribeCountChanged()
        {
            foreach (var hour in ProductionHours)
            {
                hour.InputCountChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(TotalInput));
                    OnPropertyChanged(nameof(TotalInputShiftDay));
                    OnPropertyChanged(nameof(TotalInputShiftNight));
                };
                hour.OutputCountChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(TotalOutput));
                    OnPropertyChanged(nameof(TotalOutputShiftDay));
                    OnPropertyChanged(nameof(TotalOutputShiftNight));
                };
            }

            foreach (var hour in ProductionHoursLeft)
            {
                hour.InputCountChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(TotalInputLeft));
                    OnPropertyChanged(nameof(TotalInputLeftShiftDay));
                    OnPropertyChanged(nameof(TotalInputLeftShiftNight));
                };
                hour.OutputCountChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(TotalOutputLeft));
                    OnPropertyChanged(nameof(TotalOutputLeftShiftDay));
                    OnPropertyChanged(nameof(TotalOutputLeftShiftNight));
                };
            }

            foreach (var hour in ProductionHoursRight)
            {
                hour.InputCountChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(TotalInputRight));
                    OnPropertyChanged(nameof(TotalInputRightShiftDay));
                    OnPropertyChanged(nameof(TotalInputRightShiftNight));
                };
                hour.OutputCountChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(TotalOutputRight));
                    OnPropertyChanged(nameof(TotalOutputRightShiftDay));
                    OnPropertyChanged(nameof(TotalOutputRightShiftNight));
                };
            }
        }

        // ── Original combined totals (backward compat) ────────────────────────────
        [JsonIgnore] public int TotalInput => ProductionHours.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutput => ProductionHours.Sum(i => i.OutputCount);

        [JsonIgnore] public ObservableCollection<ProductionHourItem> ShiftDay => new(ProductionHours.SkipLast(12));
        [JsonIgnore] public ObservableCollection<ProductionHourItem> ShiftNight => new(ProductionHours.Skip(12));

        [JsonIgnore] public int TotalInputShiftDay => ShiftDay.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputShiftDay => ShiftDay.Sum(i => i.OutputCount);

        [JsonIgnore] public int TotalInputShiftNight => ShiftNight.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputShiftNight => ShiftNight.Sum(i => i.OutputCount);

        // ── Left side ──────────────────────────────────────────────────────────────
        [JsonIgnore] public ObservableCollection<ProductionHourItem> AllShiftLeft => ProductionHoursLeft;
        [JsonIgnore] public int TotalInputLeft => AllShiftLeft.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputLeft => AllShiftLeft.Sum(i => i.OutputCount);

        [JsonIgnore] public ObservableCollection<ProductionHourItem> ShiftDayLeft => new(ProductionHoursLeft.SkipLast(12));
        [JsonIgnore] public ObservableCollection<ProductionHourItem> ShiftNightLeft => new(ProductionHoursLeft.Skip(12));

        [JsonIgnore] public int TotalInputLeftShiftDay => ShiftDayLeft.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputLeftShiftDay => ShiftDayLeft.Sum(i => i.OutputCount);

        [JsonIgnore] public int TotalInputLeftShiftNight => ShiftNightLeft.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputLeftShiftNight => ShiftNightLeft.Sum(i => i.OutputCount);

        // ── Right side ─────────────────────────────────────────────────────────────
        [JsonIgnore] public ObservableCollection<ProductionHourItem> AllShiftRight => ProductionHoursRight;
        [JsonIgnore] public int TotalInputRight => AllShiftRight.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputRight => AllShiftRight.Sum(i => i.OutputCount);

        [JsonIgnore] public ObservableCollection<ProductionHourItem> ShiftDayRight => new(ProductionHoursRight.SkipLast(12));
        [JsonIgnore] public ObservableCollection<ProductionHourItem> ShiftNightRight => new(ProductionHoursRight.Skip(12));

        [JsonIgnore] public int TotalInputRightShiftDay => ShiftDayRight.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputRightShiftDay => ShiftDayRight.Sum(i => i.OutputCount);

        [JsonIgnore] public int TotalInputRightShiftNight => ShiftNightRight.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputRightShiftNight => ShiftNightRight.Sum(i => i.OutputCount);
    }
}
