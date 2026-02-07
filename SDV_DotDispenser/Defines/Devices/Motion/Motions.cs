using EQX.Core.Motion;
using EQX.Motion;
using Microsoft.Extensions.DependencyInjection;

namespace SDV_DotDispenser.Defines
{
    public class Motions
    {
        public Motions([FromKeyedServices("AjinMaster#1")] IMotionMaster ajinMaster,
            [FromKeyedServices("AjinMotionFactory")] IMotionFactory<IMotion> ajinFactory,
            [FromKeyedServices("MotionAjinParameters")] List<IMotionParameter> ajinParameters)
        {
            AjinMaster = ajinMaster;

            AjinMotions = new MotionList<EMotion>(ajinFactory, ajinParameters);
        }

        #region Publics
        public readonly MotionList<EMotion> AjinMotions;

        public List<IMotion> All => AjinMotions.All;
        public IMotionMaster AjinMaster { get; }
        #endregion

        public IMotion StageY1Axis => AjinMotions.All.First(m => m.Id == (int)EMotion.StageY1Axis);
        public IMotion StageY2Axis => AjinMotions.All.First(m => m.Id == (int)EMotion.StageY2Axis);
        public IMotion DispenserHeadXAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.DispenserHeadXAxis);
        public IMotion DispenserHeadZAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.DispenserHeadZAxis);
        public IMotion InspectVisionXAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.InspectVisionXAxis);
        public IMotion TransferXAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.TransferXAxis);
        public IMotion TransferZAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.TransferZAxis);
    }
}