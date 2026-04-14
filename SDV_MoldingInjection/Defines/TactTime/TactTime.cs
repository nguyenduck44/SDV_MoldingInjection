using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows;

namespace SDV_MoldingInjection.Defines
{
    public class TactTime : ObservableObject
    {
        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<double> CycleTimes { get; }
        public ObservableCollection<double> TactTimes { get; }

        public TactTime(string name)
        {
            Name = name;
            CycleTimes = new ObservableCollection<double>();
            for (int i = 0; i < 15; i++)
            {
                CycleTimes.Add(0.0);
            }

            TactTimes = new ObservableCollection<double>();
            for (int i = 0; i < 15; i++)
            {
                TactTimes.Add(0.0);
            }
        }

        public int CycleTimeCounter { get; set; }
        public int TactTimeCounter { get; set; }

        public double AverageCycleTime
        {
            get
            {
                double sum = 0.0;
                foreach (var item in CycleTimes)
                {
                    if (item > 0.0) sum += item;
                }

                if (CycleTimes.Any(t => t > 0.0) == false) return 0.0;
                return sum / CycleTimes.Count(t => t > 0.0);
            }
        }

        public double AverageTactTime
        {
            get
            {
                double sum = 0.0;
                foreach (var item in TactTimes)
                {
                    if (item > 0.0) sum += item;
                }

                if (TactTimes.Any(t => t > 0.0) == false) return 0.0;
                return sum / TactTimes.Count(t => t > 0.0);
            }
        }

        public void SetCycleTime()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                double CycleCurrent = (Environment.TickCount - CycleTimeCounter) / 1000.0;
                CycleTimes.Add(CycleCurrent);
                if (CycleTimes.Count > 15)
                {
                    CycleTimes.RemoveAt(0);
                }
                CycleTimeCounter = Environment.TickCount;

                OnPropertyChanged(nameof(AverageCycleTime));
            });
        }

        public void SetTactTime()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                double CycleCurrent = (Environment.TickCount - TactTimeCounter) / 1000.0;
                TactTimes.Add(CycleCurrent);
                if (TactTimes.Count > 15)
                {
                    TactTimes.RemoveAt(0);
                }
            });

            OnPropertyChanged(nameof(AverageTactTime));
        }

        public void ResetCycleTime()
        {
            for (int i = 0; i < CycleTimes.Count; i++)
            {
                CycleTimes[i] = 0.0;
            }

            OnPropertyChanged(nameof(AverageCycleTime));
        }

        public void ResetTactTime()
        {
            for (int i = 0; i < TactTimes.Count; i++)
            {
                TactTimes[i] = 0.0;
            }

            OnPropertyChanged(nameof(AverageTactTime));
        }
    }
}
