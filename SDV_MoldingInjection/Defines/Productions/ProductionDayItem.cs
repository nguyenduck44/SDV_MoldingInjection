using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.Defines.Productions
{
    public class ProductionDayItem : ObservableObject
    {
        public DateTime Date { get; set; }
        public ObservableCollection<ProductionHourItem> ProductionHours { get; set; }

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
            SubscribeCountChanged();
        }

        /// <summary>
        /// Tham số Left/Right chỉ dùng khi đọc JSON cũ; dữ liệu được cộng vào <see cref="ProductionHours"/>.
        /// </summary>
        [JsonConstructor]
        public ProductionDayItem(DateTime date,
            ObservableCollection<ProductionHourItem> productionHours,
            ObservableCollection<ProductionHourItem>? productionHoursLeft,
            ObservableCollection<ProductionHourItem>? productionHoursRight)
        {
            Date = date;
            ProductionHours = productionHours ?? CreateHourCollection();
            if (productionHoursLeft != null || productionHoursRight != null)
                MergeLegacySideData(ProductionHours, productionHoursLeft, productionHoursRight);
        }

        private static void MergeLegacySideData(
            ObservableCollection<ProductionHourItem> combined,
            ObservableCollection<ProductionHourItem>? left,
            ObservableCollection<ProductionHourItem>? right)
        {
            foreach (var ph in combined)
            {
                var l = left?.FirstOrDefault(x => x.Hour == ph.Hour);
                var r = right?.FirstOrDefault(x => x.Hour == ph.Hour);
                ph.InputCount += (l?.InputCount ?? 0) + (r?.InputCount ?? 0);
                ph.OutputCount += (l?.OutputCount ?? 0) + (r?.OutputCount ?? 0);
            }
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
        }

        [JsonIgnore] public int TotalInput => ProductionHours.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutput => ProductionHours.Sum(i => i.OutputCount);

        [JsonIgnore] public ObservableCollection<ProductionHourItem> ShiftDay => new(ProductionHours.SkipLast(12));
        [JsonIgnore] public ObservableCollection<ProductionHourItem> ShiftNight => new(ProductionHours.Skip(12));

        [JsonIgnore] public int TotalInputShiftDay => ShiftDay.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputShiftDay => ShiftDay.Sum(i => i.OutputCount);

        [JsonIgnore] public int TotalInputShiftNight => ShiftNight.Sum(i => i.InputCount);
        [JsonIgnore] public int TotalOutputShiftNight => ShiftNight.Sum(i => i.OutputCount);
    }
}
