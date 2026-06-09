using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class CDASettingRecipe : RecipeBase
    {
        private double mainAirThreshold;
        private double syringeAirMainThreshold;
        private double pumpPurgeThreshold;
        private double pumpValveVacuumThreshold;
        private double syringeAirH1Threshold;
        private double syringeAirH2Threshold;
        private double syringeAirH3Threshold;
        private double syringeAirH4Threshold;

        [SingleRecipeDescription(Description = "Main Air Threshold", Unit = Unit.MegaPascal)]
        public double MainAirThreshold
        {
            get { return mainAirThreshold; }
            set
            {
                if (mainAirThreshold == value) return;
                SetRecipe(ref mainAirThreshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Air Main Threshold", Unit = Unit.MegaPascal)]
        public double SyringeAirMainThreshold
        {
            get { return syringeAirMainThreshold; }
            set
            {
                if (syringeAirMainThreshold == value) return;
                SetRecipe(ref syringeAirMainThreshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Pump Purge Threshold", Unit = Unit.MegaPascal)]
        public double PumpPurgeThreshold
        {
            get { return pumpPurgeThreshold; }
            set
            {
                if (pumpPurgeThreshold == value) return;
                SetRecipe(ref pumpPurgeThreshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Pump Valve Vacuum Threshold", Unit = Unit.MegaPascal)]
        public double PumpValveVacuumThreshold
        {
            get { return pumpValveVacuumThreshold; }
            set
            {
                if (pumpValveVacuumThreshold == value) return;
                SetRecipe(ref pumpValveVacuumThreshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Air H1 Threshold", Unit = Unit.MegaPascal)]
        public double SyringeAirH1Threshold
        {
            get { return syringeAirH1Threshold; }
            set
            {
                if (syringeAirH1Threshold == value) return;
                SetRecipe(ref syringeAirH1Threshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Air H2 Threshold", Unit = Unit.MegaPascal)]
        public double SyringeAirH2Threshold
        {
            get { return syringeAirH2Threshold; }
            set
            {
                if (syringeAirH2Threshold == value) return;
                SetRecipe(ref syringeAirH2Threshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Air H3 Threshold", Unit = Unit.MegaPascal)]
        public double SyringeAirH3Threshold
        {
            get { return syringeAirH3Threshold; }
            set
            {
                if (syringeAirH3Threshold == value) return;
                SetRecipe(ref syringeAirH3Threshold, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Air H4 Threshold", Unit = Unit.MegaPascal)]
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
