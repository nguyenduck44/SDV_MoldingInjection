using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectRecipe : RecipeBase
    {
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


        [SingleRecipeDescription(Description = "XAxis ready position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisReadyPos
        {
            get { return _xAxisReadyPos; }
            set
            {
                if (_xAxisReadyPos == value) return;

                OnRecipeChanged(_xAxisReadyPos, value);
                _xAxisReadyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "XAxis work (inject) position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisInjectPos
        {
            get { return _xAxisInjectPos; }
            set
            {
                if (_xAxisInjectPos == value) return;

                OnRecipeChanged(_xAxisInjectPos, value);
                _xAxisInjectPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "YAxis ready position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisReadyPos
        {
            get { return _yAxisReadyPos; }
            set
            {
                if (_yAxisReadyPos == value) return;

                OnRecipeChanged(_yAxisReadyPos, value);
                _yAxisReadyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "YAxis work position (inject, chamber vacuum...)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisInjectPos
        {
            get { return _yAxisInjectPos; }
            set
            {
                if (_yAxisInjectPos == value) return;

                OnRecipeChanged(_yAxisInjectPos, value);
                _yAxisInjectPos = value;
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

        [SingleRecipeDescription(Description = "Y-Axis needle clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisNeddleClean
        {
            get { return _yAxisNeedleClean; }
            set
            {
                if (_yAxisNeedleClean == value) return;

                OnRecipeChanged(_yAxisNeedleClean, value);
                _yAxisNeedleClean = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Needle Clean Cycle Count")]
        public double NiddleCleanCycleCount
        {
            get { return _niddleCleanCycleCount; }
            set
            {
                if (_niddleCleanCycleCount == value) return;

                OnRecipeChanged(_niddleCleanCycleCount, value);
                _niddleCleanCycleCount = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Needle Clean Shift Dist", Unit = Unit.mm)]
        public double NiddleCleanShiftDist
        {
            get { return _niddleCleanShiftDist; }
            set
            {
                if (_niddleCleanShiftDist == value) return;

                OnRecipeChanged(_niddleCleanShiftDist, value);
                _niddleCleanShiftDist = value;
                OnPropertyChanged();
            }
        }

        #region Privates
        private double _xAxisReadyPos;
        private double _xAxisInjectPos;

        private double _yAxisReadyPos;
        private double _yAxisInjectPos;

        private double _xAxisDummyPos;
        private double _yAxisDummyPos;

        private double _xAxisNeedleCleanPos;

        private double _niddleCleanCycleCount;
        private double _niddleCleanShiftDist;

        private double _yAxisNeedleClean;

        private double _injectTime;
        private double _delayTime;
        private double _ventTimeAfterInject;
        private double _ventTime;
        #endregion
    }
}
