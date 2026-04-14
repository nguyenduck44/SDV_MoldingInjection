using EQX.Core.Recipe;
using EQX.Core.Units;
using SDV_MoldingInjection.MVVM.Models;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectTimeRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Inject First Time", Detail = "Inject time before Open Angle Valve", Unit = Unit.Second)]
        public double DelayAfterOpenAngleValve
        {
            get => _delayAfterOpenAngleValve;
            set => SetRecipe(ref _delayAfterOpenAngleValve, value, nameof(DelayAfterOpenAngleValve));
        }

        [SingleRecipeDescription(Description = "Delay Time", Detail = "Delay time after inject", Unit = Unit.Second)]
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

        public List<InjectPath> H1H3InjectPaths => new List<InjectPath>
        {
            H1H3InjectPaths_No1,
            H1H3InjectPaths_No2,
            H1H3InjectPaths_No3,
            H1H3InjectPaths_No4,
            H1H3InjectPaths_No5,
        };

        public List<InjectPath> H2H4InjectPaths => new List<InjectPath>
        {
            H2H4InjectPaths_No1,
            H2H4InjectPaths_No2,
            H2H4InjectPaths_No3,
            H2H4InjectPaths_No4,
            H2H4InjectPaths_No5,
        };

        public InjectPath H1H3InjectPaths_No1
        {
            get => _h1H3InjectPaths_No1;
            set => SetRecipe(ref _h1H3InjectPaths_No1, value, nameof(H1H3InjectPaths_No1));
        }
        public InjectPath H1H3InjectPaths_No2
        {
            get => _h1H3InjectPaths_No2;
            set => SetRecipe(ref _h1H3InjectPaths_No2, value, nameof(H1H3InjectPaths_No2));
        }
        public InjectPath H1H3InjectPaths_No3
        {
            get => _h1H3InjectPaths_No3;
            set => SetRecipe(ref _h1H3InjectPaths_No3, value, nameof(H1H3InjectPaths_No3));
        }
        public InjectPath H1H3InjectPaths_No4
        {
            get => _h1H3InjectPaths_No4;
            set => SetRecipe(ref _h1H3InjectPaths_No4, value, nameof(H1H3InjectPaths_No4));
        }
        public InjectPath H1H3InjectPaths_No5
        {
            get => _h1H3InjectPaths_No5;
            set => SetRecipe(ref _h1H3InjectPaths_No5, value, nameof(H1H3InjectPaths_No5));
        }

        public InjectPath H2H4InjectPaths_No1
        {
            get => _h2H4InjectPaths_No1;
            set => SetRecipe(ref _h2H4InjectPaths_No1, value, nameof(H2H4InjectPaths_No1));
        }
        public InjectPath H2H4InjectPaths_No2
        {
            get => _h2H4InjectPaths_No2;
            set => SetRecipe(ref _h2H4InjectPaths_No2, value, nameof(H2H4InjectPaths_No2));
        }
        public InjectPath H2H4InjectPaths_No3
        {
            get => _h2H4InjectPaths_No3;
            set => SetRecipe(ref _h2H4InjectPaths_No3, value, nameof(H2H4InjectPaths_No3));
        }
        public InjectPath H2H4InjectPaths_No4
        {
            get => _h2H4InjectPaths_No4;
            set => SetRecipe(ref _h2H4InjectPaths_No4, value, nameof(H2H4InjectPaths_No4));
        }
        public InjectPath H2H4InjectPaths_No5
        {
            get => _h2H4InjectPaths_No5;
            set => SetRecipe(ref _h2H4InjectPaths_No5, value, nameof(H2H4InjectPaths_No5));
        }

        #region Privates
        private double _injectTime;
        private double _delayTime;
        private double _ventTimeAfterInject;
        private double _ventTime;
        private double _delayAfterInjectFinish;
        private double _delayAfterOpenAngleValve;
        private InjectPath _h1H3InjectPaths_No1 = new InjectPath();
        private InjectPath _h1H3InjectPaths_No2 = new InjectPath();
        private InjectPath _h1H3InjectPaths_No3 = new InjectPath();
        private InjectPath _h1H3InjectPaths_No4 = new InjectPath();
        private InjectPath _h1H3InjectPaths_No5 = new InjectPath();
        private InjectPath _h2H4InjectPaths_No1 = new InjectPath();
        private InjectPath _h2H4InjectPaths_No2 = new InjectPath();
        private InjectPath _h2H4InjectPaths_No3 = new InjectPath();
        private InjectPath _h2H4InjectPaths_No4 = new InjectPath();
        private InjectPath _h2H4InjectPaths_No5 = new InjectPath();
        #endregion
    }
}
