using EQX.Core.Common;
using log4net;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AutoViewModel : ViewModelBase
    {
        #region Properties
        public Devices Devices { get; }
        #endregion

        #region Contructors
        public AutoViewModel(
            Devices devices,
            NavigationStore navigationStore)
        {
            Devices = devices;
            _navigationStore = navigationStore;

            Log = LogManager.GetLogger("AutoVM");

            statusUpdateTimer = new System.Timers.Timer(100);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }
        #endregion

        #region Private Methods
        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel != this) return;
            Devices.Inputs.DoorOpenLeft.RaiseValueUpdated();
            Devices.Inputs.DoorReleaseLeft.RaiseValueUpdated();
            Devices.Inputs.DoorOpenRight.RaiseValueUpdated();
            Devices.Inputs.DoorReleaseRight.RaiseValueUpdated();
        }
        #endregion

        #region Privates
        private readonly NavigationStore _navigationStore;
        private readonly System.Timers.Timer statusUpdateTimer;
        #endregion
    }
}
