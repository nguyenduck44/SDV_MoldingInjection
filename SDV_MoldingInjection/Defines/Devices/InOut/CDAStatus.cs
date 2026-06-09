using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Common;
using EQX.Core.Helpers;
using OpenCvSharp;
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
            OnPropertyChanged(nameof(SyringeAirH1));
            OnPropertyChanged(nameof(SyringeAirH2));
            OnPropertyChanged(nameof(SyringeAirH3));
            OnPropertyChanged(nameof(SyringeAirH4));

            OnPropertyChanged(nameof(IsPumpPurge));
            OnPropertyChanged(nameof(IsMainAir));
            OnPropertyChanged(nameof(IsPumpValveVacuum));
            OnPropertyChanged(nameof(IsSyringeAirMain));
            OnPropertyChanged(nameof(IsSyringeAirH1));
            OnPropertyChanged(nameof(IsSyringeAirH2));
            OnPropertyChanged(nameof(IsSyringeAirH3));
            OnPropertyChanged(nameof(IsSyringeAirH4));
        }
        public double PumpPurge => AnalogConvertHelper.Convert(_analogInputs.PumpPurge.Volt, 0.6, 5.0, -0.1, 1.0);
        public double MainAirCDA => AnalogConvertHelper.Convert(_analogInputs.MainAir.Volt, 0.6, 5.0, -0.1, 1.0);
        public double PumpValveVacuum => AnalogConvertHelper.Convert(_analogInputs.PumpValveVacuum.Volt, 0.6, 5.0, -0.1, 1.0);
        public double SyringeMainAirCDA => AnalogConvertHelper.Convert(_analogInputs.SyringeMainAirCDA.Volt, 0.6, 5.0, -0.1, 1.0);
        public double SyringeAirH1 => AnalogConvertHelper.Convert(_analogInputs.SyringeAirH1.Volt, 0.6, 5.0, -0.1, 1.0);
        public double SyringeAirH2 => AnalogConvertHelper.Convert(_analogInputs.SyringeAirH2.Volt, 0.6, 5.0, -0.1, 1.0);
        public double SyringeAirH3 => AnalogConvertHelper.Convert(_analogInputs.SyringeAirH3.Volt, 0.6, 5.0, -0.1, 1.0);
        public double SyringeAirH4 => AnalogConvertHelper.Convert(_analogInputs.SyringeAirH4.Volt, 0.6, 5.0, -0.1, 1.0);

        public bool IsPumpPurge => PumpPurge >= _recipeList.CDASettingRecipe.PumpPurgeThreshold;
        public bool IsMainAir => MainAirCDA >= _recipeList.CDASettingRecipe.MainAirThreshold;
        public bool IsPumpValveVacuum => PumpValveVacuum >= _recipeList.CDASettingRecipe.PumpValveVacuumThreshold;
        public bool IsSyringeAirMain => SyringeMainAirCDA >= _recipeList.CDASettingRecipe.SyringeAirMainThreshold;
        public bool IsSyringeAirH1 => SyringeAirH1 >= _recipeList.CDASettingRecipe.SyringeAirH1Threshold;
        public bool IsSyringeAirH2 => SyringeAirH2 >= _recipeList.CDASettingRecipe.SyringeAirH2Threshold;
        public bool IsSyringeAirH3 => SyringeAirH3 >= _recipeList.CDASettingRecipe.SyringeAirH3Threshold;
        public bool IsSyringeAirH4 => SyringeAirH4 >= _recipeList.CDASettingRecipe.SyringeAirH4Threshold;
    }
}
