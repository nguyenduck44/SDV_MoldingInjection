using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectTimeRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Delay Time", Unit = Unit.Second)]
        [SingleRecipeMinMax(Min = 0)]
        public double DelayTime
        {
            get => _delayTime;
            set => SetRecipe(ref _delayTime, value, nameof(DelayTime));
        }

        [SingleRecipeDescription(Description = "Inject Time", Unit = Unit.Second)]
        [SingleRecipeMinMax(Min = 0)]
        public double InjectTime
        {
            get => _injectTime;
            set => SetRecipe(ref _injectTime, value, nameof(InjectTime));
        }

        [SingleRecipeDescription(Description = "Vent Time After Inject", Unit = Unit.Second)]
        [SingleRecipeMinMax(Min = 0)]
        public double VentTimeAfterInject
        {
            get => _ventTimeAfterInject;
            set => SetRecipe(ref _ventTimeAfterInject, value, nameof(VentTimeAfterInject));
        }

        [SingleRecipeDescription(Description = "Vent Time", Unit = Unit.Second)]
        [SingleRecipeMinMax(Min = 0)]
        public double VentTime
        {
            get => _ventTime;
            set => SetRecipe(ref _ventTime, value, nameof(VentTime));
        }

        #region Privates
        private double _injectTime;
        private double _delayTime;
        private double _ventTimeAfterInject;
        private double _ventTime;
        #endregion
    }
}
