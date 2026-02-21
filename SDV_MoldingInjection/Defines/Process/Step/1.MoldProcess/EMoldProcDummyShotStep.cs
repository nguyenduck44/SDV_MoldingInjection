namespace SDV_MoldingInjection.Defines
{
    public enum EMoldProcDummyShotStep
    {
        Start,

        XYAxis_H13DummyPos_Move,
        XYAxis_H13DummyPos_Wait,

        Head13_DummyShot_Request,
        Head13_DummyShot_DoneWait,

        XYAxis_H24DummyPos_Move,
        XYAxis_H24DummyPos_Wait,

        Head24_DummyShot_Request,
        Head24_DummyShot_DoneWait,

        End,
    }
}
