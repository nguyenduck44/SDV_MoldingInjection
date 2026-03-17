using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class TactTimeViewModel : ViewModelBase
    {
        public TactTimeViewModel(TactTimeList tactTimeList)
        {
            TactTimeList = tactTimeList;
        }

        public TactTimeList TactTimeList { get; }

        public ICommand ResetTactTimeCommand
        {
            get
            {
                return new RelayCommand<string>((name) =>
                {
                    if (name == "Load")
                    {
                        TactTimeList.Loading.Reset();
                    }
                    if (name == "Inject")
                    {
                        TactTimeList.Inject.Reset();
                    }
                    if (name == "DummyShot")
                    {
                        TactTimeList.DummyShot.Reset();
                    }
                    if (name == "NeedleClean")
                    {
                        TactTimeList.NeedleClean.Reset();
                    }
                    if (name == "Unload")
                    {
                        TactTimeList.Unloading.Reset();
                    }
                });
            }
        }
    }
}
