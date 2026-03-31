using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace SDV_MoldingInjection.Defines
{
    public class SyringeAmountStatus : ObservableObject
    {
        private double maxVolume = 1000.0;
        private double remainVolume;
        private DateTime resetTime = DateTime.Now;
        private TimeSpan pausedElapsedTime = TimeSpan.Zero;
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

        public TimeSpan ElapsedTime => IsHeadAvailable ? DateTime.Now - ResetTime : pausedElapsedTime;

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

                if (!value)
                {
                    pausedElapsedTime = DateTime.Now - ResetTime;
                }
                else
                {
                    resetTime = DateTime.Now - pausedElapsedTime;
                }

                isHeadAvailable = value;
    #if SIMULATION
                    isHeadAvailable = true;
    #endif
                OnPropertyChanged();
                OnPropertyChanged(nameof(ElapsedTime));
            }
        }

        public void Reset()
        {
            RemainVolume = MaxVolume;
            pausedElapsedTime = TimeSpan.Zero;
            ResetTime = DateTime.Now;
        }

        public void UpdateElapsedTime()
        {
            if (!IsHeadAvailable) return;
            OnPropertyChanged(nameof(ElapsedTime));
        }
    }
}
