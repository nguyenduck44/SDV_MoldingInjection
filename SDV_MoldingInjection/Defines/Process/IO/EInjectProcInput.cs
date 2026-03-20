namespace SDV_MoldingInjection.Defines
{
    public enum EInjectProcInput
    {
        SPDHead1_OriginDone,
        SPDHead2_OriginDone,
        SPDHead3_OriginDone,
        SPDHead4_OriginDone,

        DryPump_VacuumDone,

        SPDHead1_WorkDone,
        SPDHead2_WorkDone,
        SPDHead3_WorkDone,
        SPDHead4_WorkDone,

        Wait_SPDHead1_InjectAddTail,
        Wait_SPDHead2_InjectAddTail,
        Wait_SPDHead3_InjectAddTail,
        Wait_SPDHead4_InjectAddTail,

        SPDHead1_InjectAddTailDone,
        SPDHead2_InjectAddTailDone,
        SPDHead3_InjectAddTailDone,
        SPDHead4_InjectAddTailDone,

        SPDHead1_RequestDotWaiting,
        SPDHead2_RequestDotWaiting,
        SPDHead3_RequestDotWaiting,
        SPDHead4_RequestDotWaiting,

        VentComplete,
    }
}
