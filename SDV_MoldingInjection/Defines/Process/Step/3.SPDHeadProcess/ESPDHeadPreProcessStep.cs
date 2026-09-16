namespace SDV_MoldingInjection.Defines
{
    public enum ESPDHeadPreProcessStep
    {
        Start,
        WriteCarrierJigStatus_InjectTime,
        DummyOverFlowDetect_Check,
        Syringe_Check,
        AssembleDetect_Check,
        Assemble_Cyl_Up,
        End,
    }
}
