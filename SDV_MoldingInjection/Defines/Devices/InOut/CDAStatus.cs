using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Common;
using EQX.Core.Helpers;
using OpenCvSharp;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Defines
{
    public class CDAStatus : ObservableObject
    {
        private readonly AnalogInputs _analogInputs;
        private readonly RecipeList _recipeList;
        private readonly NonOverlappingTimer statusUpdateTimer;
        public CDAStatus(AnalogInputs analogInputs, RecipeList recipeList)
        {
            _analogInputs = analogInputs;
            _recipeList = recipeList;
            statusUpdateTimer = new NonOverlappingTimer(1000);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }
        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {
            OnPropertyChanged(nameof(PumpPurge));
            OnPropertyChanged(nameof(MainAirCDA));
            OnPropertyChanged(nameof(PumpValveVacuum));
            OnPropertyChanged(nameof(SyringeMainAirCDA));

            OnPropertyChanged(nameof(IsPumpPurge));
            OnPropertyChanged(nameof(IsMainAir));
            OnPropertyChanged(nameof(IsPumpValveVacuum));
            OnPropertyChanged(nameof(IsSyringeAirMain));

            #region FDC-CDA
            FDCHelper.WriteFDCValue(EFDCValue.HEAD_DISPENSING_PRESSURE, (int)(SyringeMainAirCDA * 1000));
            FDCHelper.WriteFDCValue(EFDCValue.CDA_ANGLE_VALVE_PRESSURE, (int)(PumpValveVacuum * 1000));
            FDCHelper.WriteFDCValue(EFDCValue.CDA_VENT_PRESSURE, (int)(PumpPurge * 1000));
            FDCHelper.WriteFDCValue(EFDCValue.CDA_MAIN_AIR_PRESSURE, (int)(MainAirCDA * 1000));
            #endregion
        }
        public double PumpPurge => AnalogConvertHelper.Convert(_analogInputs.PumpPurge.Volt, 0.6, 5.0, -0.1, 1.0);
        public double MainAirCDA => AnalogConvertHelper.Convert(_analogInputs.MainAir.Volt, 0.6, 5.0, -0.1, 1.0);
        public double PumpValveVacuum => AnalogConvertHelper.Convert(_analogInputs.PumpValveVacuum.Volt, 0.6, 5.0, -0.1, 1.0);
        public double SyringeMainAirCDA => AnalogConvertHelper.Convert(_analogInputs.SyringeMainAirCDA.Volt, 0.6, 5.0, -0.1, 1.0);

        public bool IsPumpPurge => PumpPurge >= _recipeList.CDASettingRecipe.PumpPurgeThreshold;
        public bool IsMainAir => MainAirCDA >= _recipeList.CDASettingRecipe.MainAirThreshold;
        public bool IsPumpValveVacuum => PumpValveVacuum >= _recipeList.CDASettingRecipe.PumpValveVacuumThreshold;
        public bool IsSyringeAirMain => SyringeMainAirCDA >= _recipeList.CDASettingRecipe.SyringeAirMainThreshold;

        public void StopUpdateCDA()
        {
            statusUpdateTimer.Stop();
        }
    }
}
