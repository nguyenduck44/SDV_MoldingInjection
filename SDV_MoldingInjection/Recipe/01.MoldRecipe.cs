using EQX.Core.Recipe;

namespace SDV_MoldingInjection.Recipe
{
    public class MoldRecipe : RecipeBase
    {
        public double YAxisLoadPosition
		{
			get { return _yAxisLoadPosition; }
			set { _yAxisLoadPosition = value; }
		}

        #region Privates
		private double _yAxisLoadPosition;
        #endregion
    }
}
