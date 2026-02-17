using EQX.Core.Recipe;
using Newtonsoft.Json;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipeList
    {
        [JsonConstructor]
        public RecipeList(CommonRecipe commonRecipe,
                          MoldRecipe moldRecipe,
                          DryPumpRecipe dryPumpRecipe,
                          SPDHeadRecipe sPDHead1_Recipe,
                          SPDHeadRecipe sPDHead2_Recipe,
                          SPDHeadRecipe sPDHead3_Recipe,
                          SPDHeadRecipe sPDHead4_Recipe)
        {
            CommonRecipe = commonRecipe;
            MoldRecipe = moldRecipe;
            DryPumpRecipe = dryPumpRecipe;
            SPDHead1_Recipe = sPDHead1_Recipe;
            SPDHead2_Recipe = sPDHead2_Recipe;
            SPDHead3_Recipe = sPDHead3_Recipe;
            SPDHead4_Recipe = sPDHead4_Recipe;
        }
        public RecipeList(IEnumerable<IRecipe> recipes)
        {
            if (recipes.Count() == 0)
            {
                throw new ArgumentException("No recipes found to initialize RecipeList.");
            }

            CommonRecipe = recipes.OfType<CommonRecipe>().FirstOrDefault()!;
            MoldRecipe = recipes.OfType<MoldRecipe>().FirstOrDefault()!;
            DryPumpRecipe = recipes.OfType<DryPumpRecipe>().FirstOrDefault()!;

            SPDHead1_Recipe = recipes.OfType<SPDHeadRecipe>().First(r => r.Name == "SPDHead1_Recipe")!;
            SPDHead2_Recipe = recipes.OfType<SPDHeadRecipe>().First(r => r.Name == "SPDHead2_Recipe")!;
            SPDHead3_Recipe = recipes.OfType<SPDHeadRecipe>().First(r => r.Name == "SPDHead3_Recipe")!;
            SPDHead2_Recipe = recipes.OfType<SPDHeadRecipe>().First(r => r.Name == "SPDHead4_Recipe")!;
        }

        public CommonRecipe CommonRecipe { get; }
        public MoldRecipe MoldRecipe { get; }
        public DryPumpRecipe DryPumpRecipe { get; }
        public SPDHeadRecipe SPDHead1_Recipe { get; }
        public SPDHeadRecipe SPDHead2_Recipe { get; }
        public SPDHeadRecipe SPDHead3_Recipe { get; }
        public SPDHeadRecipe SPDHead4_Recipe { get; }
    }
}
