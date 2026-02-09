using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;
using SDV_DotDispenser.Recipe;

namespace SDV_DotDispenser.Process
{
    public class StageProcess : DDProcess
    {
        #region Properties
        public EPanelStatus PanelStatus { get; set; }

        private bool IsSWStart => port == EPort.Left ? _devices.Inputs.SWStartLeft.Value
                                                       : _devices.Inputs.SWStartRight.Value;

        private IDOutput OutMuting1 => port == EPort.Left ? _devices.Outputs.LightCurtainLeftMuting1 :
                                                            _devices.Outputs.LightCurtainRightMuting1;

        private IDOutput OutMuting2 => port == EPort.Left ? _devices.Outputs.LightCurtainLeftMuting2 :
                                                            _devices.Outputs.LightCurtainRightMuting2;

        private double YAxisLoadPosition => port == EPort.Left ? _recipeList.StageLeftRecipe.YAxisLoadPosition :
                                                                 _recipeList.StageRightRecipe.YAxisLoadPosition;

        private double YAxisPlasmaStartPosition => port == EPort.Left ? _recipeList.StageLeftRecipe.YAxisPlasmaStartPosition :
                                                                        _recipeList.StageRightRecipe.YAxisPlasmaStartPosition;

        private double YAxisPlasmaEndPosition => port == EPort.Left ? _recipeList.StageLeftRecipe.YAxisPlasmaEndPosition :
                                                                      _recipeList.StageRightRecipe.YAxisPlasmaEndPosition;
        #endregion

        #region Motions
        private IMotion YAxis => port == EPort.Left ? _devices.Motions.StageY1Axis : _devices.Motions.StageY2Axis;
        #endregion

        #region ADIOs
        private IDInput In_LeftVacuum => port == EPort.Left
            ? _devices.Inputs.StageLLeftVacOn
            : _devices.Inputs.StageRLeftVacOn;

        private IDInput In_RightVacuum => port == EPort.Left
           ? _devices.Inputs.StageLRightVacOn
           : _devices.Inputs.StageRRightVacOn;
        #endregion

        #region Process IOs
        private IDInputDevice<EStageProcInput> procInputs => Name == EProcess.StageLeft.ToString() ?
            _virtualIO.LeftStageProcInput : _virtualIO.RightStageProcInput;
        private IDOutputDevice<EStageProcOutput> procOutputs => Name == EProcess.StageLeft.ToString() ?
            _virtualIO.LeftStageProcOutput : _virtualIO.RightStageProcOutput;
        #endregion

        #region Constructors
        public StageProcess(Devices devices, RecipeList recipeList, ProcessIO virtualIO)
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
                    if ((procInputs[EStageProcInput.DISPENSER_Z_AT_ORIGIN].Value &&
                        procInputs[EStageProcInput.TRANSFER_Z_AT_ORIGIN].Value) == false)
                    {
                        Wait(100);
                        break;
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
            switch ((EStageProcessAutoRunStep)Step.RunStep)
            {
                case EStageProcessAutoRunStep.Start:
                    Log.Debug("AutoRun Start");
                    Step.RunStep++;
                    break;
                case EStageProcessAutoRunStep.Stage_VacCheck:
                    EVacuumStatus vacuumStatus = VacCheck();
                    if (vacuumStatus == EVacuumStatus.Fail)
                    {
                        RaiseWarning(port == EPort.Left
                            ? EWarning.LeftStage_VacuumStatus_Fail
                            : EWarning.RightStage_VacuumStatus_Fail);
                        break;
                    }

                    if (vacuumStatus == EVacuumStatus.Off)
                    {
                        Log.Debug("Stage vacuum not detect");
                        Sequence = port == EPort.Left
                            ? ESequence.LeftLoad
                            : ESequence.RightLoad;
                        break;
                    }

                    Log.Debug("Stage vacuum on detect");
                    Step.RunStep++;
                    break;
                case EStageProcessAutoRunStep.Panel_StatusCheck:
                    switch (PanelStatus)
                    {
                        case EPanelStatus.None:
                        case EPanelStatus.InPlasma:
                            Sequence = port == EPort.Left
                                ? ESequence.LeftPlasma
                                : ESequence.RightPlasma;
                            break;
                        case EPanelStatus.PlasmaDone:
                        case EPanelStatus.InVisionAlign:
                            Sequence = port == EPort.Left
                                ? ESequence.LeftAlignVision
                                : ESequence.RightAlignVision;
                            break;
                        case EPanelStatus.VisionAlignDone:
                        case EPanelStatus.InDispensing:
                            Sequence = port == EPort.Left
                                ? ESequence.LeftDispensing
                                : ESequence.RightDispensing;
                            break;
                        case EPanelStatus.DispensingDone:
                        case EPanelStatus.InCuring:
                            Sequence = port == EPort.Left
                                ? ESequence.LeftUVCure
                                : ESequence.RightUVCure;
                            break;
                        case EPanelStatus.CuringDone:
                        case EPanelStatus.InFinalInspection:
                            Sequence = port == EPort.Left
                                ? ESequence.LeftFinalInspection
                                : ESequence.RightFinalInspection;
                            break;
                        case EPanelStatus.FinalInspectionDone_OK:
                            Sequence = port == EPort.Left
                               ? ESequence.LeftUnload
                               : ESequence.RightUnload;
                            break;
                        case EPanelStatus.FinalInspectionDone_NG:
                            Sequence = port == EPort.Left
                               ? ESequence.LeftNGUnload
                               : ESequence.RightNGUnload;
                            break;
                    }

                    Log.Info($"PanelStatus is {PanelStatus}, next Sequence set as {Sequence}");
                    break;
                case EStageProcessAutoRunStep.End:
                    Log.Debug("AutoRun End");
                    Step.RunStep++;
                    break;
            }
        }

