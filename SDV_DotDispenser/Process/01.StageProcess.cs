using EQX.Core.InOut;
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
        #region Properties

        #endregion

        #region Motions
        private IMotion YAxis => port == EPort.Left ? _devices.Motions.StageY1Axis : _devices.Motions.StageY2Axis;
        #endregion

        #region Process IOs
        private IDInputDevice<EStageProcInput> procInputs => Name == EProcess.StageLeft.ToString() ?
            _virtualIO.LeftStageProcInput : _virtualIO.RightStageProcInput;
        private IDOutputDevice<EStageProcOutput> procOutputs => Name == EProcess.StageLeft.ToString() ?
            _virtualIO.LeftStageProcOutput : _virtualIO.RightStageProcOutput;
        #endregion

        #region Constructors
        public StageProcess(Devices devices, RecipeList recipeList, VirtualIO virtualIO)
        {
            _devices = devices;
            _recipeList = recipeList;
            _virtualIO = virtualIO;
        }
        #endregion

        #region Process Methods
        public override bool ProcessToRun()
        {
            switch ((EStageProcessToRunStep)Step.ToRunStep)
            {
                case EStageProcessToRunStep.Start:
                    Log.Debug("ToRun Start");
                    Step.ToRunStep++;
                    break;
                case EStageProcessToRunStep.Clear_ProcOutputs:
                    Log.Debug($"{procOutputs.Name} ClearOutputs");
                    procOutputs.ClearOutputs();
                    Step.ToRunStep++;
                    break;
                case EStageProcessToRunStep.End:
                    Log.Debug("ToRun End");
                    Step.ToRunStep++;
                    ProcessStatus = EProcessStatus.ToRunDone;
                    break;
            }

            return true;
        }

        public override bool ProcessOrigin()
        {
            switch ((EStageProcessOriginStep)Step.OriginStep)
            {
                case EStageProcessOriginStep.Start:
                    Log.Debug("Origin Start");
                    Step.OriginStep++;
                    break;
                case EStageProcessOriginStep.Wait_ZAxis_HomeDone:
                    if ((procInputs[EStageProcInput.Dispenser_ZAxis_AtOrigin].Value &&
                        procInputs[EStageProcInput.Transfer_ZAxis_AtOrigin].Value) == false)
                    {
                        Wait(100);
                    }

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
                    Sequence_Ready();
                    break;
                case ESequence.LeftLoad:
                    if (port == EPort.Left) Sequence_Load();
                    else Sequence = ESequence.Stop;
                    break;
                case ESequence.RightLoad:
                    if (port == EPort.Left) Sequence = ESequence.Stop;
                    else Sequence_Load();
                    break;
                case ESequence.LeftPlasma:
                    if (port == EPort.Left) Sequence_Plasma();
                    else Sequence = ESequence.Stop;
                    break;
                case ESequence.RightPlasma:
                    if (port == EPort.Left) Sequence = ESequence.Stop;
                    else Sequence_Plasma();
                    break;
                case ESequence.LeftAlignVision:
                    if (port == EPort.Left) Sequence_AlignVision();
                    else Sequence = ESequence.Stop;
                    break;
                case ESequence.RightAlignVision:
                    if (port == EPort.Left) Sequence = ESequence.Stop;
                    else Sequence_AlignVision();
                    break;
                case ESequence.LeftDispensing:
                    if (port == EPort.Left) Sequence_Dispensing();
                    else Sequence = ESequence.Stop;
                    break;
                case ESequence.RightDispensing:
                    if (port == EPort.Left) Sequence = ESequence.Stop;
                    else Sequence_Dispensing();
                    break;
                case ESequence.LeftFinalInspection:
                    if (port == EPort.Left) Sequence_FinalInspection();
                    else Sequence = ESequence.Stop;
                    break;
                case ESequence.RightFinalInspection:
                    if (port == EPort.Left) Sequence = ESequence.Stop;
                    else Sequence_FinalInspection();
                    break;
                case ESequence.LeftUVCure:
                    if (port == EPort.Left) Sequence_UVCure();
                    else Sequence = ESequence.Stop;
                    break;
                case ESequence.RightUVCure:
                    if (port == EPort.Left) Sequence = ESequence.Stop;
                    else Sequence_UVCure();
                    break;
                case ESequence.LeftUnload:
                    if (port == EPort.Left) Sequence_Unload();
                    else Sequence = ESequence.Stop;
                    break;
                case ESequence.RightUnload:
                    if (port == EPort.Left) Sequence = ESequence.Stop;
                    else Sequence_Unload();
                    break;
            }

            return true;
        }
        #endregion

        #region Sequence Methods
        private void Sequence_AutoRun()
        {
        }

        private void Sequence_Ready()
        {
        }

        private void Sequence_Load()
        {
        }

        private void Sequence_Plasma()
        {
        }

        private void Sequence_AlignVision()
        {
        }

        private void Sequence_Dispensing()
        {
        }

        private void Sequence_FinalInspection()
        {
        }

        private void Sequence_UVCure()
        {
        }

        private void Sequence_Unload()
        {
        }
        #endregion

        #region Privates
        private EPort port => Name == EProcess.StageLeft.ToString() ? EPort.Left : EPort.Right;

        private readonly Devices _devices;
        private readonly RecipeList _recipeList;
        private readonly VirtualIO _virtualIO;
        #endregion

    }
}
