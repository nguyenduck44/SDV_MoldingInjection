using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class DryPumpRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Chamber Vacuum Pressure Spec", Unit = Unit.Torr)]
        public double VacuumPressureSpec
        {
            get => _vacuumPressureSpec;
            set => SetRecipe(ref _vacuumPressureSpec, value, nameof(VacuumPressureSpec));
        }

        [SingleRecipeDescription(Description = "Pressure Hold Under Spec", 
            Detail = "Hold pressure under spec while inject", Unit = Unit.Torr)]
        public double VacuumPressureHoldUnderSpec
        {
            get => _vacuumPressureHoldUnderSpec;
            set => SetRecipe(ref _vacuumPressureHoldUnderSpec, value, nameof(VacuumPressureHoldUnderSpec));
        }

        [SingleRecipeDescription(Description = "Pressure Log Time Lapse", Unit = Unit.Second)]
        public double PressureLogTimelaps
        {
            get => _pressureLogTimelaps;
            set => SetRecipe(ref _pressureLogTimelaps, value, nameof(PressureLogTimelaps));
        }

        #region Privates
        private double _vacuumPressureSpec;
        private double _vacuumPressureHoldUnderSpec;
        private double _pressureLogTimelaps;
        #endregion
    }
}
