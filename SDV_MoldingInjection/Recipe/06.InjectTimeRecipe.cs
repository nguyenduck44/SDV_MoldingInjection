using EQX.Core.Recipe;
using EQX.Core.Units;
using Newtonsoft.Json.Linq;
using SDV_MoldingInjection.MVVM.Models;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectTimeRecipe : RecipeBase
    {
        public event Action<double> InjectTimeChanged;

        //[SingleRecipeDescription(Description = "Inject First Time", Detail = "Inject time before Open Angle Valve", Unit = Unit.Second)]
        [ParameterDescription(224)]
        public double DelayAfterOpenAngleValve
        {
            get => _delayAfterOpenAngleValve;
            set => SetRecipe(ref _delayAfterOpenAngleValve, value);
        }

        [SingleRecipeDescription(Description = "Delay Time", Detail = "Delay time after inject", Unit = Unit.Second)]
        [ParameterDescription(225)]
        public double DelayTime
        {
            get => _delayTime;
            set => SetRecipe(ref _delayTime, value);
        }

        [SingleRecipeDescription(Description = "Inject Time", Detail = "Total Time Inject", Unit = Unit.Second)]
        [ParameterDescription(226)]
        public double InjectTime
        {
            get => _injectTime;
            set
            {
                SetRecipe(ref _injectTime, value);
                InjectTimeChanged?.Invoke(value);
            }
        }

        [SingleRecipeDescription(Description = "Start Vent After Inject", Unit = Unit.Second)]
        [ParameterDescription(227)]
        public double VentTimeAfterInject
        {
            get => _ventTimeAfterInject;
            set
            {
                if (value >= InjectTime - 1) return;
                SetRecipe(ref _ventTimeAfterInject, value);
            }
        }

        [SingleRecipeDescription(Description = "Vent Time", Unit = Unit.Second)]
        [ParameterDescription(228)]
        public double VentTime
        {
            get => _ventTime;
            set => SetRecipe(ref _ventTime, value);
        }

        //[SingleRecipeDescription(Description = "Delay after inject finish", Unit = Unit.Second)]
        [ParameterDescription(229)]
        public double DelayAfterInjectFinish
        {
            get => _delayAfterInjectFinish;
            set => SetRecipe(ref _delayAfterInjectFinish, value);
        }

        [SingleRecipeDescription(Description = "DummyShot Before Inject", Detail = "DummyShot After Wait No Product", Unit = Unit.Minute)]
        [ParameterDescription(230)]
        public double DummyShotBeforeInject
        {
            get => _dummyShotBeforeInject;
            set => SetRecipe(ref _dummyShotBeforeInject, value);
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
        private double _dummyShotBeforeInject;
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
