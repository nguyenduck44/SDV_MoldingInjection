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

        #endregion

        [SingleRecipeDescription(Description = "Skip Add Tail", Detail = "Check to Skip Add Tail")]
        public bool SkipAddTail
        {
            get { return _skipAddTail; }
            set
            {
                if (_skipAddTail == value) return;
                OnRecipeChanged(_skipAddTail, value);
                _skipAddTail = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Z-UP Distance Add Tail", Unit = Unit.mm)]
        public double ZUpDistanceAddTail
        {
            get { return _zUpDistanceAddTail; }
            set
            {
                if (_zUpDistanceAddTail == value) return;
                OnRecipeChanged(_zUpDistanceAddTail, value);
                _zUpDistanceAddTail = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Add Tail Weight", Unit = Unit.Percentage)]
        public double AddTailWeight
        {
            get { return _addTailWeight; }
            set
            {
                if (_addTailWeight == value) return;
                OnRecipeChanged(_addTailWeight, value);
                _addTailWeight = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Add Tail Speed", Unit = Unit.mmPerSecond)]
        public double AddTailSpeed
        {
            get { return _addTailSpeed; }
            set
            {
                if (_addTailSpeed == value) return;
                OnRecipeChanged(_addTailSpeed, value);
                _addTailSpeed = value;
                OnPropertyChanged();
            }
        }
    }
}
