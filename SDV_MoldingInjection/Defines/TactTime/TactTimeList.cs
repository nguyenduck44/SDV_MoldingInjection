namespace SDV_MoldingInjection.Defines
{
    public class TactTimeList
    {
        public TactTimeList()
        {
            Loading = new TactTime("Loading");
            Inject = new TactTime("Inject");
            DummyShot = new TactTime("DummyShot");
            NeedleClean = new TactTime("NeedleClean");
            Unloading = new TactTime("Unloading");
        }

        public TactTime Loading { get; }
        public TactTime Inject { get; }
        public TactTime DummyShot { get; }
        public TactTime NeedleClean { get; }
        public TactTime Unloading { get; }
    }
}
