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

        [SingleRecipeDescription(Description = "XAxis work (inject) position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisInjectPos
        {
            get => _xAxisInjectPos;
            set => SetRecipe(ref _xAxisInjectPos, value, nameof(XAxisInjectPos));
        }

        [SingleRecipeDescription(Description = "YAxis ready position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisReadyPos
        {
            get => _yAxisReadyPos;
            set => SetRecipe(ref _yAxisReadyPos, value, nameof(YAxisReadyPos));
        }

        [SingleRecipeDescription(Description = "YAxis work position (inject, chamber vacuum...)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisInjectPos
        {
            get => _yAxisInjectPos;
            set => SetRecipe(ref _yAxisInjectPos, value, nameof(YAxisInjectPos));
        }

        [SingleRecipeDescription(Description = "X-Axis H13 Weighting position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisH13DotWeightingPos
        {
            get => _xAxisH13DotWeightPos;
            set => SetRecipe(ref _xAxisH13DotWeightPos, value, nameof(XAxisH13DotWeightingPos));
        }

        [SingleRecipeDescription(Description = "X-Axis H24 Weighting position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisH24DotWeightingPos
        {
            get => _xAxisH24DotWeightPos;
            set => SetRecipe(ref _xAxisH24DotWeightPos, value, nameof(XAxisH24DotWeightingPos));
        }

        [SingleRecipeDescription(Description = "X-Axis Dummy Shot position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisDummyPos
        {
            get => _xAxisDummyPos;
            set => SetRecipe(ref _xAxisDummyPos, value, nameof(XAxisDummyPos));
        }

        [SingleRecipeDescription(Description = "Y-Axis Dummy Shot position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisDummyPos
        {
            get => _yAxisDummyPos;
            set => SetRecipe(ref _yAxisDummyPos, value, nameof(YAxisDummyPos));
        }

        [SingleRecipeDescription(Description = "X-Axis needle clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        public double XAxisNeedleCleanPos
        {
            get => _xAxisNeedleCleanPos;
            set => SetRecipe(ref _xAxisNeedleCleanPos, value, nameof(XAxisNeedleCleanPos));
        }

        [SingleRecipeDescription(Description = "Y-Axis needle clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        public double YAxisNeddleClean
        {
            get => _yAxisNeedleClean;
            set => SetRecipe(ref _yAxisNeedleClean, value, nameof(YAxisNeddleClean));
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

        private double _yAxisReadyPos;
        private double _yAxisInjectPos;

        private double _xAxisDummyPos;
        private double _yAxisDummyPos;

        private double _xAxisNeedleCleanPos;

        private double _niddleCleanCycleCount;
        private double _niddleCleanShiftDist;

        private double _yAxisNeedleClean;
        private double _xAxisH13DotWeightPos;
        private double _xAxisH24DotWeightPos;


        #endregion
    }
}
