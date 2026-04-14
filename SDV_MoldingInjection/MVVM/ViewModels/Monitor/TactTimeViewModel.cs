using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class TactTimeViewModel : ViewModelBase
    {
        public TactTimeViewModel(TactTimeList tactTimeList,
            MachineStatus machineStatus)
        {
            TactTimeList = tactTimeList;
            MachineStatus = machineStatus;
        }

        public TactTimeList TactTimeList { get; }
        public MachineStatus MachineStatus { get; }

        public ICommand ResetCycleTimeCommand
        {
            get
            {
                return new RelayCommand<string>((name) =>
                {
                    if (MessageBoxEx.ShowDialog("Do you want to Reset TactTime ?", "Confirm") == false) return;

                    if (name == "Chamber")
                    {
                        TactTimeList.Chamber.ResetCycleTime();
                    }
                    if (name == "DryPump")
                    {
                        TactTimeList.DryPump.ResetCycleTime();
                    }
                    if (name == "SPDHead1")
                    {
                        TactTimeList.SPDHead1.ResetCycleTime();
                    }
                    if (name == "SPDHead2")
                    {
                        TactTimeList.SPDHead2.ResetCycleTime();
                    }
                    if (name == "SPDHead3")
                    {
                        TactTimeList.SPDHead3.ResetCycleTime();
                    }
                    if (name == "SPDHead4")
                    {
                        TactTimeList.SPDHead4.ResetCycleTime();
                    }
                });
            }
        }

        public ICommand ResetTactTimeCommand
        {
            get
            {
                return new RelayCommand<string>((name) =>
                {
                    if (MessageBoxEx.ShowDialog("Do you want to Reset TactTime ?", "Confirm") == false) return;

                    if (name == "Chamber")
                    {
                        TactTimeList.Chamber.ResetTactTime();
                    }
                    if (name == "DryPump")
                    {
                        TactTimeList.DryPump.ResetTactTime();
                    }
                    if (name == "SPDHead1")
                    {
                        TactTimeList.SPDHead1.ResetTactTime();
                    }
                    if (name == "SPDHead2")
                    {
                        TactTimeList.SPDHead2.ResetTactTime();
                    }
                    if (name == "SPDHead3")
                    {
                        TactTimeList.SPDHead3.ResetTactTime();
                    }
                    if (name == "SPDHead4")
                    {
                        TactTimeList.SPDHead4.ResetTactTime();
                    }
                });
            }
        }

        public ICommand ResetTactTimeListCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog("Do you want to Reset TactTime ?", "Confirm") == false) return;

                    TactTimeList.TactTimeListViewModel.Reset();
                });
            }
        }
    }
}
