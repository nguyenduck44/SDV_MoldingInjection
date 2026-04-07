using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.Device.Balance;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class SPDHeadAllMaintenanceViewModel : AppMaintenanceViewModel
    {
        #region Privates
        private readonly Balances _balances;
        private readonly RecipeSelector _recipeSelector;
        private readonly Devices _devices;
        private double balanceLeftStableWeight;
        private double balanceRightStableWeight;

        private MettlerToledoWKC204C BalanceLeft
        {
            get => _balances.BalanceLeft;
        }

        private MettlerToledoWKC204C BalanceRight
        {
            get => _balances.BalanceRight;
        }
        #endregion

        #region Constructor
        public SPDHeadAllMaintenanceViewModel(NavigationStore navigationStore,
            MachineStatus machineStatus,
            Balances balances,
            RecipeSelector recipeSelector,
            Devices devices)
            : base(navigationStore, machineStatus, recipeSelector)
        {
            MachineStatus = machineStatus;
            _balances = balances;
            _recipeSelector = recipeSelector;
            _devices = devices;
        }
        #endregion

        #region Commands
        public ICommand SelectAllCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MachineStatus.IsSkipHead1 = false;
                    MachineStatus.IsSkipHead2= false;
                    MachineStatus.IsSkipHead3 = false;
                    MachineStatus.IsSkipHead4 = false;
                });
            }
        }

        public ICommand SelectHeadCommand
        {
            get
            {
                return new RelayCommand<object>((para) =>
                {
                    if (para == null) return;

                    if (para.ToString() == "1")
                    {
                        MachineStatus.IsSkipHead1 = !MachineStatus.IsSkipHead1;
                    }
                    if (para.ToString() == "2")
                    {
                        MachineStatus.IsSkipHead2 = !MachineStatus.IsSkipHead2;
                    }
                    if (para.ToString() == "3")
                    {
                        MachineStatus.IsSkipHead3 = !MachineStatus.IsSkipHead3;
                    }
                    if (para.ToString() == "4")
                    {
                        MachineStatus.IsSkipHead4 = !MachineStatus.IsSkipHead4;
                    }
                });
            }
        }

        public ICommand BalanceLeftSetZeroCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    BalanceLeft.SendZeroCommand();
                });
            }
        }
        public ICommand BalanceRightSetZeroCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    BalanceRight.SendZeroCommand();
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
                        if (MachineStatus.MachineCalibrationSkip[0] == false)
                            MachineStatus.MachineCalibrationSkip[0] = !MachineStatus.IsSkipHead1;
                        if (MachineStatus.MachineCalibrationSkip[1] == false)
                            MachineStatus.MachineCalibrationSkip[1] = !MachineStatus.IsSkipHead2;
                        if (MachineStatus.MachineCalibrationSkip[2] == false)
                            MachineStatus.MachineCalibrationSkip[2] = !MachineStatus.IsSkipHead3;
                        if (MachineStatus.MachineCalibrationSkip[3] == false)
                            MachineStatus.MachineCalibrationSkip[3] = !MachineStatus.IsSkipHead4;
                    }
                });
            }
        }
        #endregion

        #region Properties
        public MachineStatus MachineStatus { get; }
        public double BalanceLeftStableWeight
        {
            get { return balanceLeftStableWeight; }
            set
            {
                balanceLeftStableWeight = value;
                OnPropertyChanged();
            }
        }

        public double BalanceRightStableWeight
        {
            get { return balanceRightStableWeight; }
            set
            {
                balanceRightStableWeight = value;
                OnPropertyChanged();
            }
        }

        protected override void ExternalTimerElapsedAction()
        {
            if (BalanceLeft.WeightData != null)
            {
                BalanceLeftStableWeight = BalanceLeft.WeightData.Weight * (BalanceLeft.WeightData.Unit == "g" ? 1000 : 1);
                if ((MachineStatus as MachineStatus)!.IsStandByProcessMode)
                {
                    BalanceLeft.SendRequestImmediateWeightCommand();
                }
            }

            if (BalanceRight.WeightData != null)
            {
                BalanceRightStableWeight = BalanceRight.WeightData.Weight * (BalanceRight.WeightData.Unit == "g" ? 1000 : 1);
                if ((MachineStatus as MachineStatus)!.IsStandByProcessMode)
                {
                    BalanceRight.SendRequestImmediateWeightCommand();
                }
            }
        }

        protected override void ActualInit()
        {
            Sequences = new ObservableCollection<ESemiSequence>
            {
                ESemiSequence.DummyShot,
                ESemiSequence.NeedleCleaning,
                ESemiSequence.BubbleRemove,
                ESemiSequence.DotWeighting,
                ESemiSequence.HeadAssemble,
                ESemiSequence.HeadDisassemble,
                ESemiSequence.DrainShot,
            };
        }

        protected override RecipePositionManagerBase<RecipeList> UpdatePositionManager()
        {
            return new RecipePositionManagerBase<RecipeList>(_recipeSelector.CurrentRecipe, _devices.Motions.All);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && MachineStatus.CurrentProcessMode != EProcessMode.ToRun && MachineStatus.CurrentProcessMode != EProcessMode.Run)
            {
                MachineStatus.IsSkipHead1 = true;
                MachineStatus.IsSkipHead2 = true;
                MachineStatus.IsSkipHead3 = true;
                MachineStatus.IsSkipHead4 = true;
            }
                
            base.Dispose(disposing);
        }
        #endregion
    }
}
