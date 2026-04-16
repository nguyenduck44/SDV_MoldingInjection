using EQX.Core.Recipe;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.Defines.TeachingPosition
{
    public class DryPumpMaintenanceTeachingPosition : MaintenanceTeachingPositionsBase
    {
        public PositionPoint XAxisReadyPos { get; }
        public PositionPoint YAxisReadyPos { get; }

        public MultiPointPosition ReadyGroup { get; }

        public RecipePositionManager PositionManager { get; }

        public DryPumpMaintenanceTeachingPosition(RecipePositionManager positionManager, Devices devices)
        {
            XAxisReadyPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisReadyPos,
                _currentRecipe => _currentRecipe.InjectRecipe.XAxisReadyPosOffset,
                devices.Motions.XAxis);
            YAxisReadyPos = positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos,
                _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPosOffset,
                devices.Motions.StageYAxis);

            ReadyGroup = CreateGroup("Ready", XAxisReadyPos, YAxisReadyPos);

            positionManager.GroupedPositions = new ObservableCollection<MultiPointPosition>
            {
                ReadyGroup,
            };

            PositionManager = positionManager;
        }
    }
}
