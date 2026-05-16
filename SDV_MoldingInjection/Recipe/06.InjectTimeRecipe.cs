using EQX.Core.Recipe;
using EQX.Core.Units;
using SDV_MoldingInjection.MVVM.Models;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectTimeRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Inject First Time", Detail = "Inject time before Open Angle Valve", Unit = Unit.Second)]
        [ParameterDescription(207)]
        public double DelayAfterOpenAngleValve
        {
            get => _delayAfterOpenAngleValve;
            set => SetRecipe(ref _delayAfterOpenAngleValve, value);
        }

        [SingleRecipeDescription(Description = "Delay Time", Detail = "Delay time after inject", Unit = Unit.Second)]
        [ParameterDescription(208)]
        public double DelayTime
        {
            get => _delayTime;
            set => SetRecipe(ref _delayTime, value);
        }

        [SingleRecipeDescription(Description = "Inject Time", Unit = Unit.Second)]
        [ParameterDescription(209)]
        public double InjectTime
        {
            get => _injectTime;
            set => SetRecipe(ref _injectTime, value);
        }

        [SingleRecipeDescription(Description = "Vent Time After Inject", Unit = Unit.Second)]
        [ParameterDescription(210)]
        public double VentTimeAfterInject
        {
            get => _ventTimeAfterInject;
            set => SetRecipe(ref _ventTimeAfterInject, value);
        }

        [SingleRecipeDescription(Description = "Vent Time", Unit = Unit.Second)]
        [ParameterDescription(211)]
        public double VentTime
        {
            get => _ventTime;
            set => SetRecipe(ref _ventTime, value);
        }

        [SingleRecipeDescription(Description = "Delay after inject finish", Unit = Unit.Second)]
        [ParameterDescription(212)]
        public double DelayAfterInjectFinish
        {
            get => _delayAfterInjectFinish;
            set => SetRecipe(ref _delayAfterInjectFinish, value);
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
            set => SetRecipe(ref _h1H3InjectPaths_No1, value);
        }
        public InjectPath H1H3InjectPaths_No2
        {
            get => _h1H3InjectPaths_No2;
            set => SetRecipe(ref _h1H3InjectPaths_No2, value);
        }
        public InjectPath H1H3InjectPaths_No3
        {
            get => _h1H3InjectPaths_No3;
            set => SetRecipe(ref _h1H3InjectPaths_No3, value);
        }
        public InjectPath H1H3InjectPaths_No4
        {
            get => _h1H3InjectPaths_No4;
            set => SetRecipe(ref _h1H3InjectPaths_No4, value);
        }
        public InjectPath H1H3InjectPaths_No5
        {
            get => _h1H3InjectPaths_No5;
            set => SetRecipe(ref _h1H3InjectPaths_No5, value);
        }

        public InjectPath H2H4InjectPaths_No1
        {
            get => _h2H4InjectPaths_No1;
            set => SetRecipe(ref _h2H4InjectPaths_No1, value);
        }
        public InjectPath H2H4InjectPaths_No2
        {
            get => _h2H4InjectPaths_No2;
            set => SetRecipe(ref _h2H4InjectPaths_No2, value);
        }
        public InjectPath H2H4InjectPaths_No3
        {
            get => _h2H4InjectPaths_No3;
            set => SetRecipe(ref _h2H4InjectPaths_No3, value);
        }
        public InjectPath H2H4InjectPaths_No4
        {
            get => _h2H4InjectPaths_No4;
            set => SetRecipe(ref _h2H4InjectPaths_No4, value);
        }
        public InjectPath H2H4InjectPaths_No5
        {
            get => _h2H4InjectPaths_No5;
            set => SetRecipe(ref _h2H4InjectPaths_No5, value);
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
