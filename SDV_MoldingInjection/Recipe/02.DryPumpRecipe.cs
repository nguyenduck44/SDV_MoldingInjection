using EQX.Core.Recipe;

namespace SDV_MoldingInjection.Recipe
{
    public class DryPumpRecipe : RecipeBase
    {
		public double MinVacuumPressure
		{
			get { return _minVacuumPressure; }
			set { _minVacuumPressure = value; }
		}

        public double MaxVacuumPressure
        {
            get { return _maxVacuumPressure; }
            set { _maxVacuumPressure = value; }
        }

        #region Privates
		private double _minVacuumPressure;
        private double _maxVacuumPressure;
        #endregion
    }
}
