using EQX.Core.Recipe;
using Newtonsoft.Json;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipeList
    {
        public RecipeList()
        {
            CommonRecipe = new();
            InjectRecipe = new();
            DryPumpRecipe = new();
            SPDHead1_Recipe = new();
            SPDHead2_Recipe = new();
            SPDHead3_Recipe = new();
            SPDHead4_Recipe = new();
        }

        public CommonRecipe CommonRecipe { get; }
        public InjectRecipe InjectRecipe { get; }
        public DryPumpRecipe DryPumpRecipe { get; }
        public SPDHeadRecipe SPDHead1_Recipe { get; }
        public SPDHeadRecipe SPDHead2_Recipe { get; }
        public SPDHeadRecipe SPDHead3_Recipe { get; }
        public SPDHeadRecipe SPDHead4_Recipe { get; }

        public void CloneFrom(RecipeList source)
        {
            if (source == null) return;

            CommonRecipe.Clone(source.CommonRecipe);
            InjectRecipe.Clone(source.InjectRecipe);
            DryPumpRecipe.Clone(source.DryPumpRecipe);
            SPDHead1_Recipe.Clone(source.SPDHead1_Recipe);
            SPDHead2_Recipe.Clone(source.SPDHead2_Recipe);
            SPDHead3_Recipe.Clone(source.SPDHead3_Recipe);
            SPDHead4_Recipe.Clone(source.SPDHead4_Recipe);
        }
    }
}
