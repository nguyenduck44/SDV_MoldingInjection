using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class SPDHeadRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "Skip Head", Detail = "Check to Skip Head")]
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

        [SingleRecipeDescription(Description = "Z-Axis SAFETY position (ready position)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [SingleRecipeMinMax(Min = -5)]
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

        [SingleRecipeDescription(Description = "Z-Axis INJECT position (down position)", Unit = Unit.mm)]
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

        [SingleRecipeDescription(Description = "X-Axis Dummy Shot position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisDummyPos
        {
            get { return _xAxisDummyPos; }
            set
            {
                if (_xAxisDummyPos == value) return;

                OnRecipeChanged(_xAxisDummyPos, value);
                _xAxisDummyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Y-Axis Dummy Shot position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisDummyPos
        {
            get { return _yAxisDummyPos; }
            set
            {
                if (_yAxisDummyPos == value) return;

                OnRecipeChanged(_yAxisDummyPos, value);
                _yAxisDummyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Z-Axis Dummy Shot position", Unit = Unit.mm)]
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

        [SingleRecipeDescription(Description = "Z-Axis Bubble Remove position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisBubbleRemovePos
        {
            get { return _zAxisBubbleRemove; }
            set
            {
                if (_zAxisBubbleRemove == value) return;

                OnRecipeChanged(_zAxisBubbleRemove, value);
                _zAxisBubbleRemove = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Z-Axis Assemble-Disassemble position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisAssembleDisassemblePos
        {
            get { return _zAxisAssembleDisassemble; }
            set
            {
                if (_zAxisAssembleDisassemble == value) return;

                OnRecipeChanged(_zAxisAssembleDisassemble, value);
                _zAxisAssembleDisassemble = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "X-Axis needle clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisNeedleCleanPos
        {
            get { return _xAxisNeedleCleanPos; }
            set
            {
                if (_xAxisNeedleCleanPos == value) return;
                OnRecipeChanged(_xAxisNeedleCleanPos, value);
                _xAxisNeedleCleanPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Z-Axis Neddle Clean position", Unit = Unit.mm)]
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

        [SingleRecipeDescription(Description = "X-Axis Weighting position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisDotWeightingPos
        {
            get { return _xAxisDotWeightPos; }
            set
            {
                if (_xAxisDotWeightPos == value) return;
                OnRecipeChanged(_xAxisDotWeightPos, value);
                _xAxisDotWeightPos = value;
                OnPropertyChanged();
            }
        }


        [SingleRecipeDescription(Description = "Z-Axis Weighting position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisWeightingPos
        {
            get { return _zAxisWeightingPos; }
            set
            {
                if (_zAxisWeightingPos == value) return;

                OnRecipeChanged(_zAxisWeightingPos, value);
                _zAxisWeightingPos = value;
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

        [SingleRecipeDescription(Description = "P-Axis INJECT position", Unit = Unit.mm)]
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

        [SingleRecipeDescription(Description = "Resin Weight", Unit = Unit.mg)]
        public double ResinWeight
        {
            get { return _resinWeight; }
            set
            {
                if (_resinWeight == value) return;
                OnRecipeChanged(_resinWeight, value);
                _resinWeight = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Resin Weight Spec", Unit = Unit.mg)]
        public double ResinWeightSpec
        {
            get { return _resinWeightSpec; }
            set
            {
                if (_resinWeightSpec == value) return;
                OnRecipeChanged(_resinWeightSpec, value);
                _resinWeightSpec = value;
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

        private double _xAxisDummyPos;
        private double _yAxisDummyPos;

        private double _zAxisSafetyPos;
        private double _zAxisInjectPos;
        private double _zAxisNeedleCleanPos;
        private double _zAxisDummyPos;
        private double _zAxisBubbleRemove;
        private double _zAxisAssembleDisassemble;
        private double _xAxisDotWeightPos;
        private double _zAxisWeightingPos;

        private double _gateClosePos;
        private double _gateOpenPos;

        private double _pAxisInjectPos;
        private double _pAxisInjectChargePos;

        private double _xAxisNeedleCleanPos;

        private double _resinWeight;
        private double _resinWeightSpec;
        private double _injectTime;

        #endregion
    }
}
