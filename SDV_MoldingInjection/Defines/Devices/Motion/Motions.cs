using EQX.Core.Motion;
using EQX.Motion;
using Microsoft.Extensions.DependencyInjection;
using static OpenCvSharp.Stitcher;

namespace SDV_MoldingInjection.Defines
{
    public class Motions
    {
        #region Publics
        #region Ajin Motion (legacy - commented)
        //public List<IMotion> AjinMotions { get; }
        //public IMotionMaster AjinMaster { get; }
        #endregion
        public List<IMotion> InovanceMotions { get; }
        public List<IMotion> FastechMotions { get; }
        public List<IMotion> All => InovanceMotions.Concat(FastechMotions).ToList();
        public IMotionMaster InovanceMaster { get; }
        public IMotionMaster FastechPlusRMaster { get; }
        #endregion

        public Motions([FromKeyedServices("InovanceMaster#1")] IMotionMaster inovanceMaster,
            // [FromKeyedServices("AjinMaster#1")] IMotionMaster ajinMaster,
            [FromKeyedServices("FastechPlusRMaster#1")] IMotionMaster fastechPlusRMaster,
            IEnumerable<IMotion> motions)
        {
            #region Ajin Motion (legacy - commented)
            //AjinMaster = ajinMaster;
            //AjinMotions = motions.Where(m => Enum.IsDefined(typeof(EMachineMotion), m.Name)).ToList();
            #endregion
            InovanceMaster = inovanceMaster;
            FastechPlusRMaster = fastechPlusRMaster;

            InovanceMotions = motions.Where(m => Enum.IsDefined(typeof(EMachineMotion), m.Name)).ToList();
            FastechMotions = motions.Where(m => Enum.IsDefined(typeof(EHeadMotion), m.Name)).ToList();
        }

        #region Privates
        private readonly IEnumerable<IMotion> _motions;
        #endregion

        public IMotion XAxis => InovanceMotions.First(m => m.Id == (int)EMachineMotion.XAxis);
        public IMotion StageYAxis => InovanceMotions.First(m => m.Id == (int)EMachineMotion.StageYAxis);
        public IMotion Z1Axis => InovanceMotions.First(m => m.Id == (int)EMachineMotion.Z1Axis);
        public IMotion Z2Axis => InovanceMotions.First(m => m.Id == (int)EMachineMotion.Z2Axis);
        public IMotion Z3Axis => InovanceMotions.First(m => m.Id == (int)EMachineMotion.Z3Axis);
        public IMotion Z4Axis => InovanceMotions.First(m => m.Id == (int)EMachineMotion.Z4Axis);

        public IMotion P1Axis => FastechMotions.First(m => m.Id == (int)EHeadMotion.P1Axis);
        public IMotion P2Axis => FastechMotions.First(m => m.Id == (int)EHeadMotion.P2Axis);
        public IMotion P3Axis => FastechMotions.First(m => m.Id == (int)EHeadMotion.P3Axis);
        public IMotion P4Axis => FastechMotions.First(m => m.Id == (int)EHeadMotion.P4Axis);

        public IMotion G1Axis => FastechMotions.First(m => m.Id == (int)EHeadMotion.G1Axis);
        public IMotion G2Axis => FastechMotions.First(m => m.Id == (int)EHeadMotion.G2Axis);
        public IMotion G3Axis => FastechMotions.First(m => m.Id == (int)EHeadMotion.G3Axis);
        public IMotion G4Axis => FastechMotions.First(m => m.Id == (int)EHeadMotion.G4Axis);
    }
}