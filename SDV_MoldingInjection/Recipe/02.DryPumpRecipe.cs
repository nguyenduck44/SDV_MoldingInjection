using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class DryPumpRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Chamber Vacuum Pressure Spec", Unit = Unit.Torr)]
        [ParameterDescription(30)]
        public double VacuumPressureSpec
        {
            get => _vacuumPressureSpec;
            set => SetRecipe(ref _vacuumPressureSpec, value);
        }

        [SingleRecipeDescription(Description = "Pressure Hold Under Spec", 
            Detail = "Hold pressure under spec while inject", Unit = Unit.Torr)]
        [ParameterDescription(31)]
        public double VacuumPressureHoldUnderSpec
        {
            get => _vacuumPressureHoldUnderSpec;
            set => SetRecipe(ref _vacuumPressureHoldUnderSpec, value);
        }

        [SingleRecipeDescription(Description = "Pressure Log Time Lapse", Unit = Unit.Second)]
        [ParameterDescription(32)]
        public double PressureLogTimelaps
        {
            get => _pressureLogTimelaps;
            set => SetRecipe(ref _pressureLogTimelaps, value);
        }

        #region Privates
        private double _vacuumPressureSpec;
        private double _vacuumPressureHoldUnderSpec;
        private double _pressureLogTimelaps;
        #endregion
    }
}
