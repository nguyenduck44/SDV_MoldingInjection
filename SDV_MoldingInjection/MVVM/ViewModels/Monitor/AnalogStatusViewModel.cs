using EQX.Core.Common;
using SDV_MoldingInjection.Defines;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AnalogStatusViewModel : ViewModelBase
    {
        public AnalogStatusViewModel(CDAStatus cDAStatus)
        {
            CDAStatus = cDAStatus;

        }
        public CDAStatus CDAStatus { get; }
    }
}
