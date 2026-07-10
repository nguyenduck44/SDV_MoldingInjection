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

        [SingleRecipeDescription(Description = "Main Air Threshold", Unit = Unit.MegaPascal)]
        [ParameterDescription(243)]
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
        [ParameterDescription(244)]
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
        [ParameterDescription(245)]
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
        [ParameterDescription(246)]
        public double PumpValveVacuumThreshold
        {
            get { return pumpValveVacuumThreshold; }
            set
            {
                if (pumpValveVacuumThreshold == value) return;
                SetRecipe(ref pumpValveVacuumThreshold, value);
            }
        }
    }
}
