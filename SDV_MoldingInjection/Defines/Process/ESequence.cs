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

        [Description("Dummy shot H1")]
        DummyShot_H1,
        [Description("Dummy shot H2")]
        DummyShot_H2,
        [Description("Dummy shot H3")]
        DummyShot_H3,
        [Description("Dummy shot H4")]
        DummyShot_H4,

        [Description("Needle cleaning H1")]
        NeedleCleaning_H1,
        [Description("Needle cleaning H2")]
        NeedleCleaning_H2,
        [Description("Needle cleaning H3")]
        NeedleCleaning_H3,
        [Description("Needle cleaning H4")]
        NeedleCleaning_H4,

        [Description("Dot weighting H1")]
        DotWeighting_H1,
        [Description("Dot weighting H2")]
        DotWeighting_H2,
        [Description("Dot weighting H3")]
        DotWeighting_H3,
        [Description("Dot weighting H4")]
        DotWeighting_H4,

        [Description("Head ASSEMBLE H1")]
        HeadAssemble_H1,
        [Description("Head ASSEMBLE H2")]
        HeadAssemble_H2,
        [Description("Head ASSEMBLE H3")]
        HeadAssemble_H3,
        [Description("Head ASSEMBLE H4")]
        HeadAssemble_H4,

        [Description("Head DISASSEMBLE H1")]
        HeadDisassemble_H1,
        [Description("Head DISASSEMBLE H2")]
        HeadDisassemble_H2,
        [Description("Head DISASSEMBLE H3")]
        HeadDisassemble_H3,
        [Description("Head DISASSEMBLE H4")]
        HeadDisassemble_H4,

        [Description("Bubble remove H1")]
        BubbleRemove_H1,
        [Description("Bubble remove H2")]
        BubbleRemove_H2,
        [Description("Bubble remove H3")]
        BubbleRemove_H3,
        [Description("Bubble remove H4")]
        BubbleRemove_H4,

        [Description("Dummy shot")]
        DummyShot,
        [Description("Needle cleaning")]
        NeedleCleaning,
        [Description("Bubble Remove")]
        BubbleRemove,
        [Description("Dot Weighting")]
        DotWeighting,
        [Description("Drain Shot")]
        DrainShot,

        MoveMultiPoint,
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

        [Description("Dummy shot H1")]
        DummyShot_H1,
        [Description("Dummy shot H2")]
        DummyShot_H2,
        [Description("Dummy shot H3")]
        DummyShot_H3,
        [Description("Dummy shot H4")]
        DummyShot_H4,
        
        [Description("Needle cleaning H1")]
        NeedleCleaning_H1,
        [Description("Needle cleaning H2")]
        NeedleCleaning_H2,
        [Description("Needle cleaning H3")]
        NeedleCleaning_H3,
        [Description("Needle cleaning H4")]
        NeedleCleaning_H4,

        [Description("Dot weighting H1")]
        DotWeighting_H1,
        [Description("Dot weighting H2")]
        DotWeighting_H2,
        [Description("Dot weighting H3")]
        DotWeighting_H3,
        [Description("Dot weighting H4")]
        DotWeighting_H4,

        [Description("Head ASSEMBLE H1")]
        HeadAssemble_H1,
        [Description("Head ASSEMBLE H2")]
        HeadAssemble_H2,
        [Description("Head ASSEMBLE H3")]
        HeadAssemble_H3,
        [Description("Head ASSEMBLE H4")]
        HeadAssemble_H4,

        [Description("Head DISASSEMBLE H1")]
        HeadDisassemble_H1,
        [Description("Head DISASSEMBLE H2")]
        HeadDisassemble_H2,
        [Description("Head DISASSEMBLE H3")]
        HeadDisassemble_H3,
        [Description("Head DISASSEMBLE H4")]
        HeadDisassemble_H4,

        [Description("Bubble remove H1")]
        BubbleRemove_H1,
        [Description("Bubble remove H2")]
        BubbleRemove_H2,
        [Description("Bubble remove H3")]
        BubbleRemove_H3,
        [Description("Bubble remove H4")]
        BubbleRemove_H4,

        [Description("Dummy shot")]
        DummyShot,
        [Description("Needle cleaning")]
        NeedleCleaning,
        [Description("Bubble Remove")]
        BubbleRemove,
        [Description("Dot Weighting")]
        DotWeighting,
        [Description("Drain Shot")]
        DrainShot,

        MoveMultiPoint,
    }
}
