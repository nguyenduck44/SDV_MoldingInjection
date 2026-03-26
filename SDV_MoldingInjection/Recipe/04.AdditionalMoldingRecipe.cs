using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class AdditionalMoldingRecipe : RecipeBase
    {
        #region privates
        private bool _skipAddTail;
        private double _addTailWeight;
        private double _zUpDistanceAddTail;
        private double _addTailSpeed;
        private double _bubbleRemoveCount;
        private double _bubbleRemoveRotateCount;
        private double _addTailTime;

        #endregion

        [SingleRecipeDescription(Description = "Skip Add Tail", Detail = "Check to Skip Add Tail")]
        public bool SkipAddTail
        {
            get => _skipAddTail;
            set => SetRecipe(ref _skipAddTail, value, nameof(SkipAddTail));
        }

        [SingleRecipeDescription(Description = "Z-UP Distance Add Tail", Unit = Unit.mm)]
        public double ZUpDistanceAddTail
        {
            get => _zUpDistanceAddTail;
            set => SetRecipe(ref _zUpDistanceAddTail, value, nameof(ZUpDistanceAddTail));
        }

        [SingleRecipeDescription(Description = "Add Tail Weight", Unit = Unit.Percentage)]
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

        [SingleRecipeDescription(Description = "Bubble remove rotate count")]
        [SingleRecipeMinMax(Min = 1, Max = 100)]
        public double BubbleRemoveRotateCount
        {
            get => _bubbleRemoveRotateCount;
            set => SetRecipe(ref _bubbleRemoveRotateCount, value, nameof(BubbleRemoveRotateCount));
        }

        public double AddTailTime
        {
            get => _addTailTime;
            set => SetRecipe(ref _addTailTime, value, nameof(AddTailTime));
        }
    }
}
