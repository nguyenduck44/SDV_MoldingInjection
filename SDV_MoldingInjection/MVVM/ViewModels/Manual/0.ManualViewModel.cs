using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class ManualViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly SPDHeadAllMaintenanceViewModel _sPDHeadAllMaintenanceViewModel;

        public ManualViewModel(IEnumerable<MaintenanceViewModel<ESemiSequence, RecipeList>> maintenanceViewModels,
            INavigationService navigationService,
            SPDHeadAllMaintenanceViewModel sPDHeadAllMaintenanceViewModel)
        {
            MaintenanceViewModels = maintenanceViewModels;
            _navigationService = navigationService;
            _sPDHeadAllMaintenanceViewModel = sPDHeadAllMaintenanceViewModel;

            for (int i = 0; i < MaintenanceViewModels.Count(); i++)
            {
                MaintenanceViewModels.ToList()[i].Name = Enum.GetName(typeof(EProcess), EProcess.Root + 1 + i);
            }

            ManualViewModels = new List<MaintenanceViewModel<ESemiSequence, RecipeList>>();
            sPDHeadAllMaintenanceViewModel.Name = "SPDs";
            ManualViewModels.AddRange(maintenanceViewModels.ToList());
            ManualViewModels.Add(sPDHeadAllMaintenanceViewModel);

            foreach (var vm in ManualViewModels)
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
                    ManualViewModels.First(vm => vm.Name == name).MaintenanceView = EMaintenanceView.Manual;
                    _navigationService.NavigateTo(ManualViewModels.First(vm => vm.Name == name));
                });
            }
        }

        public List<MaintenanceViewModel<ESemiSequence, RecipeList>> ManualViewModels { get; }

        public IEnumerable<MaintenanceViewModel<ESemiSequence, RecipeList>> MaintenanceViewModels { get; }
    }
}
