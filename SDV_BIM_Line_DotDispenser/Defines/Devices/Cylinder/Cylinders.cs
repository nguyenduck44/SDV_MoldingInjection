using EQX.Core.InOut;
using EQX.InOut;
using SDV_BIM_Line_DotDispenser.Defines.Cylinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_BIM_Line_DotDispenser.Defines.Devices.Cylinder
{
    public class Cylinders
    {
        // In CST
        public ICylinder InCV_StopperCyl { get; }

        // Out CST
        public ICylinder OutCV_StopperCyl { get; }

        public Cylinders(ICylinderFactory cylinderFactory, Inputs inputs, Outputs outputs)
        {
            _cylinderFactory = cylinderFactory;
            _inputs = inputs;
            _outputs = outputs;

            // In CST
            InCV_StopperCyl = _cylinderFactory
                .Create(_inputs.InCV_StopperUp, _inputs.InCV_StopperDown, _outputs.InCst_StopperUp, _outputs.InCst_StopperDown)
                .SetIdentity((int)ECylinder.InCV_StopperUpDown, ECylinder.InCV_StopperUpDown.ToString());
            InCV_StopperCyl.CylinderType = ECylinderType.UpDown;

            // Out CST
            OutCV_StopperCyl = _cylinderFactory
                .Create(_inputs.OutCV_StopperUp, _inputs.OutCV_StopperDown, _outputs.OutCst_StopperUp, _outputs.OutCst_StopperDown)
                .SetIdentity((int)ECylinder.OutCV_StopperUpDown, ECylinder.OutCV_StopperUpDown.ToString());
            OutCV_StopperCyl.CylinderType = ECylinderType.UpDown;
        }

        private readonly ICylinderFactory _cylinderFactory;
        private readonly Inputs _inputs;
        private readonly Outputs _outputs;
    }
}
