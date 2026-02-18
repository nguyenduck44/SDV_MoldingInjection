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

        public ICylinder BellowCyl { get; }

        public ICylinder ChamberOpenClose { get; }

        public ICylinder AngleValve { get; }

        public ICylinder PistonCyl_H1 { get; }
        public ICylinder PistonCyl_H2 { get; }
        public ICylinder PistonCyl_H3 { get; }
        public ICylinder PistonCyl_H4 { get; }

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

            BellowCyl = _cylinderFactory
               .Create(_inputs.BelowsUp, _inputs.BelowsDown, _outputs.BellowsUp, _outputs.BellowsDown)
               .SetIdentity((int)ECylinder.BellowCyl, ECylinder.BellowCyl.ToString());
            BellowCyl.CylinderType = ECylinderType.UpDown;

            ChamberOpenClose = _cylinderFactory
               .Create(_inputs.ChamberOpen, _inputs.ChamberClose, _outputs.VacChamberOpen, _outputs.VacChamberClose)
               .SetIdentity((int)ECylinder.ChamberOpenClose, ECylinder.ChamberOpenClose.ToString());
            ChamberOpenClose.CylinderType = ECylinderType.OpenClose;

            AngleValve = _cylinderFactory
              .Create(_inputs.AngleValveOpen, _inputs.AngleValveClose, _outputs.ChamberVacOn, null)
              .SetIdentity((int)ECylinder.AngleValve, ECylinder.AngleValve.ToString());
            AngleValve.CylinderType = ECylinderType.OpenClose;

            PistonCyl_H1 = _cylinderFactory
              .Create(_inputs.H1_CylDown, _inputs.H1_CylUp, _outputs.H1_CylDown, _outputs.H1_CylUp)
              .SetIdentity((int)ECylinder.PistonCyl_H1, ECylinder.PistonCyl_H1.ToString());
            PistonCyl_H1.CylinderType = ECylinderType.UpDownReverse;

            PistonCyl_H2 = _cylinderFactory
              .Create(_inputs.H2_CylDown, _inputs.H2_CylUp, _outputs.H2_CylDown, _outputs.H2_CylUp)
              .SetIdentity((int)ECylinder.PistonCyl_H2, ECylinder.PistonCyl_H2.ToString());
            PistonCyl_H2.CylinderType = ECylinderType.UpDownReverse;

            PistonCyl_H3 = _cylinderFactory
              .Create(_inputs.H3_CylDown, _inputs.H3_CylUp, _outputs.H3_CylDown, _outputs.H3_CylUp)
              .SetIdentity((int)ECylinder.PistonCyl_H3, ECylinder.PistonCyl_H3.ToString());
            PistonCyl_H3.CylinderType = ECylinderType.UpDownReverse;

            PistonCyl_H4 = _cylinderFactory
              .Create(_inputs.H4_CylDown, _inputs.H4_CylUp, _outputs.H4_CylDown, _outputs.H4_CylUp)
              .SetIdentity((int)ECylinder.PistonCyl_H4, ECylinder.PistonCyl_H4.ToString());
            PistonCyl_H4.CylinderType = ECylinderType.UpDownReverse;
        }

        private readonly ICylinderFactory _cylinderFactory;
        private readonly Inputs _inputs;
        private readonly Outputs _outputs;
    }
}
