using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class ManualViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public ManualViewModel(IEnumerable<MaintenanceViewModel<ESequence>> maintenanceViewModels,
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

        public ICommand ManualUnitSelectCommand
        {
            get
            {
                return new RelayCommand<string>((name) =>
                {
                    _navigationService.NavigateTo(MaintenanceViewModels.First(vm => vm.Name == name));
                });
            }
        }

        public IEnumerable<MaintenanceViewModel<ESequence>> MaintenanceViewModels { get; }
    }
}
