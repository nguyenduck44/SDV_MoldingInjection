using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class DispensingRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Dispensing Z axis ready position", Unit = Unit.mm)]
        public double ZAxis_ReadyPosition
        {
            get { return _ZAxis_ReadyPosition; }
            set
            {
                OnRecipeChanged(_ZAxis_ReadyPosition, value);
                _ZAxis_ReadyPosition = value;
            }
        }

        #region Privates
        double _ZAxis_ReadyPosition;
        #endregion
    }
}
