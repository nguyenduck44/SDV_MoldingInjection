using EQX.Core.Recipe;
using Newtonsoft.Json;

namespace SDV_DotDispenser.Recipe
{
    public class RecipeList
    {
        [JsonConstructor]
        public RecipeList(CommonRecipe commonRecipe,
                          DispensingRecipe dispensingRecipe,
                          StageRecipe stageLeftRecipe,
                          StageRecipe stageRightRecipe,
                          PlasmaRecipe plasmaRecipe,
                          CleanRecipe cleanRecipe,
                          FinalInspectRecipe finalInspectRecipe,
                          UVCureRecipe uVCureRecipe,
                          TransferRecipe transferRecipe)
        {
            CommonRecipe = commonRecipe;
            DispensingRecipe = dispensingRecipe;
            StageLeftRecipe = stageLeftRecipe;
            StageRightRecipe = stageRightRecipe;
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
            StageLeftRecipe = recipes.OfType<StageRecipe>().FirstOrDefault(r => r.Name == "StageLeft")!;
            StageRightRecipe = recipes.OfType<StageRecipe>().FirstOrDefault(r => r.Name == "StageRight")!;
            PlasmaRecipe = recipes.OfType<PlasmaRecipe>().FirstOrDefault()!;
            CleanRecipe = recipes.OfType<CleanRecipe>().FirstOrDefault()!;
            FinalInspectRecipe = recipes.OfType<FinalInspectRecipe>().FirstOrDefault()!;
            UVCureRecipe = recipes.OfType<UVCureRecipe>().FirstOrDefault()!;
            TransferRecipe = recipes.OfType<TransferRecipe>().FirstOrDefault()!;
        }

        public CommonRecipe CommonRecipe { get; }
        public DispensingRecipe DispensingRecipe { get; }
        public StageRecipe StageLeftRecipe { get; }
        public StageRecipe StageRightRecipe { get; }
        public PlasmaRecipe PlasmaRecipe { get; }
        public CleanRecipe CleanRecipe { get; }
        public FinalInspectRecipe FinalInspectRecipe { get; }
        public UVCureRecipe UVCureRecipe { get; }
        public TransferRecipe TransferRecipe { get; }
    }
}
