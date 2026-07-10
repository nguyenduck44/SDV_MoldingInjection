using EQX.Core.Recipe;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.Defines.TeachingPosition
{
    public class SPDHeadMaintenanceTeachingPosition : MaintenanceTeachingPositionsBase
    {
        public ESPDHead Head { get; }
        public PositionPoint XAxisReadyPos { get; }
        public PositionPoint XAxisInjectPos { get; }
        public PositionPoint YAxisReadyPos { get; }
        public PositionPoint YAxisInjectPos { get; }
        public PositionPoint XAxisH13DotWeightingPos { get; }
        public PositionPoint XAxisH24DotWeightingPos { get; }
        public PositionPoint XAxisDummyPos { get; }
        public PositionPoint YAxisDummyPos { get; }
        public PositionPoint XAxisNeedleCleanPos { get; }
        public PositionPoint YAxisNeedleCleanPos { get; }

        public PositionPoint Z1AxisSafetyPos { get; }
        public PositionPoint Z2AxisSafetyPos { get; }
        public PositionPoint Z3AxisSafetyPos { get; }
        public PositionPoint Z4AxisSafetyPos { get; }
        public PositionPoint ZAxisInjectPos { get; }
        public PositionPoint ZAxisDummyPos { get; }
        public PositionPoint ZAxisWeightingPos { get; }
        public PositionPoint ZAxisNeedleCleanPos { get; }
        public PositionPoint ZAxisAssembleDisassemblePos { get; }
        public PositionPoint ZAxisMasterNozzleCheckPos { get; }

        public MultiPointPosition ReadyGroup { get; }
        public MultiPointPosition InjectZSafetyGroup { get; }
        public MultiPointPosition InjectGroup { get; }
        public MultiPointPosition DummyZSafetyGroup { get; }
        public MultiPointPosition DummyGroup { get; }
        public MultiPointPosition WeightingZSafetyGroup { get; }
        public MultiPointPosition WeightingGroup { get; }
        public MultiPointPosition CleaningZSafetyGroup { get; }
        public MultiPointPosition CleaningGroup { get; }
        public MultiPointPosition AssembleDisassembleGroup { get; }
        public MultiPointPosition MasterNozzleCheckGroup { get; }
        public RecipePositionManager PositionManager { get; }

        public SPDHeadMaintenanceTeachingPosition(RecipePositionManager positionManager, Devices devices, ESPDHead head)
        {
            Head = head;
            XAxisReadyPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisReadyPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisReadyPosOffset,
                devices.Motions.XAxis);
            YAxisReadyPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos,
                _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPosOffset,
                devices.Motions.StageYAxis);
            
            XAxisInjectPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPosOffset,
                devices.Motions.XAxis);
            YAxisInjectPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisInjectPos,
                _currentRecipe => _currentRecipe.InjectRecipe.YAxisInjectPosOffset,
                devices.Motions.StageYAxis);
            
            XAxisH13DotWeightingPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH13DotWeightingPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisH13DotWeightingPosOffset,
                devices.Motions.XAxis);
            XAxisH24DotWeightingPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH24DotWeightingPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisH24DotWeightingPosOffset,
                devices.Motions.XAxis);
            
            XAxisDummyPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPosOffset,
                devices.Motions.XAxis);
            YAxisDummyPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos,
                _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPosOffset,
                devices.Motions.StageYAxis);
            
            XAxisNeedleCleanPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPosOffset,
                devices.Motions.XAxis);
            YAxisNeedleCleanPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeedleCleanPos,
                _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeedleCleanPosOffset,
                devices.Motions.StageYAxis);

            Z1AxisSafetyPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos,
                    _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPosOffset, devices.Motions.Z1Axis);
            Z2AxisSafetyPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos,
                    _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPosOffset, devices.Motions.Z2Axis);
            Z3AxisSafetyPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos,
                    _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPosOffset, devices.Motions.Z3Axis);
            Z4AxisSafetyPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos,
                    _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPosOffset, devices.Motions.Z4Axis);
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    ZAxisInjectPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisInjectPos,
                        _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisInjectPosOffset, devices.Motions.Z1Axis);
                    ZAxisDummyPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisDummyPos,
                        _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisDummyPosOffset, devices.Motions.Z1Axis);
                    ZAxisWeightingPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos,
                        _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisWeightingPosOffset, devices.Motions.Z1Axis);
                    ZAxisNeedleCleanPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos,
                        _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPosOffset, devices.Motions.Z1Axis);
                    ZAxisAssembleDisassemblePos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePos,
                        _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePosOffset, devices.Motions.Z1Axis);
                    ZAxisMasterNozzleCheckPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisMasterNozzleCheckPos,
                        _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisMasterNozzleCheckPosOffset, devices.Motions.Z1Axis);
                    break;
                case ESPDHead.SPDHead2:
                    ZAxisInjectPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisInjectPos,
                        _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisInjectPosOffset, devices.Motions.Z2Axis);
                    ZAxisDummyPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisDummyPos,
                        _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisDummyPosOffset, devices.Motions.Z2Axis);
                    ZAxisWeightingPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos,
                        _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisWeightingPosOffset, devices.Motions.Z2Axis);
                    ZAxisNeedleCleanPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos,
                        _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPosOffset, devices.Motions.Z2Axis);
                    ZAxisAssembleDisassemblePos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePos,
                        _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePosOffset, devices.Motions.Z2Axis);
                    ZAxisMasterNozzleCheckPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisMasterNozzleCheckPos,
                        _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisMasterNozzleCheckPosOffset, devices.Motions.Z2Axis);
                    break;
                case ESPDHead.SPDHead3:
                    ZAxisInjectPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisInjectPos,
                        _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisInjectPosOffset, devices.Motions.Z3Axis);
                    ZAxisDummyPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisDummyPos,
                        _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisDummyPosOffset, devices.Motions.Z3Axis);
                    ZAxisWeightingPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos,
                        _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisWeightingPosOffset, devices.Motions.Z3Axis);
                    ZAxisNeedleCleanPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos,
                        _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPosOffset, devices.Motions.Z3Axis);
                    ZAxisAssembleDisassemblePos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePos,
                        _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePosOffset, devices.Motions.Z3Axis);
                    ZAxisMasterNozzleCheckPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisMasterNozzleCheckPos,
                        _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisMasterNozzleCheckPosOffset, devices.Motions.Z3Axis);
                    break;
                case ESPDHead.SPDHead4:
                    ZAxisInjectPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisInjectPos,
                        _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisInjectPosOffset, devices.Motions.Z4Axis);
                    ZAxisDummyPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisDummyPos,
                        _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisDummyPosOffset, devices.Motions.Z4Axis);
                    ZAxisWeightingPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos,
                        _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisWeightingPosOffset, devices.Motions.Z4Axis);
                    ZAxisNeedleCleanPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos,
                        _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPosOffset, devices.Motions.Z4Axis);
                    ZAxisAssembleDisassemblePos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePos,
                        _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePosOffset, devices.Motions.Z4Axis);
                    ZAxisMasterNozzleCheckPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisMasterNozzleCheckPos,
                        _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisMasterNozzleCheckPosOffset, devices.Motions.Z4Axis);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(head), head, "Unsupported head for maintenance teaching position.");
            }

            ReadyGroup = CreateGroup("Ready", XAxisReadyPos, YAxisReadyPos);
            InjectZSafetyGroup = CreateGroup("Inject ZSafety", XAxisInjectPos, YAxisInjectPos);
            InjectGroup = CreateGroup("Inject", XAxisInjectPos, YAxisInjectPos, ZAxisInjectPos);
            DummyZSafetyGroup = CreateGroup("Dummy ZSafety", XAxisDummyPos, YAxisDummyPos);
            DummyGroup = CreateGroup("Dummy", XAxisDummyPos, YAxisDummyPos, ZAxisDummyPos);
            MasterNozzleCheckGroup = CreateGroup("Master Nozzle Check", XAxisInjectPos, YAxisInjectPos, ZAxisMasterNozzleCheckPos);
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    WeightingZSafetyGroup = CreateGroup("Weighting ZSafety", XAxisH13DotWeightingPos, YAxisReadyPos);
                    WeightingGroup = CreateGroup("Weighting", XAxisH13DotWeightingPos, YAxisReadyPos, ZAxisWeightingPos);
                    break;
                case ESPDHead.SPDHead2:
                    WeightingZSafetyGroup = CreateGroup("Weighting ZSafety", XAxisH24DotWeightingPos, YAxisReadyPos);
                    WeightingGroup = CreateGroup("Weighting", XAxisH24DotWeightingPos, YAxisReadyPos, ZAxisWeightingPos);
                    break;
                case ESPDHead.SPDHead3:
                    WeightingZSafetyGroup = CreateGroup("Weighting ZSafety", XAxisH13DotWeightingPos, YAxisReadyPos);
                    WeightingGroup = CreateGroup("Weighting", XAxisH13DotWeightingPos, YAxisReadyPos, ZAxisWeightingPos);
                    break;
                case ESPDHead.SPDHead4:
                    WeightingZSafetyGroup = CreateGroup("Weighting ZSafety", XAxisH24DotWeightingPos, YAxisReadyPos);
                    WeightingGroup = CreateGroup("Weighting", XAxisH24DotWeightingPos, YAxisReadyPos, ZAxisWeightingPos);
                    break;
            }
            CleaningZSafetyGroup = CreateGroup("Cleaning ZSafety", XAxisNeedleCleanPos, YAxisNeedleCleanPos);
            CleaningGroup = CreateGroup("Cleaning", XAxisNeedleCleanPos, YAxisNeedleCleanPos, ZAxisNeedleCleanPos);
            AssembleDisassembleGroup = CreateGroup("Assemble/Disassemble", XAxisDummyPos, YAxisDummyPos, ZAxisAssembleDisassemblePos);

            positionManager.GroupedPositions = new ObservableCollection<MultiPointPosition>
            {
                ReadyGroup,
                InjectZSafetyGroup,
                InjectGroup,
                MasterNozzleCheckGroup,
                DummyZSafetyGroup,
                DummyGroup,
                WeightingZSafetyGroup,
                WeightingGroup,
                CleaningZSafetyGroup,
                CleaningGroup,
                AssembleDisassembleGroup
            };

            PositionManager = positionManager;
        }

    }
}
