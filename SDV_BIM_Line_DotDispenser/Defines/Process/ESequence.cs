using System.ComponentModel;

namespace SDV_BIM_Line_DotDispenser.Defines
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

        [Description("Sequence Load")]
        Load,
        [Description("Sequence Unload")]
        Unload,
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
