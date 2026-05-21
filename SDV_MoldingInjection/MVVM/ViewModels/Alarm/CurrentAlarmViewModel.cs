using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Communication.CIM.Custom;
using SDV_MoldingInjection.Defines;
using System.Windows.Input;
using TOPENG_Device;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class CurrentAlarmViewModel : ViewModelBase
    {
        private readonly InOutHandler _inOutHandler;

        public CurrentAlarmViewModel(MachineStatus machineStatus, 
            InOutHandler inOutHandler)
        {
            MachineStatus = machineStatus;
            _inOutHandler = inOutHandler;
        }

        public MachineStatus MachineStatus { get; }

        public ICommand AlarmResetCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    //CIMScenarioDispatcher.ExecuteScenario(CIMScenario.AlarmRelease, new CIMScenarioContext
                    //{
                    //    CIMAlarmData = new CIMAlarmData
                    //    {
                    //        ALCD = 2, // HEAVY ALARM
                    //        ALID = 1,
                    //        AlarmDescription = "TC DISCONNECTION ALARM",
                    //    }
                    //});

                    //foreach (var alarm in MachineStatus.CurrentAlarms)
                    //{
                    //    CIMScenarioDispatcher.ExecuteScenario(CIMScenario.AlarmRelease, new CIMScenarioContext
                    //    {
                    //        CIMAlarmData = new CIMAlarmData
                    //        {
                    //            ALCD = 2, // HEAVY ALARM
                    //            ALID = alarm.ErrorCode,
                    //            AlarmDescription = alarm.Type == "WARN" ? ((EWarning)alarm.ErrorCode).ToString() : ((EAlarm)alarm.ErrorCode).ToString(),
                    //        }
                    //    });
                    //}

                    MachineStatus.CurrentAlarms.Clear();
                    MachineStatus.Message = string.Empty;
                    _inOutHandler.Handler_SendError(0);
                    EquipEventHelpers.ClearAlarm();
                });
            }
        }
    }
}
