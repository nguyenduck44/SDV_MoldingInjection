using EQX.Core.Recipe;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.Defines.TeachingPosition
{
    public class InjectMaintenanceTeachingPosition : MaintenanceTeachingPositionsBase
    {
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
        public PositionPoint Z1AxisInjectPos { get; }
        public PositionPoint Z2AxisInjectPos { get; }
        public PositionPoint Z3AxisInjectPos { get; }
        public PositionPoint Z4AxisInjectPos { get; }


        public MultiPointPosition ZAxisReadyGroup { get; }
        public MultiPointPosition ReadyGroup { get; }
        public MultiPointPosition InjectZSafetyGroup { get; }
        public MultiPointPosition InjectGroup { get; }

        public RecipePositionManager PositionManager { get; }

        public InjectMaintenanceTeachingPosition(RecipePositionManager positionManager, Devices devices)
        {
            XAxisReadyPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisReadyPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisReadyPosOffset,
                devices.Motions.XAxis);
            XAxisInjectPos = positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPosOffset,
                devices.Motions.XAxis);
            YAxisReadyPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos,
                _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPosOffset,
                devices.Motions.StageYAxis);
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
            Z1AxisInjectPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisInjectPos,
                        _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisInjectPosOffset, devices.Motions.Z1Axis);
            Z2AxisInjectPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisInjectPos,
                        _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisInjectPosOffset, devices.Motions.Z2Axis);
            Z3AxisInjectPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisInjectPos,
                        _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisInjectPosOffset, devices.Motions.Z3Axis);
            Z4AxisInjectPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisInjectPos,
                        _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisInjectPosOffset, devices.Motions.Z4Axis);

            ReadyGroup = CreateGroup("Ready", XAxisReadyPos, YAxisReadyPos);
            ZAxisReadyGroup = CreateGroup("ZAxis Ready", Z1AxisSafetyPos, Z2AxisSafetyPos, Z3AxisSafetyPos, Z4AxisSafetyPos);
            InjectZSafetyGroup = CreateGroup("Inject ZSafety", XAxisInjectPos, YAxisInjectPos);
            InjectGroup = CreateGroup("Inject", XAxisInjectPos, YAxisInjectPos, Z1AxisInjectPos, Z2AxisInjectPos, Z3AxisInjectPos, Z4AxisInjectPos);

            positionManager.GroupedPositions = new ObservableCollection<MultiPointPosition>
            {
                ReadyGroup,
                ZAxisReadyGroup,
                InjectZSafetyGroup,
                InjectGroup
            };

            PositionManager = positionManager;
        }
    }
}
