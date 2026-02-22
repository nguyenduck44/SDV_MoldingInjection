using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectRecipe : RecipeBase
    {
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

        [SingleRecipeDescription(Description = "X-Axis dummy shot position for Head 1, 3", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisH13DummyPos
        {
            get { return _xAxisH13DummyPos; }
            set
            {
                if (_xAxisH13DummyPos == value) return;

                OnRecipeChanged(_xAxisH13DummyPos, value);
                _xAxisH13DummyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Y-Axis dummy shot position for Head 1, 3", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisH13DummyPos
        {
            get { return _yAxisH13DummyPos; }
            set
            {
                if (_yAxisH13DummyPos == value) return;

                OnRecipeChanged(_yAxisH13DummyPos, value);
                _yAxisH13DummyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "X-Axis dummy shot position for Head 2, 4", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisH24DummyPos
        {
            get { return _xAxisH24DummyPos; }
            set
            {
                if (_xAxisH24DummyPos == value) return;

                OnRecipeChanged(_xAxisH24DummyPos, value);
                _xAxisH24DummyPos = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Y-Axis dummy shot position for Head 2, 4", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisH24DummyPos
        {
            get { return _yAxisH24DummyPos; }
            set
            {
                if (_yAxisH24DummyPos == value) return;

                OnRecipeChanged(_yAxisH24DummyPos, value);
                _yAxisH24DummyPos = value;
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

        [SingleRecipeDescription(Description = "X-Axis Needle Clean", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisNeddleClean
        {
            get { return _xAxisNeedleClean; }
            set
            {
                if (_xAxisNeedleClean == value) return;

                OnRecipeChanged(_xAxisNeedleClean, value);
                _xAxisNeedleClean = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Y-Axis Neddle Clean", Unit = Unit.mm)]
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

        #region Privates
        private double _xAxisReadyPos;
        private double _xAxisInjectPos;

        private double _yAxisReadyPos;
        private double _yAxisInjectPos;

        private double _xAxisH13DummyPos;
        private double _yAxisH13DummyPos;
        private double _xAxisH24DummyPos;
        private double _yAxisH24DummyPos;

        private double _niddleCleanCycleCount;
        private double _niddleCleanShiftDist;

        private double _xAxisNeedleClean;
        private double _yAxisNeedleClean;

        #endregion
    }
}
