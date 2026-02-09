using EQX.Core.Recipe;

namespace SDV_DotDispenser.Recipe
{
    public class RecipeList
    {
        public RecipeList(IEnumerable<IRecipe> recipes)
        {
            if (recipes.Count() == 0)
            {
                throw new ArgumentException("No recipes found to initialize RecipeList.");
            }

            CommonRecipe = recipes.OfType<CommonRecipe>().FirstOrDefault()!;
            DispensingRecipe = recipes.OfType<DispensingRecipe>().FirstOrDefault()!;
        }

        public CommonRecipe CommonRecipe { get; }
        public DispensingRecipe DispensingRecipe { get; }
    }
}
