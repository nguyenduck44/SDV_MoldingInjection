using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SDV_MoldingInjection.Defines.Devices
{
    public class SyringeAmountStatus : ObservableObject
    {
        private double maxVolume = 1000.0;
        private double remainVolume;
        private DateTime resetTime = DateTime.Now;
        private bool isTimeOver;

        public double MaxVolume
        {
            get { return maxVolume; }
            set
            {
                maxVolume = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainPercent));
            }
        }

        public double RemainVolume
        {
            get { return remainVolume; }
            set
            {
                remainVolume = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainPercent));
            }
        }

        [JsonIgnore]
        public double RemainPercent
        {
            get
            {
                if (MaxVolume <= 0) return 0;
                return RemainVolume / MaxVolume * 120.0;
            }
        }

        [JsonIgnore]
        public DateTime ResetTime
        {
            get => resetTime;
            set
            {
                resetTime = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public bool IsTimeOver
        {
            get => isTimeOver;
            set
            {
                if (isTimeOver == value) return;
                isTimeOver = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            RemainVolume = MaxVolume;
            ResetTime = DateTime.Now;
        }

        #region Persistence helpers
        private const string SyringeStateFileName = "SyringeState.json";

        private class SyringeState
        {
            public DateTime[] ResetTimes { get; set; } = Array.Empty<DateTime>();
        }

        public static void SaveStates(SyringeAmountStatus[] statuses)
        {
            if (statuses == null || statuses.Length == 0) return;

            var state = new SyringeState
            {
                ResetTimes = statuses
                    .Select(s => s?.ResetTime ?? DateTime.Now)
                    .ToArray()
            };

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SyringeStateFileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(filePath, json);
        }

        public static void LoadStates(SyringeAmountStatus[] statuses)
        {
            if (statuses == null || statuses.Length == 0) return;

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SyringeStateFileName);
            if (!File.Exists(filePath)) return;

            var json = File.ReadAllText(filePath);
            var state = JsonSerializer.Deserialize<SyringeState>(json);
            if (state?.ResetTimes == null) return;

            int count = Math.Min(state.ResetTimes.Length, statuses.Length);
            for (int i = 0; i < count; i++)
            {
                var time = state.ResetTimes[i];
                statuses[i].ResetTime = time == default ? DateTime.Now : time;
            }
        }
        #endregion
    }
}
