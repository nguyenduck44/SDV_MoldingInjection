using EQX.Core.Recipe;
using EQX.Core.Units;
using Newtonsoft.Json.Linq;

namespace SDV_MoldingInjection.Recipe
{
    public class SPDHeadRecipe : RecipeBase
    {
        public event Action<double> ResinWeightChanged;

        [SingleRecipeDescription(Description = "Resin Weight", Unit = Unit.mg)]
        [SingleRecipeMinMax(Max = 380, Min = 0)]
        public double ResinWeight
        {
            get => _resinWeight;
            set
            {
                ResinWeightChanged?.Invoke(value);
                SetRecipe(ref _resinWeight, value, nameof(ResinWeight));
            }
        }

        [SingleRecipeDescription(Description = "Resin Weight Spec", Unit = Unit.mg)]
        [SingleRecipeMinMax(Max = 100, Min = 0)]
        public double ResinWeightSpec
        {
            get => _resinWeightSpec;
            set => SetRecipe(ref _resinWeightSpec, value, nameof(ResinWeightSpec));
        }

        [SingleRecipeDescription(Description = "Z-Axis SAFETY position (ready position)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [SingleRecipeMinMax(Min = -5)]
        public double ZAxisSafetyPos
        {
            get
            {
                if (_zAxisSafetyPos < -5)
                {
                    _zAxisSafetyPos = -5;
                }
                return _zAxisSafetyPos;
            }
            set
            {
                var v = value < -5 ? -5 : value;
                SetRecipe(ref _zAxisSafetyPos, v, nameof(ZAxisSafetyPos));
            }
        }

        public double ZAxisSafetyPosOffset
        {
            get => _zAxisSafetyPosOffset;
            set => SetRecipe(ref _zAxisSafetyPosOffset, value, nameof(ZAxisSafetyPosOffset));
        }

        [SingleRecipeDescription(Description = "Z-Axis INJECT position (down position)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisInjectPos
        {
            get => _zAxisInjectPos;
            set => SetRecipe(ref _zAxisInjectPos, value, nameof(ZAxisInjectPos));
        }

        public double ZAxisInjectPosOffset
        {
            get => _zAxisInjectPosOffset;
            set => SetRecipe(ref _zAxisInjectPosOffset, value, nameof(ZAxisInjectPosOffset));
        }

        [SingleRecipeDescription(Description = "Z-Axis Dummy Shot position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisDummyPos
        {
            get => _zAxisDummyPos;
            set => SetRecipe(ref _zAxisDummyPos, value, nameof(ZAxisDummyPos));
        }

        public double ZAxisDummyPosOffset
        {
            get => _zAxisDummyPosOffset;
            set => SetRecipe(ref _zAxisDummyPosOffset, value, nameof(ZAxisDummyPosOffset));
        }

        [SingleRecipeDescription(Description = "Z-Axis Assemble-Disassemble position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisAssembleDisassemblePos
        {
            get => _zAxisAssembleDisassemble;
            set => SetRecipe(ref _zAxisAssembleDisassemble, value, nameof(ZAxisAssembleDisassemblePos));
        }

        public double ZAxisAssembleDisassemblePosOffset
        {
            get => _zAxisAssembleDisassemblePosOffset;
            set => SetRecipe(ref _zAxisAssembleDisassemblePosOffset, value, nameof(ZAxisAssembleDisassemblePosOffset));
        }

        [SingleRecipeDescription(Description = "Z-Axis Neddle Clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisNeedleCleanPos
        {
            get => _zAxisNeedleCleanPos;
            set => SetRecipe(ref _zAxisNeedleCleanPos, value, nameof(ZAxisNeedleCleanPos));
        }

        public double ZAxisNeedleCleanPosOffset
        {
            get => _zAxisNeedleCleanPosOffset;
            set => SetRecipe(ref _zAxisNeedleCleanPosOffset, value, nameof(ZAxisNeedleCleanPosOffset));
        }

        [SingleRecipeDescription(Description = "Z-Axis Weighting position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        public double ZAxisWeightingPos
        {
            get => _zAxisWeightingPos;
            set => SetRecipe(ref _zAxisWeightingPos, value, nameof(ZAxisWeightingPos));
        }

        public double ZAxisWeightingPosOffset
        {
            get => _zAxisWeightingPosOffset;
            set => SetRecipe(ref _zAxisWeightingPosOffset, value, nameof(ZAxisWeightingPosOffset));
        }

        [SingleRecipeDescription(Description = "Gate open position (Nozzle <----> SPD)", Unit = Unit.Degree)]
        public double GateOpenPos
        {
            get => _gateOpenPos;
            set => SetRecipe(ref _gateOpenPos, value, nameof(GateOpenPos));
        }

        public double GateOpenPosOffset
        {
            get => _gateOpenPosOffset;
            set => SetRecipe(ref _gateOpenPosOffset, value, nameof(GateOpenPosOffset));
        }

        [SingleRecipeDescription(Description = "Gate close position (Nozzle <--|--> SPD)", Unit = Unit.Degree)]
        public double GateClosePos
        {
            get => _gateClosePos;
            set => SetRecipe(ref _gateClosePos, value, nameof(GateClosePos));
        }

        public double GateClosePosOffset
        {
            get => _gateClosePosOffset;
            set => SetRecipe(ref _gateClosePosOffset, value, nameof(GateClosePosOffset));
        }

        [SingleRecipeDescription(Description = "DummyShot Weight", Unit = Unit.mg)]
        [SingleRecipeMinMax(Max = 380, Min = 0)]
        public double DummyShotWeight
        {
            get => _dummyShotWeight;
            set => SetRecipe(ref _dummyShotWeight, value, nameof(DummyShotWeight));
        }

        [SingleRecipeDescription(Description = "Bubble Remove Weight", Unit = Unit.mg)]
        [SingleRecipeMinMax(Max = 380, Min = 0)]
        public double BubbleRemoveWeight
        {
            get => _bubbleRemoveWeight;
            set => SetRecipe(ref _bubbleRemoveWeight, value, nameof(BubbleRemoveWeight));
        }

        public double PAxisInjectChargePos
        {
            get => _pAxisInjectChargePos;
            set => SetRecipe(ref _pAxisInjectChargePos, value, nameof(PAxisInjectChargePos));
        }

        public double PAxisInjectChargePosOffset
        {
            get => _pAxisInjectChargePosOffset;
            set => SetRecipe(ref _pAxisInjectChargePosOffset, value, nameof(PAxisInjectChargePosOffset));
        }

        #region Privates
        private double _zAxisSafetyPos;
        private double _zAxisSafetyPosOffset;
        private double _zAxisInjectPos;
        private double _zAxisInjectPosOffset;
        private double _zAxisNeedleCleanPos;
        private double _zAxisNeedleCleanPosOffset;
        private double _zAxisDummyPos;
        private double _zAxisDummyPosOffset;
        private double _zAxisAssembleDisassemble;
        private double _zAxisAssembleDisassemblePosOffset;
        private double _zAxisWeightingPos;
        private double _zAxisWeightingPosOffset;

        private double _gateClosePos;
        private double _gateClosePosOffset;
        private double _gateOpenPos;
        private double _gateOpenPosOffset;

        private double _pAxisInjectChargePos;
        private double _pAxisInjectChargePosOffset;

        private double _resinWeight;
        private double _resinWeightSpec;
        private double _dummyShotWeight;
        private double _bubbleRemoveWeight;

        #endregion
    }
}
