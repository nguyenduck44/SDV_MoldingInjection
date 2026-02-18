using EQX.Core.Recipe;

namespace SDV_MoldingInjection.Recipe
{
    public class DryPumpRecipe : RecipeBase
    {
		public double VacuumPressureSpec_Under
		{
			get { return _vacuumPressureSpec_Under; }
			set { _vacuumPressureSpec_Under = value; }
		}

        public double VacuumPressureSpec_Upper
        {
            get { return _vacuumPressureSpec_Upper; }
            set { _vacuumPressureSpec_Upper = value; }
        }

        #region Privates
		private double _vacuumPressureSpec_Under;
        private double _vacuumPressureSpec_Upper;
        #endregion
    }
}
