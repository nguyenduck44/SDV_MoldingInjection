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

        private double _niddleCleanCycleCount;
        private double _niddleCleanShiftDist;

        private double _yAxisNeedleClean;
        #endregion
    }
}
