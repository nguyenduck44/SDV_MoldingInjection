using EQX.Core.Recipe;

namespace SDV_MoldingInjection.Recipe
{
    public class OptionRecipe : RecipeBase
    {
        #region Privates
        public event Action<bool> Head12SkipChanged;
        public event Action<bool> Head34SkipChanged;
        private bool _skipHead12;
        private bool _skipHead34;

        #endregion

        [SingleRecipeDescription(Description = "Skip Head 1 & 2", Detail = "Check to Skip Head 1 & Head 2")]
        public bool SkipHead12
        {
            get { return _skipHead12; }
            set
            {
                if (_skipHead12 == value) return;
                OnRecipeChanged(_skipHead12, value);
                Head12SkipChanged?.Invoke(value);
                _skipHead12 = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 3 & 4", Detail = "Check to Skip Head 3 & Head 4")]
        public bool SkipHead34
        {
            get { return _skipHead34; }
            set
            {
                if (_skipHead34 == value) return;
                OnRecipeChanged(_skipHead34, value);
                Head34SkipChanged?.Invoke(value);
                _skipHead34 = value;
                OnPropertyChanged();
            }
        }
    }
}
