using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.Process;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;
using SDV_DotDispenser.Recipe;

namespace SDV_DotDispenser.Process
{
    public class TransferProcess : DDProcess
    {
        #region Privates
        private readonly Devices _devices;
        private readonly VirtualIO _virtualIO;
        private readonly RecipeList _recipeList;

        private IMotion XAxis => _devices.Motions.TransferXAxis;
        private IMotion ZAxis => _devices.Motions.TransferZAxis;
        #endregion

        #region Cylinder
        private ICylinder TransferHand1Cyl => _devices.Cylinders.TransferHand1Cyl;
        private ICylinder TransferHand2Cyl => _devices.Cylinders.TransferHand2Cyl;
        #endregion

        #region Process IOs
        private IDInputDevice<ETransferProcInput> procInputs;
        private IDOutputDevice<ETransferProcOutput> procOutputs;
        #endregion

        #region Constructor
        public TransferProcess(Devices devices,
            RecipeList recipeList,
            VirtualIO virtualIO)
        {
            _devices = devices;
            _recipeList = recipeList;

            procInputs = virtualIO.TransferProcInput;
            procOutputs = virtualIO.TransferProcOutput;
        }
        #endregion

        #region Override Methods
        public override bool ProcessToRun()
        {
            switch ((ETransferProcessToRunStep)Step.ToRunStep)
            {
                case ETransferProcessToRunStep.Start:
                    Log.Debug("ToRun Start");
                    Step.ToRunStep++;
                    break;
                case ETransferProcessToRunStep.Clear_ProcOutputs:
                    Log.Debug($"{procOutputs.Name} ClearOutputs");
                    procOutputs.ClearOutputs();
                    Step.ToRunStep++;
                    break;
                case ETransferProcessToRunStep.End:
                    Log.Debug("ToRun End");
                    Step.ToRunStep++;
                    ProcessStatus = EProcessStatus.ToRunDone;
                    break;
            }

            return true;
        }

        public override bool ProcessOrigin()
        {
            switch ((ETransferProcessOriginStep)Step.OriginStep)
            {
                case ETransferProcessOriginStep.Start:
                    Log.Debug("Origin Start");
                    Step.OriginStep++;
                    break;
                case ETransferProcessOriginStep.ZAxis_Origin:
                    Log.Debug("Z Axis Origin");
                    ZAxis.SearchOrigin();
                    TransferHand1Cyl.Backward();
                    TransferHand2Cyl.Backward();

                    Wait((int)(_recipeList.CommonRecipe.MotionOriginTimeout * 1000),
                        () => ZAxis.Status.IsHomeDone && TransferHand1Cyl.IsBackward && TransferHand2Cyl.IsBackward);

                    Step.OriginStep++;
                    break;
                case ETransferProcessOriginStep.ZAxis_Origin_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (ZAxis.Status.IsHomeDone == false) RaiseAlarm(EAlarm.Transfer_ZAxis_OriginFail);
                        if (TransferHand1Cyl.IsBackward == false) RaiseAlarm(EWarning.Transfer_Hand1Cyl_BackwardFail);
                        if (TransferHand2Cyl.IsBackward == false) RaiseAlarm(EWarning.Transfer_Hand2Cyl_BackwardFail);
                        break;
                    }

                    Thread.Sleep(5000);

                    procOutputs[ETransferProcOutput.ZAxis_AtOrigin].Value = true;

                    Log.Debug("Z Axis Origin Done");
                    Step.OriginStep++;
                    break;
                case ETransferProcessOriginStep.XAxis_Origin:
                    Log.Debug("X Axis Origin");
                    XAxis.SearchOrigin();
                    Wait((int)(_recipeList.CommonRecipe.MotionOriginTimeout * 1000), () => XAxis.Status.IsHomeDone);
                    Step.OriginStep++;
                    break;
                case ETransferProcessOriginStep.XAxis_Origin_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseAlarm(EAlarm.Transfer_XAxis_OriginFail);
                        break;
                    }

                    Log.Debug("X Axis Origin Done");
                    Step.OriginStep++;
                    break;
                case ETransferProcessOriginStep.End:
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

        public override bool ProcessRun()
        {
            switch (Sequence)
            {
                case ESequence.Stop:
                    Wait(50);
                    break;
                case ESequence.AutoRun:
                    Sequence_AutoRun();
                    break;
                case ESequence.Ready:
                    Sequence = ESequence.Stop;
                    break;
            }
            return true;
        }
        #endregion

        #region Sequences
        private void Sequence_AutoRun()
        {
            //Sequence = ESequence.Load;
        }

        private void Sequence_Load()
        {
            switch ((ETransferProcessLoadStep)Step.RunStep)
            {
                case ETransferProcessLoadStep.Start:
                    Log.Debug("Load Start");
                    Step.RunStep++;
                    break;
                case ETransferProcessLoadStep.End:
                    Log.Debug("Load End");
                    Sequence = ESequence.LeftUnload;
                    break;
            }
        }

        private void Sequence_Unload()
        {
            switch ((ETransferProcessUnloadStep)Step.RunStep)
            {
                case ETransferProcessUnloadStep.Start:
                    Log.Debug("Unload Start");
                    Step.RunStep++;
                    break;
                case ETransferProcessUnloadStep.XY_Axis_MoveUnloadPosition:
                    Log.Debug("XY Axis Move to Unload Position");
                    Wait(3000);
                    Step.RunStep++;
                    break;
                case ETransferProcessUnloadStep.XY_Axis_MoveUnloadPosition_Wait:
                    Log.Debug("XY Axis Move to Unload Position Done");
                    Step.RunStep++;
                    break;
                case ETransferProcessUnloadStep.ZAxis_MoveUnloadPosition:
                    Log.Debug("Z Axis Move to Unload Position");
                    Wait(5000);
                    Step.RunStep++;
                    break;
                case ETransferProcessUnloadStep.ZAxis_MoveUnloadPosition_Wait:
                    Log.Debug("Z Axis Move to Unload Position Done");
                    Step.RunStep++;
                    break;
                case ETransferProcessUnloadStep.End:
                    Log.Debug("Unload End");
                    //Sequence = ESequence.Load;
                    break;
            }
        }
        #endregion
    }
}
