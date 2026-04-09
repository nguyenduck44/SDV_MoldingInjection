using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using log4net;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Productions;
using SDV_MoldingInjection.MVVM.Models;
using SDV_MoldingInjection.Recipe;
using System;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AutoViewModel : ViewModelBase
    {
        #region Properties
        public Devices Devices { get; }
        public MachineStatus MachineStatus { get; }
        public SyringAmountStatusList SyringeAmountStatusList { get; }
        public CarrierJigStatusList CarrierJigStatusList { get; }
        public Plotter Plotter { get; }
        public MachineStatusAutoViewModel MachineStatusAuto { get; }

        public int TodayInputCount => CurrentProductionData?.TotalInput ?? 0;
        public int TodayOutputCount => CurrentProductionData?.TotalOutput ?? 0;
        #endregion

        #region Contructors
        public AutoViewModel(
            Devices devices,
            MachineStatus machineStatus,
            NavigationStore navigationStore,
            SyringAmountStatusList syringeAmountStatusList,
            RecipeSelector recipeSelector,
            CarrierJigStatusList carrierJigStatusList,
            Plotter plotter,
            ProductionService productionService,
            MachineStatusAutoViewModel machineStatusAuto)
        {
            Devices = devices;
            MachineStatus = machineStatus;
            _navigationStore = navigationStore;
            SyringeAmountStatusList = syringeAmountStatusList;
            _recipeSelector = recipeSelector;
            CarrierJigStatusList = carrierJigStatusList;
            Plotter = plotter;
            _productionService = productionService;
            MachineStatusAuto = machineStatusAuto;
            Log = LogManager.GetLogger("AutoVM");

            PCInformationsystemViewModel = new PCInformationsystemViewModel();
            statusUpdateTimer = new NonOverlappingTimer(100);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }
        #endregion

        #region Private Methods
        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel != this) return;
            UpdateSyringeStatus();
            OnPropertyChanged(nameof(TodayInputCount));
            OnPropertyChanged(nameof(TodayOutputCount));
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

            double maxVolumeG = common.SyringeAmountWeight;
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
                status.IsHeadAvailable = GetSyringeCheckByIndex(i);
            }
        }

        private bool GetSyringeCheckByIndex(int index)
        {
            return index switch
            {
                0 => Devices.Inputs.H1_SyringeCheck.Value,
                1 => Devices.Inputs.H2_SyringeCheck.Value,
                2 => Devices.Inputs.H3_SyringeCheck.Value,
                3 => Devices.Inputs.H4_SyringeCheck.Value,
                _ => true
            };
        }
        #endregion

        #region Commands
        public ICommand DoorOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MachineStatus.IsAutoMode)
                    {
                        MessageBoxEx.ShowDialog("MACHINE IN AUTO MODE, CAN NOT OPEN THE DOOR!!!", false, "WARNING");
                        return;
                    }

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

        public PCInformationsystemViewModel PCInformationsystemViewModel { get; }

        private readonly NonOverlappingTimer statusUpdateTimer;
        private readonly ProductionService _productionService;
        private DateTime CurrentProductionDate => DateTime.Now.Hour < 8 ? DateTime.Now.Date.AddDays(-1) : DateTime.Now.Date;
        private ProductionDayItem? CurrentProductionData => _productionService.GetProductionByDate(CurrentProductionDate);
        #endregion
    }
}
