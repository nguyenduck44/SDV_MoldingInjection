using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class InjectMaintenanceViewModel : AppMaintenanceViewModel
    {
        #region Properties
        #endregion

        public InjectMaintenanceViewModel(NavigationStore navigationStore,
            Devices devices, MachineStatus machineStatus, RecipeSelector recipeSelector)
            : base(navigationStore, machineStatus, recipeSelector)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;

            if (GroupedPositions != null && GroupedPositions.Count > 0)
            {
                SelectedGroupedPosition = GroupedPositions.FirstOrDefault()!;
            }
        }

        protected override void ActualInit()
        {
            RelatedViewModel = new List<string>
            {
                nameof(EProcess.DryPump),
                nameof(EProcess.SPDHead1),
                nameof(EProcess.SPDHead2),
                nameof(EProcess.SPDHead3),
                nameof(EProcess.SPDHead4),
            };
            Motions = new ObservableCollection<IMotion>
            {
                _devices.Motions.XAxis,
                _devices.Motions.StageYAxis,
            };
            Cylinders = new ObservableCollection<ICylinder>
            {
                _devices.Cylinders.BellowCyl,
                _devices.Cylinders.ChamberOpenClose,
                _devices.Cylinders.NozzleClean_H1,
                _devices.Cylinders.NozzleClean_H2,
                _devices.Cylinders.NozzleClean_H3,
                _devices.Cylinders.NozzleClean_H4,
            };
            Inputs = new ObservableCollection<IDInput>
            {
                _devices.Inputs.Jig1Detect,
                _devices.Inputs.Jig2Detect,
                _devices.Inputs.Jig3Detect,
                _devices.Inputs.Jig4Detect,
            };
            Outputs = new ObservableCollection<IDOutput>
            {
                _devices.Outputs.VacChamberOpen,
                _devices.Outputs.VacChamberClose,
            };
            Sequences = new ObservableCollection<ESemiSequence>
            {
                ESemiSequence.Loading,
                ESemiSequence.Unloading,
            };
        }

        protected override RecipePositionManagerBase<RecipeList> UpdatePositionManager()
        {
            var positionManager = new RecipePositionManager(_recipeSelector.CurrentRecipe, _devices.Motions.All);

            var zReadyGroup = new MultiPointPosition
            {
                Name = "Z Ready Pos",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                }
            };

            var readyGroup = new MultiPointPosition
            {
                Name = "Ready Pos",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisReadyPos, _devices.Motions.XAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                }
            };

            var injectZSafetyGroup = new MultiPointPosition
            {
                Name = "Inject Pos (Z Safety)",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPos, _devices.Motions.XAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisInjectPos, _devices.Motions.StageYAxis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                }
            };

            var injectGroup = new MultiPointPosition
            {
                Name = "Inject Pos",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPos, _devices.Motions.XAxis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisInjectPos, _devices.Motions.StageYAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisInjectPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisInjectPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisInjectPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisInjectPos, _devices.Motions.Z4Axis),
                }
            };

            positionManager.GroupedPositions.Add(zReadyGroup);
            positionManager.GroupedPositions.Add(readyGroup);
            positionManager.GroupedPositions.Add(injectZSafetyGroup);
            positionManager.GroupedPositions.Add(injectGroup);

            return positionManager;
        }

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        #endregion
    }
}