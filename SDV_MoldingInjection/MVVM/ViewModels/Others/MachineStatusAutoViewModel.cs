using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MachineStatusAutoViewModel : ViewModelBase
    {
        #region Privates
        private bool h1Working;
        private bool h2Working;
        private bool h3Working;
        private bool h4Working;
        private readonly NavigationStore _navigationStore;
        private readonly RecipeSelector _recipeSelector;
        private readonly NonOverlappingTimer statusUpdateTimer;
        #endregion
        #region Properties
        public Devices Devices { get; }
        public double PanelTemperature => Devices.PanelIndicator.Temperature;
        public double PanelHumidity => Devices.PanelIndicator.Humidity;

        public bool H1Working
        {
            get { return h1Working; }
            set
            {
                h1Working = value;
                OnPropertyChanged();
            }
        }

        public bool H2Working
        {
            get { return h2Working; }
            set
            {
                h2Working = value;
                OnPropertyChanged();
            }
        }

        public bool H3Working
        {
            get { return h3Working; }
            set
            {
                h3Working = value;
                OnPropertyChanged();
            }
        }

        public bool H4Working
        {
            get { return h4Working; }
            set
            {
                h4Working = value;
                OnPropertyChanged();
            }
        }

        public MachineStatus MachineStatus { get; }
        #endregion
        #region Contructors
        public MachineStatusAutoViewModel(Devices devices,
            NavigationStore navigationStore,
            MachineStatus machineStatus,
            RecipeSelector recipeSelector)
        {
            Devices = devices;
           _navigationStore = navigationStore;
            MachineStatus = machineStatus;
            _recipeSelector = recipeSelector;

            statusUpdateTimer = new NonOverlappingTimer(100);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }
        #endregion

        #region Private Methods
        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {

            var current = _navigationStore.CurrentViewModel;
            if (current != this && current is not AutoViewModel && current is not OriginViewModel)
                return;
            Devices.Inputs.DoorOpenLeft.RaiseValueUpdated();
            Devices.Inputs.DoorReleaseLeft.RaiseValueUpdated();
            Devices.Inputs.DoorOpenRight.RaiseValueUpdated();
            Devices.Inputs.DoorReleaseRight.RaiseValueUpdated();
            Devices.Inputs.DoorOpenRearLeft.RaiseValueUpdated();
            Devices.Inputs.DoorReleaseRearLeft.RaiseValueUpdated();
            Devices.Inputs.DoorOpenRearRight.RaiseValueUpdated();
            Devices.Inputs.DoorReleaseRearRight.RaiseValueUpdated();
            Devices.Inputs.PanelCloseLeftCheck.RaiseValueUpdated();
            Devices.Inputs.PanelCloseRightCheck.RaiseValueUpdated();
            Devices.Inputs.ChamberOpen.RaiseValueUpdated();
            Devices.Inputs.Jig1Detect.RaiseValueUpdated();
            Devices.Inputs.Jig2Detect.RaiseValueUpdated();
            Devices.Inputs.Jig3Detect.RaiseValueUpdated();
            Devices.Inputs.Jig4Detect.RaiseValueUpdated();
            Devices.Inputs.Emergency.RaiseValueUpdated();
            Devices.Inputs.OPButtonStart.RaiseValueUpdated();
            Devices.Inputs.OPButtonStop.RaiseValueUpdated();
            Devices.Inputs.OPButtonReset.RaiseValueUpdated();
            Devices.Inputs.H1_CylDown.RaiseValueUpdated();
            Devices.Inputs.H2_CylDown.RaiseValueUpdated();
            Devices.Inputs.H3_CylDown.RaiseValueUpdated();
            Devices.Inputs.H4_CylDown.RaiseValueUpdated();
            Devices.Inputs.H1_SyringeCheck.RaiseValueUpdated();
            Devices.Inputs.H2_SyringeCheck.RaiseValueUpdated();
            Devices.Inputs.H3_SyringeCheck.RaiseValueUpdated();
            Devices.Inputs.H4_SyringeCheck.RaiseValueUpdated();
            Devices.Inputs.BelowsUp.RaiseValueUpdated();
            Devices.Inputs.DryPumpRun.RaiseValueUpdated();

            H1Working = Devices.Motions.Z1Axis.Status.IsMotioning || Devices.Motions.P1Axis.Status.IsMotioning;
            H2Working = Devices.Motions.Z2Axis.Status.IsMotioning || Devices.Motions.P2Axis.Status.IsMotioning;
            H3Working = Devices.Motions.Z3Axis.Status.IsMotioning || Devices.Motions.P3Axis.Status.IsMotioning;
            H4Working = Devices.Motions.Z4Axis.Status.IsMotioning || Devices.Motions.P4Axis.Status.IsMotioning;
            OnPropertyChanged(nameof(PanelTemperature));
            OnPropertyChanged(nameof(PanelHumidity));
        }
        #endregion
    }
}
