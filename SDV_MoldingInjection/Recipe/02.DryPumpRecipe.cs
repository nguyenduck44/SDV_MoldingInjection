using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class DryPumpRecipe : RecipeBase
    {
		public double VacuumPressureSpec_Under
        {
			get { return _vacuumPressureSpec_Under; }
			set { _vacuumPressureSpec_Under = value; }
		}

        [SingleRecipeDescription(Description = "Chamber vacuum pressure spec", Unit = Unit.Torr)]
        public double VacuumPressureSpec
        {
            get { return _vacuumPressureSpec; }
            set { _vacuumPressureSpec = value; }
        }

        #region Privates
		private double _vacuumPressureSpec_Under;
        private double _vacuumPressureSpec;
        #endregion
    }
}
