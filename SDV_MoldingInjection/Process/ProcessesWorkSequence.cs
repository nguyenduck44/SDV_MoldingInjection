using SDV_MoldingInjection.Defines;

namespace SDV_MoldingInjection.Process
{
    public class ProcessesWorkSequence
    {
        public static readonly List<EDryPumpProcResinInjectStep> DryPumpResinInjectSequence = new List<EDryPumpProcResinInjectStep>
        {
            EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait,
            EDryPumpProcResinInjectStep.AngleValve_Open,
            EDryPumpProcResinInjectStep.AngleValve_OpenWait,
            EDryPumpProcResinInjectStep.VacuumGauge_Target_Wait,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.Delay_BeforeInject,
            EDryPumpProcResinInjectStep.DryPump_VacuumDone_Send,
            EDryPumpProcResinInjectStep.WaitEndHoldPressure1st_StartHoldPressure2nd,
            EDryPumpProcResinInjectStep.DryPump_WaitVentTime,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.SetStartVent,
            EDryPumpProcResinInjectStep.DryPump_PurgeAndWait,
            EDryPumpProcResinInjectStep.WaitToClear_ProcOutput,
        }; 
        
        public static readonly List<EDryPumpProcResinInjectStep> DryPumpResinInjectSequence_Use1Torr = new List<EDryPumpProcResinInjectStep>
        {
            EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait,
            EDryPumpProcResinInjectStep.AngleValve_Open,
            EDryPumpProcResinInjectStep.AngleValve_OpenWait,
            EDryPumpProcResinInjectStep.VacuumGauge_SpecIn_Wait_1torr,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.DryPump_Purge,
            EDryPumpProcResinInjectStep.DryPump_Purge_Wait,
            EDryPumpProcResinInjectStep.AngleValve_Open,
            EDryPumpProcResinInjectStep.AngleValve_OpenWait,
            EDryPumpProcResinInjectStep.VacuumGauge_Target_Wait,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.Delay_BeforeInject,
            EDryPumpProcResinInjectStep.DryPump_VacuumDone_Send,
            EDryPumpProcResinInjectStep.WaitEndHoldPressure1st_StartHoldPressure2nd,
            EDryPumpProcResinInjectStep.DryPump_WaitVentTime,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.SetStartVent,
            EDryPumpProcResinInjectStep.DryPump_PurgeAndWait,
            EDryPumpProcResinInjectStep.WaitToClear_ProcOutput,
        };

        public static readonly List<EDryPumpProcResinInjectStep> DryPumpResinInjectSequence_SkipVent = new List<EDryPumpProcResinInjectStep>
        {
            EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait,
            EDryPumpProcResinInjectStep.AngleValve_Open,
            EDryPumpProcResinInjectStep.AngleValve_OpenWait,
            EDryPumpProcResinInjectStep.VacuumGauge_Target_Wait,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.Delay_BeforeInject,
            EDryPumpProcResinInjectStep.DryPump_VacuumDone_Send,
            EDryPumpProcResinInjectStep.WaitEndHoldPressure1st_StartHoldPressure2nd,
            EDryPumpProcResinInjectStep.Inject_PurgeRequest_Wait,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.Wait_PurgeEnd,
            EDryPumpProcResinInjectStep.SetFlag_PurgeFinish,
            EDryPumpProcResinInjectStep.WaitToClear_ProcOutput,
        };

        public static readonly List<EDryPumpProcResinInjectStep> DryPumpResinInjectSequence_Use1Torr_SkipVent = new List<EDryPumpProcResinInjectStep>
        {
            EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait,
            EDryPumpProcResinInjectStep.AngleValve_Open,
            EDryPumpProcResinInjectStep.AngleValve_OpenWait,
            EDryPumpProcResinInjectStep.VacuumGauge_SpecIn_Wait_1torr,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.DryPump_Purge,
            EDryPumpProcResinInjectStep.DryPump_Purge_Wait,
            EDryPumpProcResinInjectStep.AngleValve_Open,
            EDryPumpProcResinInjectStep.AngleValve_OpenWait,
            EDryPumpProcResinInjectStep.VacuumGauge_Target_Wait,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.Delay_BeforeInject,
            EDryPumpProcResinInjectStep.DryPump_VacuumDone_Send,
            EDryPumpProcResinInjectStep.WaitEndHoldPressure1st_StartHoldPressure2nd,
            EDryPumpProcResinInjectStep.Inject_PurgeRequest_Wait,
            EDryPumpProcResinInjectStep.AngleValve_Close,
            EDryPumpProcResinInjectStep.AngleValve_CloseWait,
            EDryPumpProcResinInjectStep.Wait_PurgeEnd,
            EDryPumpProcResinInjectStep.SetFlag_PurgeFinish,
            EDryPumpProcResinInjectStep.WaitToClear_ProcOutput,
        };
    }
}
