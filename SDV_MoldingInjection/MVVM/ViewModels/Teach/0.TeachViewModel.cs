using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Process;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class TeachViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly NavigationStore _navigationStore;

        public TeachViewModel(IEnumerable<MaintenanceViewModel<ESemiSequence, 
            RecipeList>> maintenanceViewModels,
            INavigationService navigationService,
            NavigationStore navigationStore)
        {
            MaintenanceViewModels = maintenanceViewModels;
            _navigationService = navigationService;
            _navigationStore = navigationStore;
            for (int i = 0; i < MaintenanceViewModels.Count(); i++)
            {
                MaintenanceViewModels.ToList()[i].Name = Enum.GetName(typeof(EProcess), EProcess.Root + 1 + i);
            }

            foreach (var vm in MaintenanceViewModels)
            {
                vm.Init();

                vm.RelatedViewModelNavigateEvent += (name) =>
                {
                    if (_navigationStore.CurrentViewModel is AppMaintenanceViewModel maintenanceViewModel == false) return;
                    if (maintenanceViewModel.MaintenanceView != EMaintenanceView.Teach) return;
                    if (MaintenanceViewModels.FirstOrDefault(vm => vm.Name == name) == null) return;

                    MaintenanceViewModels.First(vm => vm.Name == name).MaintenanceView = maintenanceViewModel.MaintenanceView;
                    _navigationService.NavigateTo(MaintenanceViewModels.First(vm => vm.Name == name));
                };
            }
        }

        public ICommand TeachUnitSelectCommand
        {
            get
            {
                return new RelayCommand<string>((name) =>
                {
                    MaintenanceViewModels.First(vm => vm.Name == name).MaintenanceView = EMaintenanceView.Teach;
                    _navigationService.NavigateTo(MaintenanceViewModels.First(vm => vm.Name == name));
                });
            }
        }

        public IEnumerable<MaintenanceViewModel<ESemiSequence, RecipeList>> MaintenanceViewModels { get; }
    }
}
