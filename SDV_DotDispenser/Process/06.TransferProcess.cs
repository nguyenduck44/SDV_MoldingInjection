using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.Process;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;
using SDV_DotDispenser.Recipe;

namespace SDV_DotDispenser.Process
{
    public class TransferProcess : ProcessBase<ESequence>
    {
        #region Privates
        private readonly Devices _devices;
        private readonly VirtualIO _virtualIO;
        private readonly RecipeList _recipeList;

        private IMotion XAxis => _devices.Motions.TransferXAxis;
        private IMotion ZAxis => _devices.Motions.TransferZAxis;
        #endregion

        #region Flags
        private IDInputDevice VirtualInput => _virtualIO.TransferProcessInput;
        private IDOutputDevice VirtualOutput => _virtualIO.TransferProcessOutput;
        #endregion

        #region Constructor
        public TransferProcess(Devices devices,
            VirtualIO virtualIO,
            RecipeList recipeList)
        {
            _devices = devices;
            _virtualIO = virtualIO;
            _recipeList = recipeList;
        }
        #endregion

        #region Override Methods
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
                    Wait((int)(_recipeList.CommonRecipe.MotionOriginTimeout * 1000), () => ZAxis.Status.IsHomeDone);
                    Step.OriginStep++;
                    break;
                case ETransferProcessOriginStep.ZAxis_Origin_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseAlarm(EAlarm.Transfer_ZAxis_OriginFail);
                        break;
                    }

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
                case ESequence.Load:
                    Sequence_Load();
                    break;
                case ESequence.Unload:
                    Sequence_Unload();
                    break;
            }
            return true;
        }
        #endregion

        #region Sequences
        private void Sequence_AutoRun()
        {
            Sequence = ESequence.Load;
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
                    Sequence = ESequence.Unload;
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
                    Sequence = ESequence.Load;
                    break;
            }
        }
        #endregion
    }
}
