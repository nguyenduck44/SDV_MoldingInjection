using EQX.Core.Common;
using SDV_MoldingInjection.Defines;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AnalogStatusViewModel : ViewModelBase
    {
        private readonly NonOverlappingTimer statusUpdateTimer;

        public AnalogStatusViewModel(CDAStatus cDAStatus)
        {
            CDAStatus = cDAStatus;
            statusUpdateTimer = new NonOverlappingTimer(1000);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }

        public CDAStatus CDAStatus { get; }

        public double CDA => CDAStatus.MainAirCDA;
        #region Private Methods
        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {
            OnPropertyChanged(nameof(CDA));
        }
        #endregion
    }
}
