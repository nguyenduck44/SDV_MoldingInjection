using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Recipe;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class SPDHeadAllMaintenanceViewModel : AppMaintenanceViewModel
    {
        private readonly RecipeSelector _recipeSelector;
        private readonly Devices _devices;

        public SPDHeadAllMaintenanceViewModel(NavigationStore navigationStore,
            MachineStatus machineStatus,
            RecipeSelector recipeSelector,
            Devices devices)
            : base(navigationStore, machineStatus, recipeSelector)
        {
            MachineStatus = machineStatus;
            _recipeSelector = recipeSelector;
            _devices = devices;
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

        public MachineStatus MachineStatus { get; }

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
    }
}
