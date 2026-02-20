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
        [Description("Ready")]
        Ready,

        [Description("Jig LOADING")]
        Loading,
        [Description("Resin INJECT")]
        ResinInject,
        [Description("Jig UNLOADING")]
        Unloading,
        [Description("Dummy shot")]
        DummyShot,
        [Description("Needle cleaning")]
        NeedleCleaning,
        [Description("Dot weighting")]
        DotWeighting,

        [Description("Head ASSEMBLE")]
        HeadAssemble,
        [Description("Head DISASSEMBLE")]
        HeadDisassemble,
        [Description("Bubble remove")]
        BubbleRemove
    }

    public enum ESemiSequence
    {
        None,

        /// <summary>
        /// Move Units to non-collision positions before starting Auto Run
        /// </summary>
        [Description("Ready")]
        Ready,
        [Description("Jig LOADING")]
        Loading,
        [Description("Resin INJECT")]
        ResinInject,
        [Description("Jig UNLOADING")]
        Unloading,
        [Description("Dummy shot")]
        DummyShot,
        [Description("Needle cleaning")]
        NeedleCleaning,
        [Description("Dot weighting")]
        DotWeighting,

        [Description("Head ASSEMBLE")]
        HeadAssemble,
        [Description("Head DISASSEMBLE")]
        HeadDisassemble,
        [Description("Bubble remove")]
        BubbleRemove
    }
}
