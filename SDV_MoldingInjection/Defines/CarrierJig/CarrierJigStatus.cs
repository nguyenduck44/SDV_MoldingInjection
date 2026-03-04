using CommunityToolkit.Mvvm.ComponentModel;

namespace SDV_MoldingInjection.Defines
{
    public class CarrierJigStatus : ObservableObject
    {
		public event Action<bool> HeadSkipChanged;
        public string Name { get; set; }

        private double resinWeight;
        private double injectTime;
        private double dalayTime;
        private double pAxisInjectVelocity;
        private bool headSkip;

        public double ResinWeight
		{
			get { return resinWeight; }
			set 
			{
				resinWeight = value;
				OnPropertyChanged();
			}
		}

		public double InjectTime
		{
			get { return injectTime; }
			set 
			{
				injectTime = value;
				OnPropertyChanged();
			}
		}

		public double DelayTime
		{
			get { return dalayTime; }
			set 
			{
                dalayTime = value;
				OnPropertyChanged();
			}
		}

		public double PAxisInjectVelocity
		{
			get { return pAxisInjectVelocity; }
			set 
			{
				pAxisInjectVelocity = value;
				OnPropertyChanged();
            }
		}

		public bool HeadSkip
		{
			get { return headSkip; }
			set 
			{
				if(headSkip == value) return;

                headSkip = value;
                HeadSkipChanged?.Invoke(value);
                OnPropertyChanged();
            }
		}
	}
}
