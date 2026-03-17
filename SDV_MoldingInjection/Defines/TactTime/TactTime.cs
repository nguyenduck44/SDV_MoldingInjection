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

        public ObservableCollection<double> TactTimes { get; }

        public TactTime(string name)
        {
            Name = name;
            TactTimes = new ObservableCollection<double>();
            for (int i = 0; i < 15; i++)
            {
                TactTimes.Add(0.0);
            }
        }

        public int TaktTimeCounter { get; set; }

        public double Average
        {
            get
            {
                double sum = 0.0;
                foreach (var item in TactTimes)
                {
                    if (item > 0.0) sum += item;
                }

                if (TactTimes.Count(t => t > 0.0) <= 0) return 0.0;
                return sum / TactTimes.Count(t => t > 0.0);
            }
        }

        public void SetTaktTime()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                double CycleCurrent = (Environment.TickCount - TaktTimeCounter) / 1000.0;
                TactTimes.Add(CycleCurrent);
                if (TactTimes.Count > 15)
                {
                    TactTimes.RemoveAt(0);
                }
                TaktTimeCounter = Environment.TickCount;

                OnPropertyChanged(nameof(Average));
            });
        }

        public void Reset()
        {
            for (int i = 0; i < TactTimes.Count; i++)
            {
                TactTimes[i] = 0.0;
            }

            OnPropertyChanged(nameof(Average));
        }
    }
}
