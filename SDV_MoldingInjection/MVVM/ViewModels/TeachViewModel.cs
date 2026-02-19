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
    public class TeachViewModel : ViewModelBase
    {
        private readonly Processes _processes;
        private readonly IViewModelFactory _viewModelFactory;

        public List<IProcess<ESequence>> ProcessList
        {
            get
            {
                List<IProcess<ESequence>> processTeach = new List<IProcess<ESequence>>();

                processTeach.Add(_processes.All.FirstOrDefault(p => p.Name == EProcess.Inject.ToString())!);
                processTeach.Add(_processes.All.FirstOrDefault(p => p.Name == EProcess.DryPump.ToString())!);
                processTeach.Add(_processes.All.FirstOrDefault(p => p.Name == EProcess.SPDHead1.ToString())!);
                processTeach.Add(_processes.All.FirstOrDefault(p => p.Name == EProcess.SPDHead2.ToString())!);
                processTeach.Add(_processes.All.FirstOrDefault(p => p.Name == EProcess.SPDHead3.ToString())!);
                processTeach.Add(_processes.All.FirstOrDefault(p => p.Name == EProcess.SPDHead4.ToString())!);

                return processTeach;
            }
        }

        public TeachViewModel(Processes processes, IViewModelFactory viewModelFactory)
        {
            _processes = processes;
            _viewModelFactory = viewModelFactory;

            TeachPositionUnitVM = _viewModelFactory.Create<AppTeachPositionUnitViewModel>();
            TeachPositionUnitVM.CurrentProcess = ProcessList.First();
        }

        public ICommand TeachUnitSelectCommand
        {
            get
            {
                return new RelayCommand<string>((name) =>
                {
                    TeachPositionUnitVM.CurrentProcess = ProcessList.FirstOrDefault(p => p.Name == name)!;
                });
            }
        }

        public AppTeachPositionUnitViewModel TeachPositionUnitVM { get; }
    }
}
