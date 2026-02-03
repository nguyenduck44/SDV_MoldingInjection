using EQX.Core.InOut;
using EQX.InOut;
using SDV_DotDispenser.Defines.Cylinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Defines.Devices.Cylinder
{
    public class Cylinders
    {
        public ICylinder PlasmaCoverCylBwFw { get; }
        public ICylinder TransferHand1UpDown { get; }
        public ICylinder TransferHand2UpDown { get; }
        public ICylinder UVCureBwFw { get; }
        public Cylinders(ICylinderFactory cylinderFactory, Inputs inputs, Outputs outputs)
        {
            _cylinderFactory = cylinderFactory;
            _inputs = inputs;
            _outputs = outputs;

            PlasmaCoverCylBwFw = _cylinderFactory
                .Create(_inputs.PlasmaCoverLeft, _inputs.PlasmaCoverRight, _outputs.PlasmaCylFw, _outputs.PlasmaCylBw)
                .SetIdentity((int)ECylinder.PlasmaCylBwFw, ECylinder.PlasmaCylBwFw.ToString());
            PlasmaCoverCylBwFw.CylinderType = ECylinderType.ForwardBackward;

            TransferHand1UpDown = _cylinderFactory
                .Create(_inputs.TransferHand1Down, _inputs.TransferHand1Up, _outputs.TransferHand1Down, _outputs.TransferHand1Up)
                .SetIdentity((int)ECylinder.TransferHand1UpDown, ECylinder.TransferHand1UpDown.ToString());
            TransferHand1UpDown.CylinderType = ECylinderType.UpDown;

            TransferHand2UpDown = _cylinderFactory
                .Create(_inputs.TransferHand2Down, _inputs.TransferHand2Up, _outputs.TransferHand2Down, _outputs.TransferHand2Up)
                .SetIdentity((int)ECylinder.TransferHand2UpDown , ECylinder.TransferHand2UpDown.ToString());
            TransferHand2UpDown.CylinderType = ECylinderType.UpDown;

            UVCureBwFw = _cylinderFactory
                .Create(_inputs.UvCureFw , _inputs.UvCureBw, _outputs.UvCureFw, _outputs.UvCureBw)
                .SetIdentity((int)ECylinder.UVCureBwFw, ECylinder.UVCureBwFw.ToString());
            UVCureBwFw.CylinderType = ECylinderType.ForwardBackward;
        }

        private readonly ICylinderFactory _cylinderFactory;
        private readonly Inputs _inputs;
        private readonly Outputs _outputs;
    }
}
