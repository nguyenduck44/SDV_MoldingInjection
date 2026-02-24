using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class SPDHeadMaintenanceViewModel : AppMaintenanceViewModel
    {
        public SPDHeadMaintenanceViewModel(Devices devices, NavigationStore navigationStore,
            MachineStatus machineStatus, RecipeSelector recipeSelector)
            : base(navigationStore, machineStatus, recipeSelector)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;

            Motions = new ObservableCollection<IMotion>();
            _positionManager = UpdatePositionManager();
            if (GroupedPositions != null && GroupedPositions.Count > 0)
            {
                SelectedGroupedPosition = GroupedPositions.FirstOrDefault()!;
            }
        }

        protected override void ActualInit()
        {
            RelatedViewModel = new List<string>
            {
                nameof(EProcess.Inject),
            };
            Motions = new ObservableCollection<IMotion>
            {
                ZAxis,
                PAxis,
                GAxis,
            };
            Cylinders = new ObservableCollection<ICylinder>
            {
                PistonCyl
            };
            Sequences = new ObservableCollection<ESemiSequence>
            {
                ESemiSequence.Ready,
                ESemiSequence.ResinInject,
                ESemiSequence.DummyShot,
                ESemiSequence.NeedleCleaning,
                ESemiSequence.BubbleRemove,
                ESemiSequence.DotWeighting,
                ESemiSequence.HeadAssemble,
                ESemiSequence.HeadDisassemble,
            };
            if (Name == EProcess.SPDHead1.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H1_GateOpen,
                    _devices.Inputs.H1_GateClose,
                    _devices.Inputs.H1_AssembleCheck,
                    _devices.Inputs.H1_SyringeCheck,
                    _devices.Inputs.H1_SyringeAir,
                };
                Outputs = new ObservableCollection<IDOutput>
                {
                    _devices.Outputs.H1_CylUp,
                    _devices.Outputs.H1_CylDown,
                };
            }
            if (Name == EProcess.SPDHead2.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H2_GateOpen,
                    _devices.Inputs.H2_GateClose,
                    _devices.Inputs.H2_AssembleCheck,
                    _devices.Inputs.H2_SyringeCheck,
                    _devices.Inputs.H2_SyringeAir,
                };
                Outputs = new ObservableCollection<IDOutput>
                {
                    _devices.Outputs.H2_CylUp,
                    _devices.Outputs.H2_CylDown,
                };
            }

            if (Name == EProcess.SPDHead3.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H3_GateOpen,
                    _devices.Inputs.H3_GateClose,
                    _devices.Inputs.H3_AssembleCheck,
                    _devices.Inputs.H3_SyringeCheck,
                    _devices.Inputs.H3_SyringeAir,
                };
                Outputs = new ObservableCollection<IDOutput>
                {
                    _devices.Outputs.H3_CylUp,
                    _devices.Outputs.H3_CylDown,
                };
            }

            if (Name == EProcess.SPDHead4.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H4_GateOpen,
                    _devices.Inputs.H4_GateClose,
                    _devices.Inputs.H4_AssembleCheck,
                    _devices.Inputs.H4_SyringeCheck,
                    _devices.Inputs.H4_SyringeAir,
                };
                Outputs = new ObservableCollection<IDOutput>
                {
                    _devices.Outputs.H4_CylUp,
                    _devices.Outputs.H4_CylDown,
                };
            }
        }

        protected override RecipePositionManagerBase<RecipeList> UpdatePositionManager()
        {
            var positionManager = new RecipePositionManager(_recipeSelector.CurrentRecipe, _devices.Motions.All);

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

            var dummyZSafetyGroup = new MultiPointPosition
            {
                Name = "Dummy Pos (Z Safety)",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                }
            };

            var dummyGroup = new MultiPointPosition
            {
                Name = "Dummy Pos",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisDummyPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisDummyPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisDummyPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisDummyPos, _devices.Motions.Z4Axis),
                }
            };

            var weightingZSafetyGroup = new MultiPointPosition
            {
                Name = "Weighting Pos (Z Safety)",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH13DotWeightingPos, _devices.Motions.XAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                }
            };

            var weightingGroup = new MultiPointPosition
            {
                Name = "Weighting Pos",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH13DotWeightingPos, _devices.Motions.XAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos, _devices.Motions.Z4Axis),
                }
            };

            positionManager.GroupedPositions.Add(readyGroup);
            positionManager.GroupedPositions.Add(dummyZSafetyGroup);
            positionManager.GroupedPositions.Add(dummyGroup);
            positionManager.GroupedPositions.Add(weightingZSafetyGroup);
            positionManager.GroupedPositions.Add(weightingGroup);

            return positionManager;
        }

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;

        private IMotion ZAxis => Name switch
        {
            "SPDHead1" => _devices.Motions.Z1Axis,
            "SPDHead2" => _devices.Motions.Z2Axis,
            "SPDHead3" => _devices.Motions.Z3Axis,
            "SPDHead4" => _devices.Motions.Z4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IMotion PAxis => Name switch
        {
            "SPDHead1" => _devices.Motions.P1Axis,
            "SPDHead2" => _devices.Motions.P2Axis,
            "SPDHead3" => _devices.Motions.P3Axis,
            "SPDHead4" => _devices.Motions.P4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IMotion GAxis => Name switch
        {
            "SPDHead1" => _devices.Motions.G1Axis,
            "SPDHead2" => _devices.Motions.G2Axis,
            "SPDHead3" => _devices.Motions.G3Axis,
            "SPDHead4" => _devices.Motions.G4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private ICylinder PistonCyl => Name switch
        {
            "SPDHead1" => _devices.Cylinders.PistonCyl_H1,
            "SPDHead2" => _devices.Cylinders.PistonCyl_H2,
            "SPDHead3" => _devices.Cylinders.PistonCyl_H3,
            "SPDHead4" => _devices.Cylinders.PistonCyl_H4,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private SPDHeadRecipe headRecipe => Name switch
        {
            "SPDHead1" => _recipeSelector.CurrentRecipe.SPDHead1_Recipe,
            "SPDHead2" => _recipeSelector.CurrentRecipe.SPDHead2_Recipe,
            "SPDHead3" => _recipeSelector.CurrentRecipe.SPDHead3_Recipe,
            "SPDHead4" => _recipeSelector.CurrentRecipe.SPDHead4_Recipe,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        #endregion
    }
}