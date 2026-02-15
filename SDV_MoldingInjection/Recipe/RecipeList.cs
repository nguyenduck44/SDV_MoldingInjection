using EQX.Core.Recipe;
using Newtonsoft.Json;

namespace SDV_MoldingInjection.Recipe
{
    public class RecipeList
    {
        [JsonConstructor]
        public RecipeList(CommonRecipe commonRecipe,
                          DispensingRecipe dispensingRecipe,
                          PlasmaRecipe plasmaRecipe,
                          CleanRecipe cleanRecipe,
                          FinalInspectRecipe finalInspectRecipe,
                          UVCureRecipe uVCureRecipe,
                          TransferRecipe transferRecipe)
        {
            CommonRecipe = commonRecipe;
            DispensingRecipe = dispensingRecipe;
            PlasmaRecipe = plasmaRecipe;
            CleanRecipe = cleanRecipe;
            FinalInspectRecipe = finalInspectRecipe;
            UVCureRecipe = uVCureRecipe;
            TransferRecipe = transferRecipe;
        }
        public RecipeList(IEnumerable<IRecipe> recipes)
        {
            if (recipes.Count() == 0)
            {
                throw new ArgumentException("No recipes found to initialize RecipeList.");
            }

            CommonRecipe = recipes.OfType<CommonRecipe>().FirstOrDefault()!;
            DispensingRecipe = recipes.OfType<DispensingRecipe>().FirstOrDefault()!;
            PlasmaRecipe = recipes.OfType<PlasmaRecipe>().FirstOrDefault()!;
            CleanRecipe = recipes.OfType<CleanRecipe>().FirstOrDefault()!;
            FinalInspectRecipe = recipes.OfType<FinalInspectRecipe>().FirstOrDefault()!;
            UVCureRecipe = recipes.OfType<UVCureRecipe>().FirstOrDefault()!;
            TransferRecipe = recipes.OfType<TransferRecipe>().FirstOrDefault()!;
        }

        public CommonRecipe CommonRecipe { get; }
        public DispensingRecipe DispensingRecipe { get; }
        public PlasmaRecipe PlasmaRecipe { get; }
        public CleanRecipe CleanRecipe { get; }
        public FinalInspectRecipe FinalInspectRecipe { get; }
        public UVCureRecipe UVCureRecipe { get; }
        public TransferRecipe TransferRecipe { get; }
    }
}
