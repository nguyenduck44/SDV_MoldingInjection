using EQX.Core.Motion;
using EQX.Core.Recipe;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipePositionManager : RecipePositionManagerBase<RecipeList>
    {
        public RecipePositionManager(RecipeList recipeList, IEnumerable<IMotion> motions)
            : base(recipeList, motions)
        {
        }
    }
}
