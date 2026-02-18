namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcResinInjectStep
    {
        Start,

        // ----- CHARGING PHASE -----
        Gate_Close,
        Gate_CloseWait,
        PAxis_ChargePos_Move,
        PAxis_ChargePos_MoveWait,

        WorkRequest_Wait,

        // ----- INJECT PHASE -----
        Gate_Open,
        Gate_OpenWait,
        PAxis_InjectPos_Move,
        PAxis_InjectPos_MoveWait,

        WorkDone_Send,
        WorkDone_Clear,

        End,
    }
}
