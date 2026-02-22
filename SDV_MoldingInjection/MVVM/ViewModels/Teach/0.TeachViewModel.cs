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

        public TeachViewModel(IEnumerable<MaintenanceViewModel<ESemiSequence, RecipeList>> maintenanceViewModels,
            INavigationService navigationService)
        {
            MaintenanceViewModels = maintenanceViewModels;
            _navigationService = navigationService;

            for (int i = 0; i < MaintenanceViewModels.Count(); i++)
            {
                MaintenanceViewModels.ToList()[i].Name = Enum.GetName(typeof(EProcess), EProcess.Root + 1 + i);
            }

            foreach (var vm in MaintenanceViewModels)
            {
                vm.Init();
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
