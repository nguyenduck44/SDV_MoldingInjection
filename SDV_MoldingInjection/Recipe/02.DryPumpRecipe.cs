using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class DryPumpRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Chamber Pirani Target", Unit = Unit.Torr)]
        [ParameterDescription(21)]
        public double VacuumPressureSpec
        {
            get => _vacuumPressureSpec;
            set => SetRecipe(ref _vacuumPressureSpec, value);
        }

        [SingleRecipeDescription(Description = "Chamber Pirani Upper Spec", 
            Detail = "Hold pressure under spec while inject", Unit = Unit.Torr)]
        [ParameterDescription(22)]
        public double VacuumPressureHoldUnderSpec
        {
            get => _vacuumPressureHoldUnderSpec;
            set => SetRecipe(ref _vacuumPressureHoldUnderSpec, value);
        }

        [SingleRecipeDescription(Description = "Pressure Log Time Lapse", Unit = Unit.Second)]
        [ParameterDescription(23)]
        public double PressureLogTimelaps
        {
            get => _pressureLogTimelaps;
            set => SetRecipe(ref _pressureLogTimelaps, value);
        }

        [SingleRecipeDescription(Description = "Ratio Vent Pirani Lower", Unit = Unit.Torr)]
        [ParameterDescription(24)]
        public double RatioVentPressureLower
        {
            get => _ratioVentPressureLower;
            set => SetRecipe(ref _ratioVentPressureLower, value);
        }

        [SingleRecipeDescription(Description = "Ratio Vent Pirani Upper", Unit = Unit.Torr)]
        [ParameterDescription(25)]
        public double RatioVentPressureUpper
        {
            get => _ratioVentPressureUpper;
            set => SetRecipe(ref _ratioVentPressureUpper, value);
        }

        #region Privates
        private double _vacuumPressureSpec;
        private double _vacuumPressureHoldUnderSpec;
        private double _pressureLogTimelaps;
        private double _ratioVentPressureLower;
        private double _ratioVentPressureUpper;
        #endregion
    }
}
