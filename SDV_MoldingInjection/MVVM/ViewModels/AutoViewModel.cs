using System;
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
        #endregion

        #region Contructors
        public AutoViewModel(
            Devices devices,
            NavigationStore navigationStore,
            MachineStatus machineStatus,
            RecipeSelector recipeSelector)
        {
            Devices = devices;
            _navigationStore = navigationStore;
            MachineStatus = machineStatus;
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

            UpdateSyringeStatus();
        }

        private void UpdateSyringeStatus()
        {
            var recipe = _recipeSelector.CurrentRecipe;
            if (recipe == null) return;

            var common = recipe.CommonRecipe;
            if (common == null) return;

            double maxVolumeG = common.SyringeAmountWeight / 1000.0;
            double limitHours = common.SyringeMountTimeChange;

            for (int i = 0; i < MachineStatus.SyringeAmounts.Length; i++)
            {
                var status = MachineStatus.SyringeAmounts[i];
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

        #region Privates
        private readonly NavigationStore _navigationStore;
        private readonly RecipeSelector _recipeSelector;
        private readonly System.Timers.Timer statusUpdateTimer;
        #endregion
    }
}
