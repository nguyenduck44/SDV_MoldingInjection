using EQX.Core.Motion;
using EQX.Motion;
using Microsoft.Extensions.DependencyInjection;

namespace SDV_BIM_Line_DotDispenser.Defines
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

        #region AJINEXTEK MOTIONS
        public IMotion TransferXAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.TransferXAxis);
        public IMotion TransferYAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.TransferYAxis);
        public IMotion TransferZAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.TransferZAxis);
        public IMotion ShuttleXAxis => AjinMotions.All.First(m => m.Id == (int)EMotion.ShuttleXAxis);
        #endregion
    }
}