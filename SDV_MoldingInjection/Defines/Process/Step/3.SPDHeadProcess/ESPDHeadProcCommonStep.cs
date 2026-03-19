namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadProcCommonStep
    {
        Start,

        ResetInjectTime,
        BubbleRemoveResetCountRotate,
        // ----- CHARGING PHASE -----
        Gate_Close,
        Gate_CloseWait,

        Charge_PosVel_Calculte,

        PAxis_ChargePos_Move,
        PAxis_ChargePos_MoveWait,

        // ----- INJECT PHASE -----
        Base_PosVel_Calculte,

        GAxis_BubbleRemove,
        GAxis_BubbleRemove_Wait,

        Gate_Open,
        Gate_OpenWait,

        WorkRequest_Wait,

        ZAxis_Up_ForInjectAddTail_Requsest,
        Wait_ZAxisReady_ForInjectAddTail,

        PAxis_InjectPos_Move,
        PAxis_InjectPos_MoveWait,

        WorkDone_Send,
        WorkDone_Clear,

        InjectAddTail_Check,

        End,
    }
}
