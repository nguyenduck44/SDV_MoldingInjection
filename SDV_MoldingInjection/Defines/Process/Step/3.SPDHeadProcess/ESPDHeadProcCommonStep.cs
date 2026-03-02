namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcCommonStep
    {
        Start,

        ResetInjectTime,

        // ----- CHARGING PHASE -----
        Gate_Close,
        Gate_CloseWait,

        Charge_PosVel_Calculte,

        PAxis_ChargePos_Move,
        PAxis_ChargePos_MoveWait,

        WorkRequest_Wait,

        // ----- INJECT PHASE -----
        Base_PosVel_Calculte,

        GAxis_BubbleRemove,
        GAxis_BubbleRemove_Wait,

        Gate_Open,
        Gate_OpenWait,

        PAxis_InjectPos_Move,
        PAxis_InjectPos_MoveWait,

        WorkDone_Send,
        WorkDone_Clear,

        End,
    }
}
