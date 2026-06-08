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
        private double syringeAirH1Threshold;
        private double syringeAirH2Threshold;
        private double syringeAirH3Threshold;
        private double syringeAirH4Threshold;

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

        [SingleRecipeDescription(Description = "Syringe Air H1 Threshold", Unit = Unit.MilliPascal)]
        public double SyringeAirH1Threshold
        {
            get { return syringeAirH1Threshold; }
            set
            {
                if (syringeAirH1Threshold == value) return;
                SetRecipe(ref syringeAirH1Threshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Air H2 Threshold", Unit = Unit.MilliPascal)]
        public double SyringeAirH2Threshold
        {
            get { return syringeAirH2Threshold; }
            set
            {
                if (syringeAirH2Threshold == value) return;
                SetRecipe(ref syringeAirH2Threshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Air H3 Threshold", Unit = Unit.MilliPascal)]
        public double SyringeAirH3Threshold
        {
            get { return syringeAirH3Threshold; }
            set
            {
                if (syringeAirH3Threshold == value) return;
                SetRecipe(ref syringeAirH3Threshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Air H4 Threshold", Unit = Unit.MilliPascal)]
        public double SyringeAirH4Threshold
        {
            get { return syringeAirH4Threshold; }
            set
            {
                if (syringeAirH4Threshold == value) return;
                SetRecipe(ref syringeAirH4Threshold, value);
            }
        }
    }
}
