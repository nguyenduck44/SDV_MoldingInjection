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
        #region Privates
        private readonly Devices _devices;
        private readonly RecipeList _recipeList;
        private readonly DispensingRecipe _dispensingRecipe;
        #endregion

        #region Motions
        private IMotion XAxis => _devices.Motions.DispenserHeadXAxis;
        private IMotion ZAxis => _devices.Motions.DispenserHeadZAxis;
        #endregion

        #region Process IOs
        private IDInputDevice<EDispenserProcInput> procInputs;
        private IDOutputDevice<EDispenserProcOutput> procOutputs;
        #endregion

        public DispenserProcess(Devices devices,
            RecipeList recipeList,
            VirtualIO virtualIO)
        {
            _devices = devices;
            _recipeList = recipeList;

            _dispensingRecipe = _recipeList.DispensingRecipe;

            procInputs = virtualIO.DispenserProcInput;
            procOutputs = virtualIO.DispenserProcOutput;
        }

        #region Process Methods
        public override bool ProcessToRun()
        {
            switch ((EDispenserProcessToRunStep)Step.ToRunStep)
            {
                case EDispenserProcessToRunStep.Start:
                    Log.Debug("ToRun Start");
                    Step.ToRunStep++;
                    break;
                case EDispenserProcessToRunStep.ZAxis_Up:
                    if (ZAxis.Status.ActualPosition >= _dispensingRecipe.ZAxis_ReadyPosition)
                    {
                        Step.ToRunStep = (int)EDispenserProcessToRunStep.Clear_ProcOutputs;
                        break;
                    }

                    ZAxis.MoveAbs(_dispensingRecipe.ZAxis_ReadyPosition);
                    Wait(_recipeList.CommonRecipe.MotionMoveTimeout,
                        () => ZAxis.IsOnPosition(_dispensingRecipe.ZAxis_ReadyPosition));

                    Log.Debug($"Move {ZAxis} to ReadyPosition [{_dispensingRecipe.ZAxis_ReadyPosition} mm]");
                    Step.ToRunStep++;
                    break;
                case EDispenserProcessToRunStep.ZAxis_UpWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Dispenser_ZAxis_MoveReadyFail);
                        break;
                    }

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
        #endregion

        #region Sequence Methods
        #endregion

        #region Private Methods
        #endregion
    }
}
