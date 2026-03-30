using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.Device.Balance;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class SPDHeadMaintenanceViewModel : AppMaintenanceViewModel
    {
        public SPDHeadMaintenanceViewModel(
            Devices devices,
            NavigationStore navigationStore,
            Balances balances,
            MachineStatus machineStatus,
            RecipeSelector recipeSelector)
            : base(navigationStore, machineStatus, recipeSelector)
        {
            _devices = devices;
            _balances = balances;
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;

            Motions = new ObservableCollection<IMotion>();
            if (GroupedPositions != null && GroupedPositions.Count > 0)
            {
                SelectedGroupedPosition = GroupedPositions.FirstOrDefault()!;
            }

        }

        protected override void ExternalTimerElapsedAction()
        {
            if (Balance.WeightData != null)
            {
                BalanceStableWeight = Balance.WeightData.Weight * (Balance.WeightData.Unit == "g" ? 1000 : 1);
                if ((MachineStatus as MachineStatus)!.IsStandByProcessMode)
                {
                    Balance.SendRequestImmediateWeightCommand();
                }
            }
        }

        public double BalanceStableWeight
        {
            get { return balanceStableWeight; }
            set
            {
                balanceStableWeight = value;
                OnPropertyChanged();
            }
        }

        public ICommand DotWeightingCalibCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ESemiSequence seq = ESemiSequence.DotWeighting_H1 + (int)(Head - ESPDHead.SPDHead1);

                    MachineStatus.OPCommand = EOperationCommand.SemiAuto;
                    MachineStatus.SemiAutoSequence = seq;
                });
            }
        }

        public ICommand SkipCalCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog($"Do you want to Skip Cal?") == true)
                    {
                    _machineStatus.MachineCalibrationSkip[(int)(Head - ESPDHead.SPDHead1)] = true;
                    }
                });
            }
        }

        public bool IsSkipCal => _machineStatus.MachineCalibrationSkip[(int)(Head - ESPDHead.SPDHead1)];
        private MettlerToledoWKC204C Balance
        {
            get
            {
                if (Head == ESPDHead.SPDHead1 || Head == ESPDHead.SPDHead2)
                {
                    return _balances.BalanceLeft;
                }
                else
                {
                    return _balances.BalanceRight;
                }
            }
        }

        public ICommand BalanceSetZeroCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Balance.SendZeroCommand();
                });
            }
        }

        public ICommand BalanceGetMeasureCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Balance.SendRequestStableWeightCommand();
                });
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
                PistonCyl,
                NozzleClean,
            };
            Sequences = new ObservableCollection<ESemiSequence>
            {
                ESemiSequence.Ready,
                ESemiSequence.ResinInject,
                ESemiSequence.DummyShot_H1 + (int)(Head - ESPDHead.SPDHead1),
                ESemiSequence.NeedleCleaning_H1 + (int)(Head - ESPDHead.SPDHead1),
                ESemiSequence.BubbleRemove_H1 + (int)(Head - ESPDHead.SPDHead1),
                ESemiSequence.DotWeighting_H1 + (int)(Head - ESPDHead.SPDHead1),
                ESemiSequence.HeadAssemble_H1 + (int)(Head - ESPDHead.SPDHead1),
                ESemiSequence.HeadDisassemble_H1 + (int)(Head - ESPDHead.SPDHead1),
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
                    _devices.Outputs.NozzleCleanH1H2,
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
                    _devices.Outputs.NozzleCleanH1H2,
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
                    _devices.Outputs.NozzleCleanH3H4,
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
                    _devices.Outputs.NozzleCleanH3H4,
                };
            }

            Balance.SendRequestImmediateWeightCommand();
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

            var injectZSafetyGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    injectZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Inject Pos (Z Safety) {Name}",
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
                    break;
                case ESPDHead.SPDHead2:
                    injectZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Inject Pos (Z Safety) {Name}",
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
                    break;
                case ESPDHead.SPDHead3:
                    injectZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Inject Pos (Z Safety) {Name}",
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
                    break;
                case ESPDHead.SPDHead4:
                    injectZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Inject Pos (Z Safety) {Name}",
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
                    break;
            }

            var injectGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    injectGroup = new MultiPointPosition
                    {
                        Name = $"Inject Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisInjectPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisInjectPos, _devices.Motions.Z1Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead2:
                    injectGroup = new MultiPointPosition
                    {
                        Name = $"Inject Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisInjectPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisInjectPos, _devices.Motions.Z2Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead3:
                    injectGroup = new MultiPointPosition
                    {
                        Name = $"Inject Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisInjectPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisInjectPos, _devices.Motions.Z3Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead4:
                    injectGroup = new MultiPointPosition
                    {
                        Name = $"Inject Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisInjectPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisInjectPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisInjectPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
            }


            var dummyZSafetyGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    dummyZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Dummy Pos (Z Safety) {Name}",
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
                    break;
                case ESPDHead.SPDHead2:
                    dummyZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Dummy Pos (Z Safety) {Name}",
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
                    break;
                case ESPDHead.SPDHead3:
                    dummyZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Dummy Pos (Z Safety) {Name}",
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
                    break;
                case ESPDHead.SPDHead4:
                    dummyZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Dummy Pos (Z Safety) {Name}",
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
                    break;
            }

            var dummyGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    dummyGroup = new MultiPointPosition
                    {
                        Name = $"Dummy Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisDummyPos, _devices.Motions.Z1Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead2:
                    dummyGroup = new MultiPointPosition
                    {
                        Name = $"Dummy Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisDummyPos, _devices.Motions.Z2Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead3:
                    dummyGroup = new MultiPointPosition
                    {
                        Name = $"Dummy Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisDummyPos, _devices.Motions.Z3Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead4:
                    dummyGroup = new MultiPointPosition
                    {
                        Name = $"Dummy Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisDummyPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
            }

            var weightingZSafetyGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    weightingZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Dot Weighting (Z Safety) Pos {Name}",
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
                    break;
                case ESPDHead.SPDHead2:
                    weightingZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Dot Weighting (Z Safety) Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH24DotWeightingPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead3:
                    weightingZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Dot Weighting (Z Safety) Pos {Name}",
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
                    break;
                case ESPDHead.SPDHead4:
                    weightingZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Dot Weighting (Z Safety) Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH24DotWeightingPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
            }

            var weightingGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    weightingGroup = new MultiPointPosition
                    {
                        Name = $"Dot Weighting Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH13DotWeightingPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos, _devices.Motions.Z1Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead2:
                    weightingGroup = new MultiPointPosition
                    {
                        Name = $"Dot Weighting Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH24DotWeightingPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos, _devices.Motions.Z2Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead3:
                    weightingGroup = new MultiPointPosition
                    {
                        Name = $"Dot Weighting Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH13DotWeightingPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos, _devices.Motions.Z3Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead4:
                    weightingGroup = new MultiPointPosition
                    {
                        Name = $"Dot Weighting Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisH24DotWeightingPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
            }

            var cleaningZSafetyGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    cleaningZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Clean (Z Safety) Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeddleClean, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead2:
                    cleaningZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Clean (Z Safety) Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeddleClean, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead3:
                    cleaningZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Clean (Z Safety) Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeddleClean, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead4:
                    cleaningZSafetyGroup = new MultiPointPosition
                    {
                        Name = $"Clean (Z Safety) Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeddleClean, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
            }

            var cleaningGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    cleaningGroup = new MultiPointPosition
                    {
                        Name = $"Clean Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeddleClean, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos, _devices.Motions.Z1Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead2:
                    cleaningGroup = new MultiPointPosition
                    {
                        Name = $"Clean Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeddleClean, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos, _devices.Motions.Z2Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead3:
                    cleaningGroup = new MultiPointPosition
                    {
                        Name = $"Clean Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeddleClean, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos, _devices.Motions.Z3Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead4:
                    cleaningGroup = new MultiPointPosition
                    {
                        Name = $"Clean Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisNeedleCleanPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisNeddleClean, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
            }

            var assembleDisassembleGroup = new MultiPointPosition();
            switch (Head)
            {
                case ESPDHead.SPDHead1:
                    assembleDisassembleGroup = new MultiPointPosition
                    {
                        Name = $"Assemble/DisAssemble Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePos, _devices.Motions.Z1Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead2:
                    assembleDisassembleGroup = new MultiPointPosition
                    {
                        Name = $"Assemble/DisAssemble Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePos, _devices.Motions.Z2Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead3:
                    assembleDisassembleGroup = new MultiPointPosition
                    {
                        Name = $"Assemble/DisAssemble Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePos, _devices.Motions.Z3Axis),
                        }
                    };
                    break;
                case ESPDHead.SPDHead4:
                    assembleDisassembleGroup = new MultiPointPosition
                    {
                        Name = $"Assemble/DisAssemble Pos {Name}",
                        Points = new ObservableCollection<PositionPoint>
                        {
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.XAxisDummyPos, _devices.Motions.XAxis),
                            positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.InjectRecipe.YAxisDummyPos, _devices.Motions.StageYAxis),
                            positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePos, _devices.Motions.Z4Axis),
                        }
                    };
                    break;
            }

            positionManager.GroupedPositions.Add(readyGroup);
            positionManager.GroupedPositions.Add(injectZSafetyGroup);
            positionManager.GroupedPositions.Add(injectGroup);
            positionManager.GroupedPositions.Add(dummyZSafetyGroup);
            positionManager.GroupedPositions.Add(dummyGroup);
            positionManager.GroupedPositions.Add(cleaningZSafetyGroup);
            positionManager.GroupedPositions.Add(cleaningGroup);
            positionManager.GroupedPositions.Add(weightingZSafetyGroup);
            positionManager.GroupedPositions.Add(weightingGroup);
            positionManager.GroupedPositions.Add(assembleDisassembleGroup);

            return positionManager;
        }

        #region Privates
        private readonly Devices _devices;
        private readonly Balances _balances;
        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
        private double balanceStableWeight;

        private ESPDHead Head => Name switch
        {
            "SPDHead1" => ESPDHead.SPDHead1,
            "SPDHead2" => ESPDHead.SPDHead2,
            "SPDHead3" => ESPDHead.SPDHead3,
            "SPDHead4" => ESPDHead.SPDHead4,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
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
        private ICylinder NozzleClean => Name switch
        {
            "SPDHead1" => _devices.Cylinders.NozzleClean_H1,
            "SPDHead2" => _devices.Cylinders.NozzleClean_H2,
            "SPDHead3" => _devices.Cylinders.NozzleClean_H3,
            "SPDHead4" => _devices.Cylinders.NozzleClean_H4,
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