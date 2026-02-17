using EQX.Core.InOut;
using EQX.InOut;
using SDV_MoldingInjection.Defines.Cylinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines.Devices.Cylinder
{
    public class Cylinders
    {
        public ICylinder NozzleClean_H1 { get; }
        public ICylinder NozzleClean_H2 { get; }
        public ICylinder NozzleClean_H3 { get; }
        public ICylinder NozzleClean_H4 { get; }

        public ICylinder BellowUpDown { get; }

        public ICylinder ChamberOpenClose { get; }

        public ICylinder AngleValve { get; }

        public ICylinder H1_UpDown { get; }
        public ICylinder H2_UpDown { get; }
        public ICylinder H3_UpDown { get; }
        public ICylinder H4_UpDown { get; }

        public Cylinders(ICylinderFactory cylinderFactory, Inputs inputs, Outputs outputs)
        {
            _cylinderFactory = cylinderFactory;
            _inputs = inputs;
            _outputs = outputs;

            NozzleClean_H1 = _cylinderFactory
                .Create(_inputs.Nozzle1Clean, null, _outputs.NozzleCleanH1H2, null)
                .SetIdentity((int)ECylinder.NozzleClean_H1, ECylinder.NozzleClean_H1.ToString());
            NozzleClean_H1.CylinderType = ECylinderType.ForwardBackward;

            NozzleClean_H2 = _cylinderFactory
                .Create(_inputs.Nozzle2Clean, null, _outputs.NozzleCleanH1H2, null)
                .SetIdentity((int)ECylinder.NozzleClean_H2, ECylinder.NozzleClean_H2.ToString());
            NozzleClean_H2.CylinderType = ECylinderType.ForwardBackward;

            NozzleClean_H3 = _cylinderFactory
               .Create(_inputs.Nozzle3Clean, null, _outputs.NozzleCleanH3H4, null)
               .SetIdentity((int)ECylinder.NozzleClean_H3, ECylinder.NozzleClean_H3.ToString());
            NozzleClean_H3.CylinderType = ECylinderType.ForwardBackward;

            NozzleClean_H4 = _cylinderFactory
                .Create(_inputs.Nozzle4Clean, null, _outputs.NozzleCleanH3H4, null)
                .SetIdentity((int)ECylinder.NozzleClean_H4, ECylinder.NozzleClean_H4.ToString());
            NozzleClean_H4.CylinderType = ECylinderType.ForwardBackward;

            BellowUpDown = _cylinderFactory
               .Create(_inputs.BelowsUp, _inputs.BelowsDown, _outputs.BellowsUp, _outputs.BellowsDown)
               .SetIdentity((int)ECylinder.BellowUpDown, ECylinder.BellowUpDown.ToString());
            BellowUpDown.CylinderType = ECylinderType.UpDown;

            ChamberOpenClose = _cylinderFactory
               .Create(_inputs.VacChamberOpen, _inputs.VacChamberClose, _outputs.VacChamberOpen, _outputs.VacChamberClose)
               .SetIdentity((int)ECylinder.ChamberOpenClose, ECylinder.ChamberOpenClose.ToString());
            ChamberOpenClose.CylinderType = ECylinderType.OpenClose;

            AngleValve = _cylinderFactory
              .Create(_inputs.AngleValveOpen, _inputs.AngleValveClose, _outputs.ChamberVacOn, null)
              .SetIdentity((int)ECylinder.AngleValve, ECylinder.AngleValve.ToString());
            AngleValve.CylinderType = ECylinderType.ForwardBackward;

            H1_UpDown = _cylinderFactory
              .Create(_inputs.H1_CylDown, _inputs.H1_CylUp, _outputs.H1_CylDown, _outputs.H1_CylUp)
              .SetIdentity((int)ECylinder.H1_UpDown, ECylinder.H1_UpDown.ToString());
            H1_UpDown.CylinderType = ECylinderType.UpDownReverse;

            H2_UpDown = _cylinderFactory
              .Create(_inputs.H2_CylDown, _inputs.H2_CylUp, _outputs.H2_CylDown, _outputs.H2_CylUp)
              .SetIdentity((int)ECylinder.H2_UpDown, ECylinder.H2_UpDown.ToString());
            H2_UpDown.CylinderType = ECylinderType.UpDownReverse;

            H3_UpDown = _cylinderFactory
              .Create(_inputs.H3_CylDown, _inputs.H3_CylUp, _outputs.H3_CylDown, _outputs.H3_CylUp)
              .SetIdentity((int)ECylinder.H3_UpDown, ECylinder.H3_UpDown.ToString());
            H3_UpDown.CylinderType = ECylinderType.UpDownReverse;

            H4_UpDown = _cylinderFactory
              .Create(_inputs.H4_CylDown, _inputs.H4_CylUp, _outputs.H4_CylDown, _outputs.H4_CylUp)
              .SetIdentity((int)ECylinder.H4_UpDown, ECylinder.H4_UpDown.ToString());
            H4_UpDown.CylinderType = ECylinderType.UpDownReverse;
        }

        private readonly ICylinderFactory _cylinderFactory;
        private readonly Inputs _inputs;
        private readonly Outputs _outputs;
    }
}
