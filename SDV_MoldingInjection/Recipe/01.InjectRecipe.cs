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
            get => _xAxisReadyPos;
            set => SetRecipe(ref _xAxisReadyPos, value, nameof(XAxisReadyPos));
        }

        public double XAxisReadyPosOffset
        {
            get => _xAxisReadyPosOffset;
            set => SetRecipe(ref _xAxisReadyPosOffset, value, nameof(XAxisReadyPosOffset));
        }

        [SingleRecipeDescription(Description = "XAxis work (inject) position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisInjectPos
        {
            get => _xAxisInjectPos;
            set => SetRecipe(ref _xAxisInjectPos, value, nameof(XAxisInjectPos));
        }

        public double XAxisInjectPosOffset
        {
            get => _xAxisInjectPosOffset;
            set => SetRecipe(ref _xAxisInjectPosOffset, value, nameof(XAxisInjectPosOffset));
        }

        [SingleRecipeDescription(Description = "YAxis ready position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisReadyPos
        {
            get => _yAxisReadyPos;
            set => SetRecipe(ref _yAxisReadyPos, value, nameof(YAxisReadyPos));
        }

        public double YAxisReadyPosOffset
        {
            get => _yAxisReadyPosOffset;
            set => SetRecipe(ref _yAxisReadyPosOffset, value, nameof(YAxisReadyPosOffset));
        }

        [SingleRecipeDescription(Description = "YAxis work position (inject, chamber vacuum...)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisInjectPos
        {
            get => _yAxisInjectPos;
            set => SetRecipe(ref _yAxisInjectPos, value, nameof(YAxisInjectPos));
        }

        public double YAxisInjectPosOffset
        {
            get => _yAxisInjectPosOffset;
            set => SetRecipe(ref _yAxisInjectPosOffset, value, nameof(YAxisInjectPosOffset));
        }

        [SingleRecipeDescription(Description = "X-Axis H13 Weighting position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisH13DotWeightingPos
        {
            get => _xAxisH13DotWeightPos;
            set => SetRecipe(ref _xAxisH13DotWeightPos, value, nameof(XAxisH13DotWeightingPos));
        }

        public double XAxisH13DotWeightingPosOffset
        {
            get => _xAxisH13DotWeightingPosOffset;
            set => SetRecipe(ref _xAxisH13DotWeightingPosOffset, value, nameof(XAxisH13DotWeightingPosOffset));
        }

        [SingleRecipeDescription(Description = "X-Axis H24 Weighting position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisH24DotWeightingPos
        {
            get => _xAxisH24DotWeightPos;
            set => SetRecipe(ref _xAxisH24DotWeightPos, value, nameof(XAxisH24DotWeightingPos));
        }

        public double XAxisH24DotWeightingPosOffset
        {
            get => _xAxisH24DotWeightingPosOffset;
            set => SetRecipe(ref _xAxisH24DotWeightingPosOffset, value, nameof(XAxisH24DotWeightingPosOffset));
        }

        [SingleRecipeDescription(Description = "X-Axis Dummy Shot position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisDummyPos
        {
            get => _xAxisDummyPos;
            set => SetRecipe(ref _xAxisDummyPos, value, nameof(XAxisDummyPos));
        }

        public double XAxisDummyPosOffset
        {
            get => _xAxisDummyPosOffset;
            set => SetRecipe(ref _xAxisDummyPosOffset, value, nameof(XAxisDummyPosOffset));
        }

        [SingleRecipeDescription(Description = "Y-Axis Dummy Shot position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisDummyPos
        {
            get => _yAxisDummyPos;
            set => SetRecipe(ref _yAxisDummyPos, value, nameof(YAxisDummyPos));
        }

        public double YAxisDummyPosOffset
        {
            get => _yAxisDummyPosOffset;
            set => SetRecipe(ref _yAxisDummyPosOffset, value, nameof(YAxisDummyPosOffset));
        }

        [SingleRecipeDescription(Description = "X-Axis needle clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisNeedleCleanPos
        {
            get => _xAxisNeedleCleanPos;
            set => SetRecipe(ref _xAxisNeedleCleanPos, value, nameof(XAxisNeedleCleanPos));
        }

        public double XAxisNeedleCleanPosOffset
        {
            get => _xAxisNeedleCleanPosOffset;
            set => SetRecipe(ref _xAxisNeedleCleanPosOffset, value, nameof(XAxisNeedleCleanPosOffset));
        }

        [SingleRecipeDescription(Description = "Y-Axis needle clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisNeedleCleanPos
        {
            get => _yAxisNeedleClean;
            set => SetRecipe(ref _yAxisNeedleClean, value, nameof(YAxisNeedleCleanPos));
        }

        public double YAxisNeedleCleanPosOffset
        {
            get => _yAxisNeedleCleanPosOffset;
            set => SetRecipe(ref _yAxisNeedleCleanPosOffset, value, nameof(YAxisNeedleCleanPosOffset));
        }

        [SingleRecipeDescription(Description = "Needle Clean Cycle Count")]
        public double NiddleCleanCycleCount
        {
            get => _niddleCleanCycleCount;
            set => SetRecipe(ref _niddleCleanCycleCount, value, nameof(NiddleCleanCycleCount));
        }

        [SingleRecipeDescription(Description = "Needle Clean Shift Dist", Unit = Unit.mm)]
        public double NiddleCleanShiftDist
        {
            get => _niddleCleanShiftDist;
            set => SetRecipe(ref _niddleCleanShiftDist, value, nameof(NiddleCleanShiftDist));
        }

        #region Privates
        private double _xAxisReadyPos;
        private double _xAxisInjectPos;
        private double _xAxisInjectPosOffset;

        private double _yAxisReadyPos;
        private double _yAxisInjectPos;
        private double _yAxisReadyPosOffset;
        private double _yAxisInjectPosOffset;

        private double _xAxisDummyPos;
        private double _yAxisDummyPos;
        private double _xAxisDummyPosOffset;
        private double _yAxisDummyPosOffset;

        private double _xAxisNeedleCleanPos;
        private double _xAxisNeedleCleanPosOffset;

        private double _niddleCleanCycleCount;
        private double _niddleCleanShiftDist;

        private double _yAxisNeedleClean;
        private double _yAxisNeedleCleanPosOffset;
        private double _xAxisH13DotWeightPos;
        private double _xAxisH24DotWeightPos;
        private double _xAxisH13DotWeightingPosOffset;
        private double _xAxisH24DotWeightingPosOffset;
        private double _xAxisReadyPosOffset;


        #endregion
    }
}
