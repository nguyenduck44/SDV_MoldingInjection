using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class SPDHeadRecipe : RecipeBase
    {
        public bool HeadSkip
        {
            get { return _headSkip; }
            set
            {
                if (_headSkip == value) return;

                OnRecipeChanged(_headSkip, value);
                _headSkip = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "ZAxis SAFETY position (ready position)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisSafetyPos
        {
            get { return _zAxisSafetyPos; }
            set
            {
                if (_zAxisSafetyPos == value) return;

                OnRecipeChanged(_zAxisSafetyPos, value);
                _zAxisSafetyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "ZAxis INJECT position (down position)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisInjectPos
        {
            get { return _zAxisInjectPos; }
            set
            {
                if (_zAxisInjectPos == value) return;

                OnRecipeChanged(_zAxisInjectPos, value);
                _zAxisInjectPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "ZAxis Dummy position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisDummyPos
        {
            get { return _zAxisDummyPos; }
            set
            {
                if (_zAxisDummyPos == value) return;

                OnRecipeChanged(_zAxisDummyPos, value);
                _zAxisDummyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "ZAxis Neddle Clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisNeedleCleanPos
        {
            get { return _zAxisNeedleCleanPos; }
            set
            {
                if (_zAxisNeedleCleanPos == value) return;

                OnRecipeChanged(_zAxisNeedleCleanPos, value);
                _zAxisNeedleCleanPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Gate open position (Nozzle <----> SPD)", Unit = Unit.Degree)]
        public double GateOpenPos
        {
			get { return _gateOpenPos; }
            set
            {
                if (_gateOpenPos == value) return;

                OnRecipeChanged(_gateOpenPos, value);
                _gateOpenPos = value;
                OnPropertyChanged();
            }
        }


        [SingleRecipeDescription(Description = "Gate close position (Nozzle <--|--> SPD)", Unit = Unit.Degree)]
        public double GateClosePos
        {
            get { return _gateClosePos; }
            set
            {
                if (_gateClosePos == value) return;

                OnRecipeChanged(_gateClosePos, value);
                _gateClosePos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "PAxis INJECT position", Unit = Unit.Degree)]
        public double PAxisInjectPos
        {
            get { return _pAxisInjectPos; }
            set
            {
                if (_pAxisInjectPos == value) return;

                OnRecipeChanged(_pAxisInjectPos, value);
                _pAxisInjectPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Bubble remove turn")]
        public double BubbleRemoveTurn
        {
            get { return _bubbleRemoveTurn; }
            set
            {
                if (_bubbleRemoveTurn == value) return;

                OnRecipeChanged(_bubbleRemoveTurn, value);
                _bubbleRemoveTurn = value;
                OnPropertyChanged();
            }
        }

        public double PAxisInjectChargePos
        {
            get { return _pAxisInjectChargePos; }
            set
            {
                if (_pAxisInjectChargePos == value) return;

                OnRecipeChanged(_pAxisInjectChargePos, value);
                _pAxisInjectChargePos = value;
                OnPropertyChanged();
            }
        }
        #region Privates
        private bool _headSkip;

        private double _zAxisSafetyPos;
        private double _zAxisInjectPos;
        private double _zAxisNeedleCleanPos;
        private double _zAxisDummyPos;

        private double _gateClosePos;
        private double _gateOpenPos;

        private double _pAxisInjectPos;
        private double _pAxisInjectChargePos;

        private double _bubbleRemoveTurn;

        #endregion
    }
}
