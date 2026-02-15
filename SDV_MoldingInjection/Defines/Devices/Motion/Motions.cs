using EQX.Core.Motion;
using EQX.Motion;
using Microsoft.Extensions.DependencyInjection;
using static OpenCvSharp.Stitcher;

namespace SDV_MoldingInjection.Defines
{
    public class Motions
    {
        #region Publics
        public List<IMotion> AjinMotions { get; }
        public List<IMotion> FastechMotions { get; }
        public List<IMotion> All => AjinMotions.Concat(FastechMotions).ToList();
        public IMotionMaster AjinMaster { get; }
        #endregion

        public Motions([FromKeyedServices("AjinMaster#1")] IMotionMaster ajinMaster,
            IEnumerable<IMotion> motions)
        {
            AjinMaster = ajinMaster;
            AjinMotions = motions.Where(m => Enum.IsDefined(typeof(EMachineMotion), m.Name)).ToList();
            FastechMotions = motions.Where(m => Enum.IsDefined(typeof(EHeadMotion), m.Name)).ToList();
        }

        #region Privates
        private readonly IEnumerable<IMotion> _motions;
        #endregion

        public IMotion XAxis => All.First(m => m.Id == (int)EMachineMotion.XAxis);
        public IMotion StageYAxis => All.First(m => m.Id == (int)EMachineMotion.StageYAxis);
        public IMotion Z1Axis => All.First(m => m.Id == (int)EMachineMotion.Z1Axis);
        public IMotion Z2Axis => All.First(m => m.Id == (int)EMachineMotion.Z2Axis);
        public IMotion Z3Axis => All.First(m => m.Id == (int)EMachineMotion.Z3Axis);
        public IMotion Z4Axis => All.First(m => m.Id == (int)EMachineMotion.Z4Axis);

        public IMotion P1Axis => All.First(m => m.Id == (int)EHeadMotion.P1Axis);
        public IMotion P2Axis => All.First(m => m.Id == (int)EHeadMotion.P2Axis);
        public IMotion P3Axis => All.First(m => m.Id == (int)EHeadMotion.P3Axis);
        public IMotion P4Axis => All.First(m => m.Id == (int)EHeadMotion.P4Axis);

        public IMotion G1Axis => All.First(m => m.Id == (int)EHeadMotion.G1Axis);
        public IMotion G2Axis => All.First(m => m.Id == (int)EHeadMotion.G2Axis);
        public IMotion G3Axis => All.First(m => m.Id == (int)EHeadMotion.G3Axis);
        public IMotion G4Axis => All.First(m => m.Id == (int)EHeadMotion.G4Axis);
    }
}