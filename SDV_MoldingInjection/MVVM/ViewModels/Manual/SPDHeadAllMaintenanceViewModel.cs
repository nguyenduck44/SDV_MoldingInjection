using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Recipe;
using EQX.Device.Balance;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Defines.Devices.Balance;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
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

        protected override void ActualInit()
        {
            Sequences = new ObservableCollection<ESemiSequence>
            {
                ESemiSequence.DummyShot,
                ESemiSequence.NeedleCleaning,
                ESemiSequence.BubbleRemove,
                ESemiSequence.DotWeighting,
            };
        }

        protected override RecipePositionManagerBase<RecipeList> UpdatePositionManager()
        {
            return new RecipePositionManagerBase<RecipeList>(_recipeSelector.CurrentRecipe, _devices.Motions.All);
        }
        #endregion
    }
}
