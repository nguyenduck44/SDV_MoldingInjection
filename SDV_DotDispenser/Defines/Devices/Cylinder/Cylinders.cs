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
        public ICylinder PlasmaCoverCyl { get; }
        public ICylinder TransferHand1Cyl { get; }
        public ICylinder TransferHand2Cyl { get; }
        public ICylinder UVCureCyl { get; }
        public ICylinder NozzleCleanCyl { get; }
        public Cylinders(ICylinderFactory cylinderFactory, Inputs inputs, Outputs outputs)
        {
            _cylinderFactory = cylinderFactory;
            _inputs = inputs;
            _outputs = outputs;

            PlasmaCoverCyl = _cylinderFactory
                .Create(_inputs.PlasmaCoverLeft, _inputs.PlasmaCoverRight, _outputs.PlasmaCylFw, _outputs.PlasmaCylBw)
                .SetIdentity((int)ECylinder.PlasmaCover, ECylinder.PlasmaCover.ToString());
            PlasmaCoverCyl.CylinderType = ECylinderType.ForwardBackward;

            TransferHand1Cyl = _cylinderFactory
                .Create(_inputs.TransferHand1Down, _inputs.TransferHand1Up, _outputs.TransferHand1Down, _outputs.TransferHand1Up)
                .SetIdentity((int)ECylinder.TransferHand1, ECylinder.TransferHand1.ToString());
            TransferHand1Cyl.CylinderType = ECylinderType.UpDown;

            TransferHand2Cyl = _cylinderFactory
                .Create(_inputs.TransferHand2Down, _inputs.TransferHand2Up, _outputs.TransferHand2Down, _outputs.TransferHand2Up)
                .SetIdentity((int)ECylinder.TransferHand2, ECylinder.TransferHand2.ToString());
            TransferHand2Cyl.CylinderType = ECylinderType.UpDown;

            UVCureCyl = _cylinderFactory
                .Create(_inputs.UvCureFw, _inputs.UvCureBw, _outputs.UvCureFw, _outputs.UvCureBw)
                .SetIdentity((int)ECylinder.UVCure, ECylinder.UVCure.ToString());
            UVCureCyl.CylinderType = ECylinderType.ForwardBackward;

            NozzleCleanCyl = _cylinderFactory
                .Create(_inputs.NozzleCleanerUp, _inputs.NozzleCleanerDown, _outputs.NozzleCleanerUp, null)
                .SetIdentity((int)ECylinder.NozzleClean, ECylinder.NozzleClean.ToString());
            NozzleCleanCyl.CylinderType = ECylinderType.UpDownReverse;
        }

        private readonly ICylinderFactory _cylinderFactory;
        private readonly Inputs _inputs;
        private readonly Outputs _outputs;
    }
}
