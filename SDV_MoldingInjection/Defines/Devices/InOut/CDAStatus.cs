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

        public bool IsPumpCDA => PumpCDA >= _recipeList.CDASettingRecipe.PumpCDAThreshold;
        public bool IsMainAir => MainAirCDA >= _recipeList.CDASettingRecipe.MainAirThreshold;
        public bool IsPumpVentCDA => PumpVentCDA >= _recipeList.CDASettingRecipe.PumpVentThreshold;
        public bool IsChamberPressure => ChamberPressureCDA >= _recipeList.CDASettingRecipe.ChamberPressureThreshold;
    }
}
