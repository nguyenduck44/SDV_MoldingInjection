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
        private double _bubbleRemoveCount;
        #endregion

        [SingleRecipeDescription(Description = "USE Add Tail", Detail = "Check to Use Add Tail")]
        public bool UseAddTail
        {
            get => _useAddTail;
            set => SetRecipe(ref _useAddTail, value, nameof(UseAddTail));
        }

        [SingleRecipeDescription(Description = "Z-UP Distance Add Tail", Unit = Unit.mm)]
        public double ZUpDistanceAddTail
        {
            get => _zUpDistanceAddTail;
            set => SetRecipe(ref _zUpDistanceAddTail, value, nameof(ZUpDistanceAddTail));
        }

        [SingleRecipeDescription(Description = "Add Tail Weight", Unit = Unit.mg)]
        public double AddTailWeight
        {
            get => _addTailWeight;
            set => SetRecipe(ref _addTailWeight, value, nameof(AddTailWeight));
        }

        [SingleRecipeDescription(Description = "Add Tail Speed", Unit = Unit.mmPerSecond)]
        public double AddTailSpeed
        {
            get => _addTailSpeed;
            set => SetRecipe(ref _addTailSpeed, value, nameof(AddTailSpeed));
        }

        [SingleRecipeDescription(Description = "Bubble remove count")]
        [CIMParameterAddress((int)ECIMParamter.BubbleRemoveCount)]
        [SingleRecipeMinMax(Min = 1, Max = 100)]
        public double BubbleRemoveCount
        {
            get => _bubbleRemoveCount;
            set => SetRecipe(ref _bubbleRemoveCount, value, nameof(BubbleRemoveCount));
        }
    }
}
