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
        public SyringAmountStatusList SyringeAmountStatusList { get; }
        #endregion

        #region Contructors
        public AutoViewModel(
            Devices devices,
            NavigationStore navigationStore,
            SyringAmountStatusList syringeAmountStatusList,
            RecipeSelector recipeSelector)
        {
            Devices = devices;
            _navigationStore = navigationStore;
            SyringeAmountStatusList = syringeAmountStatusList;
            _recipeSelector = recipeSelector;

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
            Devices.Inputs.MainPanelCloseCheck.RaiseValueUpdated();

            UpdateSyringeStatus();
        }

        private void UpdateSyringeStatus()
        {
            foreach(var syringe in SyringeAmountStatusList.SyringeAmounts)
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
