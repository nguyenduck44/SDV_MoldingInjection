using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace SDV_MoldingInjection.Defines.Devices
{
    public class SyringeAmountStatus : ObservableObject
    {
        private double maxVolume = 1000.0;
        private double remainVolume;
        private DateTime resetTime = DateTime.Now;
        private bool isTimeOver;
        private bool isHeadAvailable = true;

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

        public double RemainPercent
        {
            get
            {
                if (MaxVolume <= 0) return 0;
                return RemainVolume / MaxVolume * 120.0;
            }
        }

        public DateTime ResetTime
        {
            get => resetTime;
            set
            {
                resetTime = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan ElapsedTime => DateTime.Now - ResetTime;

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

        [JsonIgnore]
        public bool IsHeadAvailable
        {
            get => isHeadAvailable;
            set
            {
                if (isHeadAvailable == value) return;
                isHeadAvailable = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            RemainVolume = MaxVolume;
            ResetTime = DateTime.Now;
        }

        public void UpdateElapsedTime()
        {
            OnPropertyChanged(nameof(ElapsedTime));
        }
    }
}
