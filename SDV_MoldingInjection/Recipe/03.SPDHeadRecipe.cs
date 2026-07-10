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
        [ParameterDescription(40)]
        public double ResinWeight
        {
            get => _resinWeight;
            set
            {
                ResinWeightChanged?.Invoke(value);
                SetRecipe(ref _resinWeight, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
            }
        }

        [SingleRecipeDescription(Description = "Resin Weight Spec", Unit = Unit.Percentage)]
        [SingleRecipeMinMax(Max = 100, Min = 0)]
        [ParameterDescription(41)]
        public double ResinWeightSpec
        {
            get => _resinWeightSpec;
            set => SetRecipe(ref _resinWeightSpec, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Z-Axis SAFETY Position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [SingleRecipeMinMax(Min = -5)]
        [ParameterDescription(42)]
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
                SetRecipe(ref _zAxisSafetyPos, v, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
            }
        }

        [ParameterDescription(43)]
        public double ZAxisSafetyPosOffset
        {
            get => _zAxisSafetyPosOffset;
            set => SetRecipe(ref _zAxisSafetyPosOffset, value);
        }

        [SingleRecipeDescription(Description = "Z-Axis INJECT position (down position)", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [ParameterDescription(44)]
        public double ZAxisInjectPos
        {
            get => _zAxisInjectPos;
            set => SetRecipe(ref _zAxisInjectPos, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(45)]
        public double ZAxisInjectPosOffset
        {
            get => _zAxisInjectPosOffset;
            set => SetRecipe(ref _zAxisInjectPosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Z-Axis INJECT GAP", Detail ="Distance With Inject Position Move With Slow Speed", Unit = Unit.mm)]
        [ParameterDescription(46)]
        public double ZAxisInjectUpDistance
        {
            get => _zxisInjectUpDistance;
            set => SetRecipe(ref _zxisInjectUpDistance, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Z-Axis Inject Slow Speed", Detail = "Z Axis Move Inject Position With Slow Speed", Unit = Unit.mmPerSecond)]
        [ParameterDescription(47)]
        public double ZAxisInjectSlowSpeed
        {
            get => _zAxisInjectSlowSpeed;
            set => SetRecipe(ref _zAxisInjectSlowSpeed, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }


        [SingleRecipeDescription(Description = "Z-Axis Dummy Shot position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [ParameterDescription(48)]
        public double ZAxisDummyPos
        {
            get => _zAxisDummyPos;
            set => SetRecipe(ref _zAxisDummyPos, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(49)]
        public double ZAxisDummyPosOffset
        {
            get => _zAxisDummyPosOffset;
            set => SetRecipe(ref _zAxisDummyPosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Z-Axis Assemble-Disassemble position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [ParameterDescription(50)]
        public double ZAxisAssembleDisassemblePos
        {
            get => _zAxisAssembleDisassemble;
            set => SetRecipe(ref _zAxisAssembleDisassemble, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(51)]
        public double ZAxisAssembleDisassemblePosOffset
        {
            get => _zAxisAssembleDisassemblePosOffset;
            set => SetRecipe(ref _zAxisAssembleDisassemblePosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Z-Axis Neddle Clean position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [ParameterDescription(52)]
        public double ZAxisNeedleCleanPos
        {
            get => _zAxisNeedleCleanPos;
            set => SetRecipe(ref _zAxisNeedleCleanPos, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(53)]
        public double ZAxisNeedleCleanPosOffset
        {
            get => _zAxisNeedleCleanPosOffset;
            set => SetRecipe(ref _zAxisNeedleCleanPosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Z-Axis Weighting position", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [ParameterDescription(54)]
        public double ZAxisWeightingPos
        {
            get => _zAxisWeightingPos;
            set => SetRecipe(ref _zAxisWeightingPos, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(55)]
        public double ZAxisWeightingPosOffset
        {
            get => _zAxisWeightingPosOffset;
            set => SetRecipe(ref _zAxisWeightingPosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Z-Axis Master Nozzle Check Postion", Unit = Unit.mm)]
        [SinglePositionTeaching(Motion = "ZAxis")]
        [ParameterDescription(56)]
        public double ZAxisMasterNozzleCheckPos
        {
            get => _zAxisMasterNozzleCheckPos;
            set => SetRecipe(ref _zAxisMasterNozzleCheckPos, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(57)]
        public double ZAxisMasterNozzleCheckPosOffset
        {
            get => _zAxisMasterNozzleCheckPosOffset;
            set => SetRecipe(ref _zAxisMasterNozzleCheckPosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Gate open position (Nozzle <----> SPD)", Unit = Unit.Degree)]
        [ParameterDescription(58)]
        public double GateOpenPos
        {
            get => _gateOpenPos;
            set => SetRecipe(ref _gateOpenPos, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(59)]
        public double GateOpenPosOffset
        {
            get => _gateOpenPosOffset;
            set => SetRecipe(ref _gateOpenPosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Gate close position (Nozzle <--|--> SPD)", Unit = Unit.Degree)]
        [ParameterDescription(60)]
        public double GateClosePos
        {
            get => _gateClosePos;
            set => SetRecipe(ref _gateClosePos, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(61)]
        public double GateClosePosOffset
        {
            get => _gateClosePosOffset;
            set => SetRecipe(ref _gateClosePosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "DummyShot Weight", Unit = Unit.mg)]
        [SingleRecipeMinMax(Max = 380, Min = 0)]
        [ParameterDescription(62)]
        public double DummyShotWeight
        {
            get => _dummyShotWeight;
            set => SetRecipe(ref _dummyShotWeight, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [SingleRecipeDescription(Description = "Bubble Remove Weight", Unit = Unit.mg)]
        [SingleRecipeMinMax(Max = 380, Min = 0)]
        [ParameterDescription(63)]
        public double BubbleRemoveWeight
        {
            get => _bubbleRemoveWeight;
            set => SetRecipe(ref _bubbleRemoveWeight, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(64)]
        public double PAxisInjectChargePos
        {
            get => _pAxisInjectChargePos;
            set => SetRecipe(ref _pAxisInjectChargePos, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
        }

        [ParameterDescription(65)]
        public double PAxisInjectChargePosOffset
        {
            get => _pAxisInjectChargePosOffset;
            set => SetRecipe(ref _pAxisInjectChargePosOffset, value, Name == "SPDHead1_Recipe" ? 0 : Name == "SPDHead2_Recipe" ? 40 : Name == "SPDHead3_Recipe" ? 80 : 120);
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
        private double _zxisInjectUpDistance;
        private double _zAxisInjectSlowSpeed;
        private double _zAxisMasterNozzleCheckPos;
        private double _zAxisMasterNozzleCheckPosOffset;

        #endregion
    }
}
