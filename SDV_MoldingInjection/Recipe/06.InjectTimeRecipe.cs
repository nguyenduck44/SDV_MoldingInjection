using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectTimeRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Delay Time", Unit = Unit.Second)]
        public double DelayTime
        {
            get => _delayTime;
            set => SetRecipe(ref _delayTime, value, nameof(DelayTime));
        }

        [SingleRecipeDescription(Description = "Inject Time", Unit = Unit.Second)]
        public double InjectTime
        {
            get => _injectTime;
            set => SetRecipe(ref _injectTime, value, nameof(InjectTime));
        }

        [SingleRecipeDescription(Description = "Vent Time After Inject", Unit = Unit.Second)]
        public double VentTimeAfterInject
        {
            get => _ventTimeAfterInject;
            set => SetRecipe(ref _ventTimeAfterInject, value, nameof(VentTimeAfterInject));
        }

        [SingleRecipeDescription(Description = "Vent Time", Unit = Unit.Second)]
        public double VentTime
        {
            get => _ventTime;
            set => SetRecipe(ref _ventTime, value, nameof(VentTime));
        }

        [SingleRecipeDescription(Description = "Delay after inject finish", Unit = Unit.Second)]
        public double DelayAfterInjectFinish
        {
            get => _delayAfterInjectFinish;
            set => SetRecipe(ref _delayAfterInjectFinish, value, nameof(DelayAfterInjectFinish));
        }

        #region Privates
        private double _injectTime;
        private double _delayTime;
        private double _ventTimeAfterInject;
        private double _ventTime;
        private double _delayAfterInjectFinish;
        #endregion
    }
}
