using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.Process
{
    public class SPDHeadProcess : MIProcess
    {
        #region Inputs
        private IDInput In_CylUp => head switch
        {
            ESPDHead.SPDHead1 => devices.Inputs.H1_CylUp,
            ESPDHead.SPDHead2 => devices.Inputs.H2_CylUp,
            ESPDHead.SPDHead3 => devices.Inputs.H3_CylUp,
            ESPDHead.SPDHead4 => devices.Inputs.H4_CylUp,
            _ => throw new Exception($"Invalid head: {head}")
        };
        private IDInput In_CylDown => head switch
        {
            ESPDHead.SPDHead1 => devices.Inputs.H1_CylDown,
            ESPDHead.SPDHead2 => devices.Inputs.H2_CylDown,
            ESPDHead.SPDHead3 => devices.Inputs.H3_CylDown,
            ESPDHead.SPDHead4 => devices.Inputs.H4_CylDown,
            _ => throw new Exception($"Invalid head: {head}")
        };
        #endregion

        #region Outputs
        private IDOutput Out_CylUp => head switch
        {
            ESPDHead.SPDHead1 => devices.Outputs.H1_CylUp,
            ESPDHead.SPDHead2 => devices.Outputs.H2_CylUp,
            ESPDHead.SPDHead3 => devices.Outputs.H3_CylUp,
            ESPDHead.SPDHead4 => devices.Outputs.H4_CylUp,
            _ => throw new Exception($"Invalid head: {head}")
        };
        private IDOutput Out_CylDown => head switch
        {
            ESPDHead.SPDHead1 => devices.Outputs.H1_CylDown,
            ESPDHead.SPDHead2 => devices.Outputs.H2_CylDown,
            ESPDHead.SPDHead3 => devices.Outputs.H3_CylDown,
            ESPDHead.SPDHead4 => devices.Outputs.H4_CylDown,
            _ => throw new Exception($"Invalid head: {head}")
        };
        #endregion

        #region Motions
        private IMotion PAxis => Name switch
        {
            "SPDHead1" => devices.Motions.P1Axis,
            "SPDHead2" => devices.Motions.P2Axis,
            "SPDHead3" => devices.Motions.P3Axis,
            "SPDHead4" => devices.Motions.P4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };

        private IMotion GAxis => Name switch
        {
            "SPDHead1" => devices.Motions.G1Axis,
            "SPDHead2" => devices.Motions.G2Axis,
            "SPDHead3" => devices.Motions.G3Axis,
            "SPDHead4" => devices.Motions.G4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        #endregion

        #region Cylinders
        private ICylinder UpDownCyl => head switch
        {
            ESPDHead.SPDHead1 => devices.Cylinders.H1_UpDown,
            ESPDHead.SPDHead2 => devices.Cylinders.H2_UpDown,
            ESPDHead.SPDHead3 => devices.Cylinders.H3_UpDown,
            ESPDHead.SPDHead4 => devices.Cylinders.H4_UpDown,
            _ => throw new Exception($"Invalid head: {head}")
        };
        #endregion

        public SPDHeadProcess(Devices devices)
        {
            this.devices = devices;
        }

        #region Privates
        private readonly Devices devices;

        private ESPDHead head => Name switch
        {
            "SPDHead1" => ESPDHead.SPDHead1,
            "SPDHead2" => ESPDHead.SPDHead2,
            "SPDHead3" => ESPDHead.SPDHead3,
            "SPDHead4" => ESPDHead.SPDHead4,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        #endregion
    }
}
