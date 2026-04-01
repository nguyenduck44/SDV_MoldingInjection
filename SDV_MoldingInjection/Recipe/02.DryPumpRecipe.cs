using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class DryPumpRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Chamber vacuum pressure spec", Unit = Unit.Torr)]
        public double VacuumPressureSpec
        {
            get => _vacuumPressureSpec;
            set => SetRecipe(ref _vacuumPressureSpec, value, nameof(VacuumPressureSpec));
        }

        [SingleRecipeDescription(Description = "Pressure Spec 1st", 
            Detail = "Hold pressure under spec first", Unit = Unit.Torr)]
        public double VacuumPressureHoldUnderSpec
        {
            get => _vacuumPressureHoldUnderSpec;
            set => SetRecipe(ref _vacuumPressureHoldUnderSpec, value, nameof(VacuumPressureHoldUnderSpec));
        }

        [SingleRecipeDescription(Description = "Time start hold spec 2nd",
            Detail = "Time start hold pressure under spec 2nd", Unit = Unit.Second)]
        public double TimeStartHoldUnderSpecSecond
        {
            get => _timeStartHoldUnderSpecSecond;
            set => SetRecipe(ref _timeStartHoldUnderSpecSecond, value, nameof(TimeStartHoldUnderSpecSecond));
        }

        [SingleRecipeDescription(Description = "Pressure Spec 2nd",
            Detail = "Hold pressure under spec 2nd", Unit = Unit.Torr)]
        public double VacuumPressureHoldUnderSpecSecond
        {
            get => _vacuumPressureHoldUnderSpecSecond;
            set => SetRecipe(ref _vacuumPressureHoldUnderSpecSecond, value, nameof(VacuumPressureHoldUnderSpecSecond));
        }

        [SingleRecipeDescription(Description = "Pressure log timelaps", Unit = Unit.Second)]
        public double PressureLogTimelaps
        {
            get => _pressureLogTimelaps;
            set => SetRecipe(ref _pressureLogTimelaps, value, nameof(PressureLogTimelaps));
        }

        #region Privates
        private double _vacuumPressureSpec;
        private double _vacuumPressureHoldUnderSpec;
        private double _pressureLogTimelaps;
        private double _vacuumPressureHoldUnderSpecSecond;
        private double _timeStartHoldUnderSpecSecond;
        #endregion
    }
}
