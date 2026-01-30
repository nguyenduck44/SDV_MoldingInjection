namespace SDV_BIM_Line_DotDispenser.Recipe
{
    public class RecipeList
    {
        public RecipeList(CommonRecipe commonRecipe)
        {
            CommonRecipe = commonRecipe;
            CommonRecipe.Name = "Common";
        }

        public CommonRecipe CommonRecipe { get; }
    }
}
