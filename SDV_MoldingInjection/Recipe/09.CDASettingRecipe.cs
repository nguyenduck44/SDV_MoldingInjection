using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class CDASettingRecipe : RecipeBase
    {
        private double mainAirThreshold;
        private double chamberPressureThreshold;
        private double pumpCDAThreshold;
        private double pumpVentThreshold;

        [SingleRecipeDescription(Description = "Main Air Threshold", Unit = Unit.MilliPascal)]
        public double MainAirThreshold
        {
            get { return mainAirThreshold; }
            set
            {
                if (mainAirThreshold == value) return;
                SetRecipe(ref mainAirThreshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Chamber Pressure Threshold", Unit = Unit.MilliPascal)]
        public double ChamberPressureThreshold
        {
            get { return chamberPressureThreshold; }
            set
            {
                if (chamberPressureThreshold == value) return;
                SetRecipe(ref chamberPressureThreshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Pump CDA Threshold", Unit = Unit.MilliPascal)]
        public double PumpCDAThreshold
        {
            get { return pumpCDAThreshold; }
            set
            {
                if (pumpCDAThreshold == value) return;
                SetRecipe(ref pumpCDAThreshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Pump Vent Threshold", Unit = Unit.MilliPascal)]
        public double PumpVentThreshold
        {
            get { return pumpVentThreshold; }
            set
            {
                if (pumpVentThreshold == value) return;
                SetRecipe(ref pumpVentThreshold, value);
            }
        }

    }
}
