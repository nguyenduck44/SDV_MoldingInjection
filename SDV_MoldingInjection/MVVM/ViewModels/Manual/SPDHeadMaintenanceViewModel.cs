using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.Device.Balance;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.TeachingPosition;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class SPDHeadMaintenanceViewModel : AppMaintenanceViewModel
    {
        #region Properties
        public AutoTeachModeViewModel AutoTeachModeViewModel { get; }
        public SPDHeadMaintenanceTeachingPosition SPDHeadMaintenanceTeachingPosition =>
            _spdHeadMaintenanceTeachingPositions.First(p => p.Head == Head);
        #endregion

        #region Contructor(s)
        public SPDHeadMaintenanceViewModel(
            Devices devices,
            NavigationStore navigationStore,
            Balances balances,
            MachineStatus machineStatus,
            RecipeSelector recipeSelector,
            AutoTeachModeViewModel autoTeachModeViewModel,
            IEnumerable<SPDHeadMaintenanceTeachingPosition> sPDHeadMaintenanceTeachingPositions)
            : base(navigationStore, machineStatus, recipeSelector, devices)
        {
            _devices = devices;
            _balances = balances;
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;
            AutoTeachModeViewModel = autoTeachModeViewModel;
            _spdHeadMaintenanceTeachingPositions = sPDHeadMaintenanceTeachingPositions.ToList();
            Motions = new ObservableCollection<IMotion>();
            if (GroupedPositions != null && GroupedPositions.Count > 0)
            {
                SelectedGroupedPosition = GroupedPositions.FirstOrDefault()!;
            }

        }
        #endregion

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

            base.ExternalTimerElapsedAction();
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
            };
            if (Name == EProcess.SPDHead1.ToString())
            {
                Inputs = new ObservableCollection<IDInput>
                {
                    _devices.Inputs.H1_CylUp,
                    _devices.Inputs.H1_CylDown,
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
                    _devices.Inputs.H2_CylUp,
                    _devices.Inputs.H2_CylDown,
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
                    _devices.Inputs.H3_CylUp,
                    _devices.Inputs.H3_CylDown,
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
                    _devices.Inputs.H4_CylUp,
                    _devices.Inputs.H4_CylDown,
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
            return SPDHeadMaintenanceTeachingPosition.PositionManager;
        }

        #region Privates
        private readonly Devices _devices;
        private readonly Balances _balances;
        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
        private readonly List<SPDHeadMaintenanceTeachingPosition> _spdHeadMaintenanceTeachingPositions;
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