using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace SDV_MoldingInjection.Defines.Productions
{
    public class ProductionHourItem : ObservableObject
    {
        [JsonIgnore]
        public EventHandler InputCountChanged;
        [JsonIgnore]
        public EventHandler OutputCountChanged;

        private int inputCount;
        private int outputCount;

        public int Hour { get; set; }

        public ProductionHourItem(int hour)
        {
            Hour = hour;
        }
        public int InputCount
        {
            get { return inputCount; }
            set
            {
                inputCount = value;
                OnPropertyChanged();
                InputCountChanged?.Invoke(value, EventArgs.Empty);
            }
        }

        public int OutputCount
        {
            get { return outputCount; }
            set
            {
                outputCount = value;
                OnPropertyChanged();
                OutputCountChanged?.Invoke(value, EventArgs.Empty);
            }
        }
    }
}
