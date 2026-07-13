using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class AdditionalMoldingRecipe : RecipeBase
    {
        #region privates
        private bool _useAddTail;
        private double _addTailWeight;
        private double _zUpDistanceAddTail;
        private double _addTailSpeed;
        #endregion

        [SingleRecipeDescription(Description = "Use Add Tail", Detail = "Check to Use Add Tail")]
        [ParameterDescription(200)]
        public bool UseAddTail
        {
            get => _useAddTail;
            set => SetRecipe(ref _useAddTail, value);
        }

        [SingleRecipeDescription(Description = "Z-UP Distance Add Tail", Unit = Unit.mm)]
        [ParameterDescription(201)]
        public double ZUpDistanceAddTail
        {
            get => _zUpDistanceAddTail;
            set => SetRecipe(ref _zUpDistanceAddTail, value);
        }

        [SingleRecipeDescription(Description = "Add Tail Weight", Unit = Unit.mg)]
        [SingleRecipeMinMax(Max = 380, Min = 0)]
        [ParameterDescription(202)]
        public double AddTailWeight
        {
            get => _addTailWeight;
            set => SetRecipe(ref _addTailWeight, value);
        }

        [SingleRecipeDescription(Description = "Add Tail Speed", Unit = Unit.mmPerSecond)]
        [ParameterDescription(203)]
        public double AddTailSpeed
        {
            get => _addTailSpeed;
            set => SetRecipe(ref _addTailSpeed, value);
        }
    }
}
