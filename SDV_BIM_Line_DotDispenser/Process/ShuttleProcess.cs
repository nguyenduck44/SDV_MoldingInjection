using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.Process;
using SDV_BIM_Line_DotDispenser.Defines;
using SDV_BIM_Line_DotDispenser.Defines.Devices;
using SDV_BIM_Line_DotDispenser.Recipe;

namespace SDV_BIM_Line_DotDispenser.Process
{
    public class ShuttleProcess :ProcessBase<ESequence>
    {
        #region Privates
        private readonly RecipeList _recipeList;
        private readonly VirtualIO _virtualIO;
        private readonly Devices _devices;

        private IMotion XAxis => _devices.Motions.ShuttleXAxis;
        #endregion

        #region Flags
        private IDInputDevice VirtualInput => _virtualIO.ShuttleProcessInput;
        private IDOutputDevice VirtualOutput => _virtualIO.ShuttleProcessOutput;
        private bool FlagIn_TransferLoadDone
        {
            get
            {
                return VirtualInput[EShuttleProcessInput.TRANSFER_LOAD_DONE].Value;
            }
        }

        private bool FlagOut_ShuttleRequestLoad
        {
            set
            {
                VirtualOutput[EShuttleProcessOutput.SHUTTLE_REQUEST_LOAD].Value = value;
            }
        }
        #endregion

        #region Constructor
        public ShuttleProcess(RecipeList recipeList,
            VirtualIO virtualIO,
            Devices devices)
        {
            _recipeList = recipeList;
            _virtualIO = virtualIO;
            _devices = devices;
        }
        #endregion

        #region Override Methods
        public override bool ProcessOrigin()
        {
            switch ((EShuttleProcessOriginStep)Step.OriginStep)
            {
                case EShuttleProcessOriginStep.Start:
                    Log.Debug("Origin Start");
                    Step.OriginStep++;
                    break;
                case EShuttleProcessOriginStep.XAxis_Origin:
                    Log.Debug("X Axis Origin");
                    XAxis.SearchOrigin();
                    Wait((int)(_recipeList.CommonRecipe.MotionOriginTimeout * 1000), () => XAxis.Status.IsHomeDone);
                    Step.OriginStep++;
                    break;
                case EShuttleProcessOriginStep.XAxis_Origin_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseAlarm(EAlarm.ShuttleXAxis_OriginTimeout);
                        break;
                    }
                    Log.Debug("X Axis Origin Done");
                    Step.OriginStep++;
                    break;
                case EShuttleProcessOriginStep.End:
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
            switch ((EShuttleProcessLoadStep)Step.RunStep)
            {
                case EShuttleProcessLoadStep.Start:
                    Log.Debug("Load Start");
                    Step.RunStep++;
                    break;
                case EShuttleProcessLoadStep.XAxis_MoveLoadPosition:
                    Log.Debug("X Axis Move Load Position");
                    Wait(3000);
                    Step.RunStep++;
                    break;
                case EShuttleProcessLoadStep.XAxis_MoveLoadPosition_Wait:
                    Log.Debug("X Axis Move Load Position Done");
                    Step.RunStep++;
                    break;
                case EShuttleProcessLoadStep.Set_FlagRequestLoad:
                    Log.Debug("Set Flag ShuttleRequestLoad");
                    FlagOut_ShuttleRequestLoad = true;
                    Step.RunStep++;
                    break;
                case EShuttleProcessLoadStep.Wait_TransferLoadDone:
                    if(FlagIn_TransferLoadDone == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("Transfer Load Done => Clear Flag ShuttleRequestLoad");
                    FlagOut_ShuttleRequestLoad = false;
                    Step.RunStep++;
                    break;
                case EShuttleProcessLoadStep.End:
                    Log.Debug("Load End");
                    Sequence = ESequence.Unload;
                    break;
            }
        }

        private void Sequence_Unload()
        {
            Sequence = ESequence.Load;
        }
        #endregion
    }
}
