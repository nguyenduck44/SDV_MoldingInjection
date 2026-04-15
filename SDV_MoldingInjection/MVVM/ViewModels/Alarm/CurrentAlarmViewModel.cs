using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class CurrentAlarmViewModel : ViewModelBase
    {
        public CurrentAlarmViewModel(MachineStatus machineStatus)
        {
            MachineStatus = machineStatus;
        }

        public MachineStatus MachineStatus { get; }

        public ICommand AlarmResetCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    MachineStatus.CurrentAlarms.Clear();
                    MachineStatus.Message = string.Empty;
                });
            }
        }
    }
}
