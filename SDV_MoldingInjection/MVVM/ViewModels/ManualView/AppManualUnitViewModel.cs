using EQX.Core.Process;
using EQX.UI.MVVM;
using SDV_MoldingInjection.Defines;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AppManualUnitViewModel : ManualUnitViewModel<ESequence>
    {
        public AppManualUnitViewModel(IEnumerable<IProcess<ESequence>> processes)
            : base(processes)
        {
        }
    }
}
