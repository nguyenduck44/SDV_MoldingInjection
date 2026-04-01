using CommunityToolkit.Mvvm.ComponentModel;

namespace SDV_MoldingInjection.MVVM.Models
{
    public class InjectPath : ObservableObject
    {
        public bool IsEnabled => VelocityRate > 0;

        public double VelocityRate
        {
            get { return _velocityRate; }
            set
            {
                _velocityRate = value;
                OnPropertyChanged(nameof(VelocityRate));
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        /// <summary>
        /// Injection time for current InjectionPath in second unit
        /// </summary>
        public double TimeInSecond
        {
            get => _timeInSecond;
            set
            {
                _timeInSecond = value;
                OnPropertyChanged(nameof(TimeInSecond));
            }
        }

        #region Privates
        private double _velocityRate;
        private double _timeInSecond;
        #endregion
    }
}
