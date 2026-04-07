namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcDummyShotStep
    {
        Start,

        XYAxis_DummyPos_Move,
        XYAxis_DummyPos_Wait,

        ZAxis_DummyPos_Move,
        ZAxis_DummyPos_Wait,

        SPDHead_Working_Request,
        SPDHead_Work_Done_And_VentComplete_Wait,

        ZAxis_SafetyPos_Move,
        ZAxis_SafetyPos_Wait,

        End,
    }
}
