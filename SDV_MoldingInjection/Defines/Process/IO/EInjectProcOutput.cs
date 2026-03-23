namespace SDV_MoldingInjection.Defines
{
    public enum EInjectProcOutput
    {
        XYAxisInDummyPos,

        DryPump_VacuumRequest,
        DryPump_PurgeRequest,
        SPDHeadWorkRequest,

        SPDHead1_RemoveResinRequest,
        SPDHead2_RemoveResinRequest,
        SPDHead3_RemoveResinRequest,
        SPDHead4_RemoveResinRequest,

        SPDHead_InjectAddTail_Request,

        Request_H13DotWeighting,
        Request_H24DotWeighting,
    }
}
