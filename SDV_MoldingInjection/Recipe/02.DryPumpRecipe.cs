using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class DryPumpRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Chamber vacuum pressure spec", Unit = Unit.Torr)]
        public double VacuumPressureSpec
        {
            get { return _vacuumPressureSpec; }
            set { _vacuumPressureSpec = value; }
        }

        [SingleRecipeDescription(Description = "Chamber vacuum hold pressure under spec", Unit = Unit.Torr)]
        public double VacuumPressureHoldUnderSpec
        {
            get { return _vacuumPressureHoldUnderSpec; }
            set { _vacuumPressureHoldUnderSpec = value; }
        }

        #region Privates
        private double _vacuumPressureSpec;
        private double _vacuumPressureHoldUnderSpec;
        #endregion
    }
}
