using System.ComponentModel;

namespace SDV_DotDispenser.Defines
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

        [Description("Sequence Left Stage Load")]
        LeftLoad,
        [Description("Sequence Right Stage Load")]
        RightLoad,
        [Description("Sequence Left Stage Plasma")]
        LeftPlasma,
        [Description("Sequence Right Stage Plasma")]
        RightPlasma,
        [Description("Sequence Nozzle Clean")]
        Clean,
        [Description("Sequence Left Stage Vision Align")]
        LeftAlignVision,
        [Description("Sequence Right Stage Vision Align")]
        RightAlignVision,
        [Description("Sequence Left Stage Dispensing")]
        LeftDispensing,
        [Description("Sequence Right Stage Dispensing")]
        RightDispensing,
        [Description("Sequence Left Stage Final Inspection")]
        LeftFinalInspection,
        [Description("Sequence Right Stage Final Inspection")]
        RightFinalInspection,
        [Description("Sequence Left Stage UV Cure")]
        LeftUVCure,
        [Description("Sequence Right Stage UV Cure")]
        RightUVCure,
        [Description("Sequence Left Stage Unload")]
        LeftUnload,
        [Description("Sequence Right Stage Unload")]
        RightUnload,
    }

    public enum ESemiSequence
    {
        None,

        [Description("Sequence Load")]
        Load,
        [Description("Sequence Unload")]
        Unload,
    }
}
