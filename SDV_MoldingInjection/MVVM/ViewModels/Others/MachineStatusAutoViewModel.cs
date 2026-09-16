using EQX.Core.Common;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Windows;

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
        private readonly IServiceProvider _serviceProvider;
        private readonly Config _config;
        private readonly NonOverlappingTimer statusUpdateTimer;
        private Processes _processes => _serviceProvider.GetRequiredService<Processes>();
        private InjectProcess InjectProcess => _processes.All.OfType<InjectProcess>().First();
        #endregion

        #region Properties
        public Devices Devices { get; }
        public string LeftJigID => InjectProcess.LeftJigID;
        public string RightJigID => InjectProcess.RightJigID;
        public double PanelTemperature => Devices.PanelIndicator.Temperature;
        public double PanelHumidity => Devices.PanelIndicator.Humidity;
        public double MainPowerTemperature => _config.IsConnectEBoxMainPower ? Devices.EBoxMainPowerIndicator.Temperature : 0.0;
        public double MainPowerHumidity => _config.IsConnectEBoxMainPower ? Devices.EBoxMainPowerIndicator.Humidity : 0.0;
        public bool IsLeakSensor_H1 => Devices.Inputs.DummyOverflowDetect1.Value == false;
        public bool IsLeakSensor_H2 => Devices.Inputs.DummyOverflowDetect2.Value == false;
        public bool IsLeakSensor_H3 => Devices.Inputs.DummyOverflowDetect3.Value == false;
        public bool IsLeakSensor_H4 => Devices.Inputs.DummyOverflowDetect4.Value == false;
        public bool IsLeftJigDetected => Devices.Inputs.Jig1Detect.Value && Devices.Inputs.Jig2Detect.Value == true;
        public bool IsRightJigDetected => Devices.Inputs.Jig3Detect.Value && Devices.Inputs.Jig4Detect.Value == true;
        public bool IsConnectMainPower => _config.IsConnectEBoxMainPower;

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
        public RecipeSelector RecipeSelector { get; }
        public CDAStatus CDAStatus { get; }
        #endregion

        #region Contructors
        public MachineStatusAutoViewModel(Devices devices,
            NavigationStore navigationStore,
            MachineStatus machineStatus,
            RecipeSelector recipeSelector,
            CDAStatus cDAStatus,
            IServiceProvider serviceProvider,
            Config config)
        {
            Devices = devices;
            MachineStatus = machineStatus;
            RecipeSelector = recipeSelector;
            CDAStatus = cDAStatus;
            _navigationStore = navigationStore;
            _serviceProvider = serviceProvider;
            _config = config;
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

            OnPropertyChanged(nameof(IsLeakSensor_H1));
            OnPropertyChanged(nameof(IsLeakSensor_H2));
            OnPropertyChanged(nameof(IsLeakSensor_H3));
            OnPropertyChanged(nameof(IsLeakSensor_H4));
            OnPropertyChanged(nameof(LeftJigID));
            OnPropertyChanged(nameof(RightJigID));
            OnPropertyChanged(nameof(IsLeftJigDetected));
            OnPropertyChanged(nameof(IsRightJigDetected));

            if (_config.IsConnectEBoxMainPower)
            {
                OnPropertyChanged(nameof(MainPowerTemperature));
                OnPropertyChanged(nameof(MainPowerHumidity));
            }
        }
        #endregion
    }
}
