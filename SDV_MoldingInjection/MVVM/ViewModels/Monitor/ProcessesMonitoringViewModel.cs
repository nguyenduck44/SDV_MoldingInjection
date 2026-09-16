using EQX.Core.Common;
using SDV_MoldingInjection.Process;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class ProcessesMonitoringViewModel : ViewModelBase
    {
        public ProcessesMonitoringViewModel(Processes processes)
        {
            Processes = processes;
        }

        public Processes Processes { get; }
    }
}
