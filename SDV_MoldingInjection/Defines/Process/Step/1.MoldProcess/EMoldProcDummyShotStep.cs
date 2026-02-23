namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcDummyShotStep
    {
        Start,

        XYAxis_DummyPos_Move,
        XYAxis_DummyPos_Wait,

        ZAxis_DummyPos_Move,
        ZAxis_DummyPos_Wait,

        SPDHead_InjectResin_Request,
        SPDHead_InjectResin_DoneWait,

        End,
    }
}
