namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcDummyShotStep
    {
        Start,

        XYAxis_DummyPos_Move,
        XYAxis_DummyPos_Wait,

        ZAxis_DummyPos_Move,
        ZAxis_DummyPos_Wait,

        SPDHead_DummyShot_Request,
        SPDHead_DummyShot_DoneWait,

        End,
    }
}
