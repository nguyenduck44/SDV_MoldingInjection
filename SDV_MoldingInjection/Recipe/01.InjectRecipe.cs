using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class InjectRecipe : RecipeBase
    {
        [SingleRecipeDescription(Description = "X-Axis Ready Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        [ECMParameterDescription(1)]
        public double XAxisReadyPos
        {
            get => _xAxisReadyPos;
            set => SetRecipe(ref _xAxisReadyPos, value);
        }

        [ParameterDescription(8)]
        public double XAxisReadyPosOffset
        {
            get => _xAxisReadyPosOffset;
            set => SetRecipe(ref _xAxisReadyPosOffset, value);
        }

        [SingleRecipeDescription(Description = "Y-Axis Ready Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        [ECMParameterDescription(2)]
        public double YAxisReadyPos
        {
            get => _yAxisReadyPos;
            set => SetRecipe(ref _yAxisReadyPos, value);
        }

        [ParameterDescription(9)]
        public double YAxisReadyPosOffset
        {
            get => _yAxisReadyPosOffset;
            set => SetRecipe(ref _yAxisReadyPosOffset, value);
        }

        [SingleRecipeDescription(Description = "X-Axis Inject Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        [ECMParameterDescription(3)]
        public double XAxisInjectPos
        {
            get => _xAxisInjectPos;
            set => SetRecipe(ref _xAxisInjectPos, value);
        }

        [ParameterDescription(10)]
        public double XAxisInjectPosOffset
        {
            get => _xAxisInjectPosOffset;
            set => SetRecipe(ref _xAxisInjectPosOffset, value);
        }
        
        [SingleRecipeDescription(Description = "Y-Axis Inject Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        [ECMParameterDescription(4)]
        public double YAxisInjectPos
        {
            get => _yAxisInjectPos;
            set => SetRecipe(ref _yAxisInjectPos, value);
        }

        [ParameterDescription(11)]
        public double YAxisInjectPosOffset
        {
            get => _yAxisInjectPosOffset;
            set => SetRecipe(ref _yAxisInjectPosOffset, value);
        }

        [SingleRecipeDescription(Description = "X-Axis H13 Weighting Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        [ECMParameterDescription(5)]
        public double XAxisH13DotWeightingPos
        {
            get => _xAxisH13DotWeightPos;
            set => SetRecipe(ref _xAxisH13DotWeightPos, value);
        }

        [ParameterDescription(12)]
        public double XAxisH13DotWeightingPosOffset
        {
            get => _xAxisH13DotWeightingPosOffset;
            set => SetRecipe(ref _xAxisH13DotWeightingPosOffset, value);
        }

        [SingleRecipeDescription(Description = "X-Axis H24 Weighting Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        [ECMParameterDescription(6)]
        public double XAxisH24DotWeightingPos
        {
            get => _xAxisH24DotWeightPos;
            set => SetRecipe(ref _xAxisH24DotWeightPos, value);
        }

        [ParameterDescription(13)]
        public double XAxisH24DotWeightingPosOffset
        {
            get => _xAxisH24DotWeightingPosOffset;
            set => SetRecipe(ref _xAxisH24DotWeightingPosOffset, value);
        }

        [SingleRecipeDescription(Description = "X-Axis Dummy Shot Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        [ECMParameterDescription(7)]
        public double XAxisDummyPos
        {
            get => _xAxisDummyPos;
            set => SetRecipe(ref _xAxisDummyPos, value);
        }

        [ParameterDescription(14)]
        public double XAxisDummyPosOffset
        {
            get => _xAxisDummyPosOffset;
            set => SetRecipe(ref _xAxisDummyPosOffset, value);
        }

        [SingleRecipeDescription(Description = "Y-Axis Dummy Shot Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        [ECMParameterDescription(8)]
        public double YAxisDummyPos
        {
            get => _yAxisDummyPos;
            set => SetRecipe(ref _yAxisDummyPos, value);
        }

        [ParameterDescription(15)]
        public double YAxisDummyPosOffset
        {
            get => _yAxisDummyPosOffset;
            set => SetRecipe(ref _yAxisDummyPosOffset, value);
        }

        [SingleRecipeDescription(Description = "X-Axis Needle Clean Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "XAxis")]
        [ECMParameterDescription(9)]
        public double XAxisNeedleCleanPos
        {
            get => _xAxisNeedleCleanPos;
            set => SetRecipe(ref _xAxisNeedleCleanPos, value);
        }

        [ParameterDescription(16)]
        public double XAxisNeedleCleanPosOffset
        {
            get => _xAxisNeedleCleanPosOffset;
            set => SetRecipe(ref _xAxisNeedleCleanPosOffset, value);
        }

        [SingleRecipeDescription(Description = "Y-Axis Needle Clean Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "YAxis")]
        [ECMParameterDescription(10)]
        public double YAxisNeedleCleanPos
        {
            get => _yAxisNeedleClean;
            set => SetRecipe(ref _yAxisNeedleClean, value);
        }

        [ParameterDescription(17)]
        public double YAxisNeedleCleanPosOffset
        {
            get => _yAxisNeedleCleanPosOffset;
            set => SetRecipe(ref _yAxisNeedleCleanPosOffset, value);
        }

        [SingleRecipeDescription(Description = "Needle Clean Cycle Count")]
        [SingleRecipeMinMax(Max = 99, Min = 1)]
        [ParameterDescription(18)]
        public int NiddleCleanCycleCount
        {
            get => _niddleCleanCycleCount;
            set => SetRecipe(ref _niddleCleanCycleCount, value);
        }

        [SingleRecipeDescription(Description = "Needle Clean Shift Dist", Unit = Unit.mm)]
        [ParameterDescription(19)]
        public double NiddleCleanShiftDist
        {
            get => _niddleCleanShiftDist;
            set => SetRecipe(ref _niddleCleanShiftDist, value);
        }

        [SingleRecipeDescription(Description = "DummyShot Cycle Count")]
        [SingleRecipeMinMax(Max = 99, Min = 1)]
        [ParameterDescription(20)]
        public int DummyShotCycleCount
        {
            get => _dummyShotCycleCount;
            set => SetRecipe(ref _dummyShotCycleCount, value);
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

        private double _niddleCleanShiftDist;
        private double _yAxisNeedleClean;
        private double _yAxisNeedleCleanPosOffset;
        private double _xAxisH13DotWeightPos;
        private double _xAxisH24DotWeightPos;
        private double _xAxisH13DotWeightingPosOffset;
        private double _xAxisH24DotWeightingPosOffset;
        private double _xAxisReadyPosOffset;

        private int _niddleCleanCycleCount;
        private int _dummyShotCycleCount;
        #endregion
    }
}
