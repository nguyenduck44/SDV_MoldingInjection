using System.ComponentModel;

namespace SDV_MoldingInjection.Defines
{
    /// <summary>
    /// Sequences of the machine
    /// </summary>
    public enum ESequence
    {
        Stop,

        AutoRun,
        /// <summary>
        /// Move Units to non-collision positions before starting Auto Run
        /// </summary>
        Ready,

        [Description("Jig LOADING sequence")]
        Loading,
        [Description("Resin INJECT sequence")]
        ResinInject,
        [Description("Jig UNLOADING sequence")]
        Unloading,
        [Description("Dummy shot sequence")]
        DummyShot,
        [Description("Needle cleaning sequence")]
        NeedleCleaning,
        [Description("Dot weighting sequence")]
        DotWeighting,

        [Description("Head ASSEMBLE sequence")]
        HeadAssemble,
        [Description("Head DISASSEMBLE sequence")]
        HeadDisassemble,
        [Description("Bubble remove sequence")]
        BubbleRemove
    }

    public enum ESemiSequence
    {
        None,

        [Description("Jig LOADING sequence")]
        Loading,
        [Description("Resin INJECT sequence")]
        ResinInject,
        [Description("Jig UNLOADING sequence")]
        Unloading,
        [Description("Dummy shot sequence")]
        DummyShot,
        [Description("Needle cleaning sequence")]
        NeedleCleaning,
        [Description("Dot weighting sequence")]
        DotWeighting,

        [Description("Head ASSEMBLE sequence")]
        HeadAssemble,
        [Description("Head DISASSEMBLE sequence")]
        HeadDisassemble,
        [Description("Bubble remove sequence")]
        BubbleRemove
    }
}
