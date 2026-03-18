using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectTimeRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Delay Time", Unit = Unit.Second)]
        public double DelayTime
        {
            get { return _delayTime; }
            set
            {
                if (_delayTime == value) return;
                OnRecipeChanged(_delayTime, value);
                _delayTime = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Inject Time", Unit = Unit.Second)]
        public double InjectTime
        {
            get { return _injectTime; }
            set
            {
                if (_injectTime == value) return;
                OnRecipeChanged(_injectTime, value);
                _injectTime = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Vent Time After Inject", Unit = Unit.Second)]
        public double VentTimeAfterInject
        {
            get { return _ventTimeAfterInject; }
            set { _ventTimeAfterInject = value; }
        }

        [SingleRecipeDescription(Description = "Vent Time", Unit = Unit.Second)]
        public double VentTime
        {
            get { return _ventTime; }
            set { _ventTime = value; }
        }

        #region Privates
        private double _injectTime;
        private double _delayTime;
        private double _ventTimeAfterInject;
        private double _ventTime;
        #endregion
    }
}
