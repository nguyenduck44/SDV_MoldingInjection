using EQX.Core.Helpers;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Defines
{
    public class CDAStatus
    {
        private readonly AnalogInputs _analogInputs;
        private readonly RecipeList _recipeList;

        public CDAStatus(AnalogInputs analogInputs, RecipeList recipeList)
        {
            _analogInputs = analogInputs;
            _recipeList = recipeList;
        }

        public double PumpCDA => AnalogConvertHelper.Convert(_analogInputs.Pump.Volt, 0.0, 5.0, 0, 100);
        public double MainAirCDA => AnalogConvertHelper.Convert(_analogInputs.MainAir.Volt, 0.0, 5.0, 0, 100);
        public double PumpVentCDA => AnalogConvertHelper.Convert(_analogInputs.PumpVent.Volt, 0.0, 5.0, 0, 100);
        public double ChamberPressureCDA => AnalogConvertHelper.Convert(_analogInputs.ChamberPressure.Volt, 0.0, 5.0, 0, 100);
        public double SyringeAirH1 => AnalogConvertHelper.Convert(_analogInputs.SyringeAirH1.Volt, 0.0, 5.0, 0, 100);
        public double SyringeAirH2 => AnalogConvertHelper.Convert(_analogInputs.SyringeAirH2.Volt, 0.0, 5.0, 0, 100);
        public double SyringeAirH3 => AnalogConvertHelper.Convert(_analogInputs.SyringeAirH3.Volt, 0.0, 5.0, 0, 100);
        public double SyringeAirH4 => AnalogConvertHelper.Convert(_analogInputs.SyringeAirH4.Volt, 0.0, 5.0, 0, 100);

        public bool IsPumpCDA => PumpCDA >= _recipeList.CDASettingRecipe.PumpCDAThreshold;
        public bool IsMainAir => MainAirCDA >= _recipeList.CDASettingRecipe.MainAirThreshold;
        public bool IsPumpVentCDA => PumpVentCDA >= _recipeList.CDASettingRecipe.PumpVentThreshold;
        public bool IsChamberPressure => ChamberPressureCDA >= _recipeList.CDASettingRecipe.ChamberPressureThreshold;
        public bool IsSyringeAirH1 => SyringeAirH1 >= _recipeList.CDASettingRecipe.SyringeAirH1Threshold;
        public bool IsSyringeAirH2 => SyringeAirH2 >= _recipeList.CDASettingRecipe.SyringeAirH2Threshold;
        public bool IsSyringeAirH3 => SyringeAirH3 >= _recipeList.CDASettingRecipe.SyringeAirH3Threshold;
        public bool IsSyringeAirH4 => SyringeAirH4 >= _recipeList.CDASettingRecipe.SyringeAirH4Threshold;
    }
}
