using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using log4net;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AutoViewModel : ViewModelBase
    {
        #region Properties
        public Devices Devices { get; }
        public MachineStatus MachineStatus { get; }
        public SyringAmountStatusList SyringeAmountStatusList { get; }
        public CarrierJigStatusList CarrierJigStatusList { get; }
        public double PanelTemperature => Devices.PanelIndicator.Temperature;
        public double PanelHumidity => Devices.PanelIndicator.Humidity;

        private bool h1Working;
        private bool h2Working;
        private bool h3Working;
        private bool h4Working;

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

        #endregion

        #region Contructors
        public AutoViewModel(
            Devices devices,
            MachineStatus machineStatus,
            NavigationStore navigationStore,
            SyringAmountStatusList syringeAmountStatusList,
            RecipeSelector recipeSelector,
            CarrierJigStatusList carrierJigStatusList)
        {
            Devices = devices;
            MachineStatus = machineStatus;
            _navigationStore = navigationStore;
            SyringeAmountStatusList = syringeAmountStatusList;
            _recipeSelector = recipeSelector;
            CarrierJigStatusList = carrierJigStatusList;
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
            Devices.Inputs.BelowsUp.RaiseValueUpdated();
            Devices.Inputs.DryPumpRun.RaiseValueUpdated();
            Devices.Inputs.PanelClodeCheck.RaiseValueUpdated();

            UpdateSyringeStatus();


            H1Working = Devices.Motions.Z1Axis.Status.IsMotioning || Devices.Motions.P1Axis.Status.IsMotioning || Devices.Motions.G1Axis.Status.IsMotioning;
            H2Working = Devices.Motions.Z2Axis.Status.IsMotioning || Devices.Motions.P2Axis.Status.IsMotioning || Devices.Motions.G2Axis.Status.IsMotioning;
            H3Working = Devices.Motions.Z3Axis.Status.IsMotioning || Devices.Motions.P3Axis.Status.IsMotioning || Devices.Motions.G3Axis.Status.IsMotioning;
            H4Working = Devices.Motions.Z4Axis.Status.IsMotioning || Devices.Motions.P4Axis.Status.IsMotioning || Devices.Motions.G4Axis.Status.IsMotioning;
        }

        private void UpdateSyringeStatus()
        {
            foreach (var syringe in SyringeAmountStatusList.SyringeAmounts)
            {
                syringe.UpdateElapsedTime();
            }
            var recipe = _recipeSelector.CurrentRecipe;
            if (recipe == null) return;

            var common = recipe.CommonRecipe;
            if (common == null) return;

            double maxVolumeG = common.SyringeAmountWeight / 1000.0;
            double limitHours = common.SyringeMountTimeChange;

            for (int i = 0; i < SyringeAmountStatusList.SyringeAmounts.Count; i++)
            {
                var status = SyringeAmountStatusList.SyringeAmounts[i];
                if (status == null) continue;

                if (maxVolumeG > 0 && Math.Abs(status.MaxVolume - maxVolumeG) > 0.0001)
                {
                    status.MaxVolume = maxVolumeG;
                    if (status.RemainVolume <= 0)
                    {
                        status.RemainVolume = maxVolumeG;
                    }
                }

                bool isOver = false;
                if (limitHours > 0)
                {
                    var elapsedHours = (DateTime.Now - status.ResetTime).TotalHours;
                    isOver = elapsedHours >= limitHours;
                }

                status.IsTimeOver = isOver;
            }
        }
        #endregion

        #region Commands
        public ICommand DoorOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Outputs.EQPStop.Value = !Devices.Outputs.EQPStop.Value;
                });
            }
        }

        public ICommand BuzzerOffCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Outputs.Buzzer1On.Value = false;
                    Devices.Outputs.Buzzer2On.Value = false;
                    Devices.Outputs.Buzzer3On.Value = false;
                    Devices.Outputs.Buzzer4On.Value = false;
                });
            }
        }

        public ICommand SafetyKeyCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Outputs.SWKeyLock.Value = !Devices.Outputs.SWKeyLock.Value;
                });
            }
        }
        #endregion

        #region Privates
        private readonly NavigationStore _navigationStore;
        private readonly RecipeSelector _recipeSelector;
        private readonly System.Timers.Timer statusUpdateTimer;
        #endregion
    }
}
