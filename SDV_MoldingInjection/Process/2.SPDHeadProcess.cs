using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.Process
{
    public class SPDHeadProcess : MIProcess
    {
        #region Inputs
        
        #endregion

        #region Outputs
        
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
