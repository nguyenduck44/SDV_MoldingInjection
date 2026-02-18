using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Process;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class ManualViewModel : ViewModelBase
    {
        private readonly Processes _processes;
        private readonly INavigationService _navigationService;

        public ManualViewModel(Processes processes,
            INavigationService navigationService,
            IViewModelFactory viewModelFactory)
        {
            _processes = processes;
            _navigationService = navigationService;
            ManualUnitVM = viewModelFactory.Create<AppManualUnitViewModel>();
        }

        public List<IProcess<ESequence>> ProcessList => _processes.RootProcess.Childs!.ToList();

        private AppManualUnitViewModel ManualUnitVM { get; }

        public ICommand ManualUnitSelectCommand
        {
            get
            {
                return new RelayCommand<string>((name) =>
                {
                    ManualUnitVM.DependenceProcessList = new List<IProcess<ESequence>> { ProcessList.First(p => p.Name == name)!, ProcessList[2], ProcessList[4] };
                    ManualUnitVM.CurrentProcess = ProcessList.First(p => p.Name == name)!;

                    _navigationService.NavigateTo<AppManualUnitViewModel>();
                });
            }
        }
    }
}
