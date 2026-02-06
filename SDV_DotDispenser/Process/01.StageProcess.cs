using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.Process;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;
using SDV_DotDispenser.Recipe;

namespace SDV_DotDispenser.Process
{
    /// <summary>
    /// SDV Dot Dispenser Process
    /// </summary>
    public class DDProcess : ProcessBase<ESequence> { }

    public class StageProcess : DDProcess
    {
        private readonly Devices _devices;
        private readonly RecipeList _recipeList;

        private EPort port => Name == EProcess.StageLeft.ToString() ? EPort.Left : EPort.Right;
        private IMotion YAxis => port == EPort.Left ? _devices.Motions.StageY1Axis : _devices.Motions.StageY2Axis;

        public StageProcess(Devices devices, RecipeList recipeList)
        {
            _devices = devices;
            _recipeList = recipeList;
        }

        public override bool ProcessOrigin()
        {
            switch ((EStageProcessOriginStep)Step.OriginStep)
            {
                case EStageProcessOriginStep.Start:
                    Log.Debug("Origin Start");
                    Step.OriginStep++;
                    break;
                case EStageProcessOriginStep.YAxis_Origin:
                    Log.Debug("YAxis Origin");
                    YAxis.SearchOrigin();
                    Wait((int)(_recipeList.CommonRecipe.MotionOriginTimeout * 1000), () => YAxis.Status.IsHomeDone);
                    Step.OriginStep++;
                    break;
                case EStageProcessOriginStep.YAxis_Origin_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseAlarm(port == EPort.Left ? EAlarm.LeftStage_YAxis_OriginFail : EAlarm.RightStage_YAxis_OriginFail);
                        break;
                    }

                    Log.Debug("YAxis Origin Done");
                    Step.OriginStep++;
                    break;
                case EStageProcessOriginStep.End:
                    Log.Debug("Origin End");
                    ProcessStatus = EProcessStatus.OriginDone;
                    Step.OriginStep++;
                    break;
                default:
                    Wait(20);
                    break;
            }
            return true;
        }
    }
}
