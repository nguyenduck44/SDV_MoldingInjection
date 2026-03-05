using EQX.Core.Recipe;

namespace SDV_MoldingInjection.Recipe
{
    public class OptionRecipe : RecipeBase
    {
        #region Privates
        private bool _skipHead1;
        private bool _skipHead2;
        private bool _skipHead3;
        private bool _skipHead4;

        #endregion

        [SingleRecipeDescription(Description = "Skip Head 1", Detail = "Check to Skip Head 1")]
        public bool SkipHead1
        {
            get { return _skipHead1; }
            set
            {
                OnRecipeChanged(_skipHead1, value);
                OnRecipeChanged(_skipHead2, value);
                _skipHead1 = value;
                _skipHead2 = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 2", Detail = "Check to Skip Head 2")]
        public bool SkipHead2
        {
            get { return _skipHead2; }
            set
            {
                OnRecipeChanged(_skipHead1, value);
                OnRecipeChanged(_skipHead2, value);
                _skipHead1 = value;
                _skipHead2 = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 3", Detail = "Check to Skip Head 3")]
        public bool SkipHead3
        {
            get { return _skipHead3; }
            set
            {
                OnRecipeChanged(_skipHead3, value);
                OnRecipeChanged(_skipHead4, value);
                _skipHead3 = value;
                _skipHead4 = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 4", Detail = "Check to Skip Head 4")]
        public bool SkipHead4
        {
            get { return _skipHead4; }
            set
            {
                OnRecipeChanged(_skipHead3, value);
                OnRecipeChanged(_skipHead4, value);
                _skipHead3 = value;
                _skipHead4 = value;
                OnPropertyChanged();
            }
        }
    }
}
