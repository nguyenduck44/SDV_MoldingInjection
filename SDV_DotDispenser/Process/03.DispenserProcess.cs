using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.Process;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;
using SDV_DotDispenser.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Process
{
    public class DispenserProcess : DDProcess
    {
        private readonly Devices _devices;
        private readonly RecipeList _recipeList;

        private IMotion XAxis => _devices.Motions.DispenserHeadXAxis;
        private IMotion ZAxis => _devices.Motions.DispenserHeadZAxis;

        private IDInputDevice<EDispenserProcInput> procInputs;
        private IDOutputDevice<EDispenserProcOutput> procOutputs;

        public DispenserProcess(Devices devices,
            RecipeList recipeList,
            VirtualIO virtualIO)
        {
            _devices = devices;
            _recipeList = recipeList;

            procInputs = virtualIO.DispenserProcInput;
            procOutputs = virtualIO.DispenserProcOutput;
        }

        public override bool ProcessToRun()
        {
            switch ((EDispenserProcessToRunStep)Step.ToRunStep)
            {
                case EDispenserProcessToRunStep.Start:
                    Log.Debug("ToRun Start");
                    Step.ToRunStep++;
                    break;
                case EDispenserProcessToRunStep.Clear_ProcOutputs:
                    Log.Debug($"{procOutputs.Name} ClearOutputs");
                    procOutputs.ClearOutputs();
                    Step.ToRunStep++;
                    break;
                case EDispenserProcessToRunStep.End:
                    Log.Debug("ToRun End");
                    Step.ToRunStep++;
                    ProcessStatus = EProcessStatus.ToRunDone;
                    break;
            }

            return true;
        }

        public override bool ProcessOrigin()
        {
            switch ((EDispenserProcessOriginStep)Step.OriginStep)
            {
                case EDispenserProcessOriginStep.Start:
                    Log.Debug("Origin Start");
                    Step.OriginStep++;
                    break;
                case EDispenserProcessOriginStep.ZAxis_Origin:
                    Log.Debug("Z Axis Origin");
                    ZAxis.SearchOrigin();
                    Wait((int)(_recipeList.CommonRecipe.MotionOriginTimeout * 1000), () => ZAxis.Status.IsHomeDone);
                    Step.OriginStep++;
                    break;
                case EDispenserProcessOriginStep.ZAxis_Origin_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseAlarm(EAlarm.DispenserHead_ZAxis_OriginFail);
                        break;
                    }

                    procOutputs[EDispenserProcOutput.ZAxis_AtOrigin].Value = true;

                    Log.Debug("Z Axis Origin Done");
                    Step.OriginStep++;
                    break;
                case EDispenserProcessOriginStep.XAxis_Origin:
                    Log.Debug("X Axis Origin");
                    XAxis.SearchOrigin();
                    Wait((int)(_recipeList.CommonRecipe.MotionOriginTimeout * 1000), () => XAxis.Status.IsHomeDone);
                    Step.OriginStep++;
                    break;
                case EDispenserProcessOriginStep.XAxis_Origin_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseAlarm(EAlarm.Transfer_XAxis_OriginFail);
                        break;
                    }

                    Log.Debug("X Axis Origin Done");
                    Step.OriginStep++;
                    break;
                case EDispenserProcessOriginStep.End:
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