        private void Sequence_Ready()
        {
        }

        private void Sequence_Load()
        {
            switch ((EStageProcessLoadStep)Step.RunStep)
            {
                case EStageProcessLoadStep.Start:
                    Log.Debug("Load Start");
                    Step.RunStep++;
                    break;
                case EStageProcessLoadStep.YAxis_MoveLoadPosition:
                    Log.Debug("Y Axis Move Load Position");
                    YAxis.MoveAbs(YAxisLoadPosition);
                    Wait(_recipeList.CommonRecipe.MotionMoveTimeout, () => YAxis.IsOnPosition(YAxisLoadPosition));
                    Step.RunStep++;
                    break;
                case EStageProcessLoadStep.YAxis_MoveLoadPosition_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseAlarm(port == EPort.Left ? EAlarm.LeftStage_YAxis_MoveLoadPositionFail
                                                      : EAlarm.RightStage_YAxis_MoveLoadPositionFail);
                        break;
                    }

                    Log.Debug("Y Axis Move Load Position Done");
                    Step.RunStep++;
                    break;
                case EStageProcessLoadStep.MutingLightCurtain:
                    Log.Debug("Muting Light Curtain");
                    MutingLightCurtain(true);
                    Step.RunStep++;
                    break;
                case EStageProcessLoadStep.Wait_SWStart:
                    if (IsSWStart == false)
                    {
                        Wait(100);
                        break;
                    }

                    Log.Debug("SW Start Detected");
                    Step.RunStep++;
                    break;
                case EStageProcessLoadStep.EnableLightCurtain:
                    Log.Debug("Enable Light Curtain");
                    MutingLightCurtain(false);
                    Step.RunStep++;
                    break;
                case EStageProcessLoadStep.VacuumStatus_Check:
                    var vacuumStatus = VacCheck();

                    if (vacuumStatus != EVacuumStatus.On)
                    {
                        RaiseWarning(port == EPort.Left
                            ? EWarning.LeftStage_VacuumStatus_Fail
                            : EWarning.RightStage_VacuumStatus_Fail);
                        break;
                    }

                    Log.Debug("Vacuum On Detected");
                    Step.RunStep++;
                    break;
                case EStageProcessLoadStep.End:
                    Log.Debug($"{port} Load End");
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("Sequence Plasma");
                    Sequence = port == EPort.Left ? ESequence.LeftPlasma : ESequence.RightPlasma;
                    break;
            }
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

        #region Private Methods
        private EVacuumStatus VacCheck()
        {
            bool leftVacOn = In_LeftVacuum.Value;
            bool rightVacOn = In_RightVacuum.Value;

            if (leftVacOn && rightVacOn)
            {
                return EVacuumStatus.On;
            }
            else if (!leftVacOn && !rightVacOn)
            {
                return EVacuumStatus.Off;
            }
            else
            {
                return EVacuumStatus.Fail;
            }
        }

        private void MutingLightCurtain(bool isMutingOn)
        {
            if (isMutingOn)
            {
                OutMuting1.Value = true;
                Thread.Sleep(300);
                OutMuting2.Value = true;
            }
            else
            {
                OutMuting2.Value = false;
                OutMuting1.Value = false;
            }
        }
        #endregion

        #region Private Fields
        private EPort port => Name == EProcess.StageLeft.ToString() ? EPort.Left : EPort.Right;

        private readonly Devices _devices;
        private readonly RecipeList _recipeList;
        private readonly ProcessIO _virtualIO;
        #endregion
    }
}
