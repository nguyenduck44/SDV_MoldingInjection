using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.InOut;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Process
{
    public class InjectProcess : MIProcess
    {
        #region Properties
        /// <summary>
        /// JigLeft / JigRight status
        /// </summary>
        public EJigStatus[] JigStatuses { get; }
        #endregion

        #region Motions
        private IMotion XAxis => _devices.Motions.XAxis;
        private IMotion YAxis => _devices.Motions.StageYAxis;
        private IMotion Z1Axis => _devices.Motions.Z1Axis;
        private IMotion Z2Axis => _devices.Motions.Z2Axis;
        private IMotion Z3Axis => _devices.Motions.Z3Axis;
        private IMotion Z4Axis => _devices.Motions.Z4Axis;
        #endregion

        #region Cylinders
        private ICylinder NozzleClean_H1 => _devices.Cylinders.NozzleClean_H1;
        private ICylinder NozzleClean_H2 => _devices.Cylinders.NozzleClean_H2;
        private ICylinder NozzleClean_H3 => _devices.Cylinders.NozzleClean_H3;
        private ICylinder NozzleClean_H4 => _devices.Cylinders.NozzleClean_H4;

        private ICylinder BellowCyl => _devices.Cylinders.BellowCyl;
        private ICylinder ChamberOpenClose => _devices.Cylinders.ChamberOpenClose;
        #endregion

        #region Inputs
        private IDInput In_Jig1Detect => _devices.Inputs.Jig1Detect;
        private IDInput In_Jig2Detect => _devices.Inputs.Jig2Detect;
        private IDInput In_Jig3Detect => _devices.Inputs.Jig3Detect;
        private IDInput In_Jig4Detect => _devices.Inputs.Jig4Detect;

        private bool LeftJigTiltState => In_Jig1Detect.Value ^ In_Jig2Detect.Value;
        private bool RightJigTiltState => In_Jig3Detect.Value ^ In_Jig4Detect.Value;
        private bool LeftJigDetect => In_Jig1Detect.Value && In_Jig2Detect.Value;
        private bool RightJigDetect => In_Jig3Detect.Value && In_Jig4Detect.Value;
        #endregion

        #region Process IOs
        private IDInputDevice<EInjectProcInput> procInputs;
        private IDOutputDevice<EInjectProcOutput> procOutputs;
        #endregion

        #region Constructors
        public InjectProcess(Devices devices,
            ProcessIO processIO,
            MachineStatus machineStatus,
            RecipeSelector recipeSelector)
        {
            _devices = devices;
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;

            procInputs = processIO.InjectProcInput;
            procOutputs = processIO.InjectProcOutput;

            JigStatuses = new EJigStatus[2]
            {
                EJigStatus.None,
                EJigStatus.None
            };
        }
        #endregion

        #region Process Methods
        public override bool ProcessToRun()
        {
            switch ((EMoldProcToRunStep)Step.ToRunStep)
            {
                case EMoldProcToRunStep.Start:
                    Log.Info("ToRun start");
                    Log.Debug($"{procOutputs.Name} ClearOutputs");
                    procOutputs.ClearOutputs();
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.CheckIfChamberOpen:
                    if (ChamberOpenClose.IsOpen())
                    {
                        RaiseWarning(EWarning.Mold_Chamber_OpenWarning);
                        return false;
                    }

                    Log.Debug("Chamber is closed");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Bellow_Down:
                    if (BellowCyl.IsDown())
                    {
                        Log.Debug($"{BellowCyl} is down already");
                        Step.ToRunStep = (int)EMoldProcToRunStep.ZAxis_SafetyPos_Move;
                        break;
                    }

                    Log.Debug($"{BellowCyl} moving down");
                    BellowCyl.Down();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => BellowCyl.IsDown());
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Bellow_DownWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.BellowCyl_DownFail);
                        break;
                    }

                    Log.Debug($"Move {BellowCyl} down done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ZAxis_SafetyPos_Move:
                    if (AllZAxisInSafetyPos(ref _failHead))
                    {
                        Log.Debug($"ZAxis is on safety position already");
                        Step.ToRunStep = (int)EMoldProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"Moving Z-Axes to safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ZAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety position done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.End:
                    Log.Info("ToRun end");
                    Step.ToRunStep++;
                    base.ProcessToRun();
                    break;
            }

            return true;
        }

        public override bool ProcessRun()
        {
            switch (Sequence)
            {
                case ESequence.Stop:
                    break;
                case ESequence.Ready:
                    Sequence_Ready();
                    break;
                case ESequence.AutoRun:
                    Sequence_AutoRun();
                    break;
                case ESequence.Loading:
                    Sequence_LoadingUnloading(isLoading: true);
                    break;
                case ESequence.ResinInject:
                    Sequence_ResinInject();
                    break;
                case ESequence.Unloading:
                    Sequence_LoadingUnloading(isLoading: false);
                    break;
                case ESequence.DummyShot_H1:
                    Sequence_AtDummyPos(ESPDHead.SPDHead1);
                    break;
                case ESequence.DummyShot_H2:
                    Sequence_AtDummyPos(ESPDHead.SPDHead2);
                    break;
                case ESequence.DummyShot_H3:
                    Sequence_AtDummyPos(ESPDHead.SPDHead3);
                    break;
                case ESequence.DummyShot_H4:
                    Sequence_AtDummyPos(ESPDHead.SPDHead4);
                    break;
                case ESequence.NeedleCleaning:
                    Sequence_NeedleClean();
                    break;
                case ESequence.DotWeighting_H1:
                    Sequence_DotWeighting(ESPDHead.SPDHead1);
                    break;
                case ESequence.DotWeighting_H2:
                    Sequence_DotWeighting(ESPDHead.SPDHead2);
                    break;
                case ESequence.DotWeighting_H3:
                    Sequence_DotWeighting(ESPDHead.SPDHead3);
                    break;
                case ESequence.DotWeighting_H4:
                    Sequence_DotWeighting(ESPDHead.SPDHead4);
                    break;
                case ESequence.HeadDisassemble:
                case ESequence.HeadAssemble:
                    Sequence_AtDummyPos(ESPDHead.SPDHead1);
                    break;
                case ESequence.BubbleRemove_H1:
                    Sequence_AtDummyPos(ESPDHead.SPDHead1);
                    break;
                case ESequence.BubbleRemove_H2:
                    Sequence_AtDummyPos(ESPDHead.SPDHead2);
                    break;
                case ESequence.BubbleRemove_H3:
                    Sequence_AtDummyPos(ESPDHead.SPDHead3);
                    break;
                case ESequence.BubbleRemove_H4:
                    Sequence_AtDummyPos(ESPDHead.SPDHead4);
                    break;
                default:
                    Sequence = ESequence.Stop;
                    break;
            }

            return true;
        }

        public override bool ProcessToOrigin()
        {
            if (ProcessStatus != EProcessStatus.ToOriginDone)
            {
                procOutputs.ClearOutputs();
            }

            return base.ProcessToOrigin();
        }

        public override bool ProcessOrigin()
        {
            switch ((EMoldProcOriginStep)Step.OriginStep)
            {
                case EMoldProcOriginStep.Start:
                    Log.Debug("Origin start");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.CheckIfChamberOpen:
                    if (ChamberOpenClose.IsOpen())
                    {
                        RaiseWarning(EWarning.Mold_Chamber_OpenWarning);
                        return false;
                    }

                    Log.Debug("Chamber is closed");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_Origin:
                    Log.Debug("Searching origin for Z-Axes");
                    ZAxisSearchOrigin();

                    Wait(_currentRecipe.CommonRecipe.MotionOriginTimeout, () => AllZAxisOriginCompleted());
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_OriginWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!Z1Axis.Status.IsHomeDone) RaiseWarning(EWarning.Z1Axis_Origin_TimeOut);
                        if (!Z2Axis.Status.IsHomeDone) RaiseWarning(EWarning.Z2Axis_Origin_TimeOut);
                        if (!Z3Axis.Status.IsHomeDone) RaiseWarning(EWarning.Z3Axis_Origin_TimeOut);
                        if (!Z4Axis.Status.IsHomeDone) RaiseWarning(EWarning.Z4Axis_Origin_TimeOut);
                        break;
                    }

                    Log.Debug("Z-Axes origin search done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.Bellow_Down:
                    if (BellowCyl.IsDown())
                    {
                        Step.OriginStep = (int)EMoldProcOriginStep.XYAxis_Origin;
                        break;
                    }

                    Log.Debug($"{BellowCyl} moving down");
                    BellowCyl.Down();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => BellowCyl.IsDown());
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.Bellow_DownWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.BellowCyl_DownFail);
                        break;
                    }

                    Log.Debug($"Move {BellowCyl} down done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_Origin:
                    XAxis.SearchOrigin();
                    YAxis.SearchOrigin();

                    Log.Debug($"Searching origin {XAxis.Name} {YAxis.Name}");
                    Wait(_currentRecipe.CommonRecipe.MotionOriginTimeout,
                        () => XAxis.Status.IsHomeDone && YAxis.Status.IsHomeDone);
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_OriginWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.Status.IsHomeDone) RaiseWarning(EWarning.XAxis_Origin_TimeOut);
                        if (!YAxis.Status.IsHomeDone) RaiseWarning(EWarning.YAxis_Origin_TimeOut);
                        break;
                    }

                    currentHead = ESPDHead.SPDHead1;
                    Log.Debug($"{XAxis.Name} {YAxis.Name} origin search done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_MoveDummyPos:
                    Log.Debug($"Move {XAxis.Name} {YAxis.Name} to dummy position");
                    XAxis.MoveAbs(GetXAxisDummyPos(currentHead));
                    YAxis.MoveAbs(GetYAxisDummyPos(currentHead));
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(GetXAxisDummyPos(currentHead)) &&
                              YAxis.IsOnPosition(GetYAxisDummyPos(currentHead)));
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_MoveDummyPosWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(GetXAxisDummyPos(currentHead)))
                            RaiseWarning(EWarning.XAxis_DummyPos_MoveTimeOut);
                        if (!YAxis.IsOnPosition(GetYAxisDummyPos(currentHead)))
                            RaiseWarning(EWarning.YAxis_DummyPos_MoveTimeOut);
                        break;
                    }
                    Log.Debug($"Move {XAxis.Name} {YAxis.Name} to dummy position done {currentHead}");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.SetFlag_MoveDummyPosDone:
                    Log.Debug("Set flag move dummy pos done {currentHead}");
                    if (currentHead == ESPDHead.SPDHead1) procOutputs[EInjectProcOutput.XYAxisInDummyPosH1].Value = true;
                    if (currentHead == ESPDHead.SPDHead2) procOutputs[EInjectProcOutput.XYAxisInDummyPosH2].Value = true;
                    if (currentHead == ESPDHead.SPDHead3) procOutputs[EInjectProcOutput.XYAxisInDummyPosH3].Value = true;
                    if (currentHead == ESPDHead.SPDHead4) procOutputs[EInjectProcOutput.XYAxisInDummyPosH4].Value = true;
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ClearFlag_MoveDummyPosDone:
                    if (IsHeadOriginDone(currentHead) == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug($"Clear flag move dummy pos done {currentHead}");
                    if (currentHead == ESPDHead.SPDHead1) procOutputs[EInjectProcOutput.XYAxisInDummyPosH1].Value = false;
                    if (currentHead == ESPDHead.SPDHead2) procOutputs[EInjectProcOutput.XYAxisInDummyPosH2].Value = false;
                    if (currentHead == ESPDHead.SPDHead3) procOutputs[EInjectProcOutput.XYAxisInDummyPosH3].Value = false;
                    if (currentHead == ESPDHead.SPDHead4) procOutputs[EInjectProcOutput.XYAxisInDummyPosH4].Value = false;

                    if (currentHead == ESPDHead.SPDHead4)
                    {
                        Step.OriginStep++;
                        break;
                    }
                    currentHead++;
                    Step.OriginStep = (int)EMoldProcOriginStep.XYAxis_MoveDummyPos;
                    break;
                case EMoldProcOriginStep.End:
                    Log.Debug("Origin end");
                    Step.OriginStep++;
                    base.ProcessOrigin();
                    break;
            }

            return true;
        }
        #endregion

        #region Sequence Methods
        private void Sequence_Ready()
        {
            Sequence = ESequence.Stop;
        }

        private void Sequence_AutoRun()
        {
            switch ((EMoldProcAutoRunStep)Step.RunStep)
            {
                case EMoldProcAutoRunStep.Start:
                    Log.Debug("AutoRun start");
                    Step.RunStep++;
                    break;
                case EMoldProcAutoRunStep.JigDetect_Check:
                    if (LeftJigTiltState)
                    {
                        RaiseWarning(EWarning.Chamber_LeftJig_TiltDetect);
                        break;
                    }
                    if (RightJigTiltState)
                    {
                        RaiseWarning(EWarning.Chamber_RightJig_TiltDetect);
                        break;
                    }
                    if (!LeftJigDetect || !RightJigDetect)
                    {
                        Log.Info($"Sequence set to {ESequence.Loading}");
                        Sequence = ESequence.Loading;
                        break;
                    }

                    Log.Debug("Both jigs detected");
                    Step.RunStep++;
                    break;
                case EMoldProcAutoRunStep.JigStatus_Check:
                    if (JigStatuses[(int)EJig.JigLeft] == EJigStatus.InMolding)
                    {
                        RaiseWarning(EWarning.Chamber_LeftJig_InjectNotFinished);
                        break;
                    }
                    if (JigStatuses[(int)EJig.JigRight] == EJigStatus.InMolding)
                    {
                        RaiseWarning(EWarning.Chamber_RightJig_InjectNotFinished);
                        break;
                    }
                    if (JigStatuses.All(jig => jig == EJigStatus.MoldingFinish))
                    {
                        Log.Info($"Sequence set to {ESequence.Unloading}");
                        Sequence = ESequence.Unloading;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcAutoRunStep.Calibration_Check:
                    Log.Info($"Sequence set to {ESequence.ResinInject}");
                    Sequence = ESequence.ResinInject;
                    break;
                case EMoldProcAutoRunStep.End:
                    Log.Debug("AutoRun end");
                    break;
            }
        }

        private void Sequence_ResinInject()
        {
            switch ((EMoldProcResinInjectStep)Step.RunStep)
            {
                case EMoldProcResinInjectStep.Start:
                    Log.Info("ResinInject start");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.XYAxis_InjectPos_Move:
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisInjectPos);
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisInjectPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisInjectPos) &&
                              YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisInjectPos));
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.XYAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisInjectPos))
                            RaiseWarning(EWarning.XAxis_InjectPos_MoveTimeOut);
                        else
                            RaiseWarning(EWarning.YAxis_InjectPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug($"{XAxis.Name}|{YAxis.Name} move to inject pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxisBellowCyl_InjectPos_Move:
                    ZAxisInjectPosMove();
                    BellowCyl.Up();

                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInInjectPos(ref _failHead) && BellowCyl.IsUp());
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxisBellowCyl_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!BellowCyl.IsUp())
                            RaiseWarning(EWarning.BellowCyl_UpFail);
                        RaiseHeadWarning(EWarning.Z1Axis_InjectPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"{BellowCyl} cylinder move up done");
                    Log.Debug($"ZAxis move to inject pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.DryPump_Vacuum_Request:
                    Log.Debug($"Set Output {EInjectProcOutput.DryPump_VacuumRequest}");
                    procOutputs[EInjectProcOutput.DryPump_VacuumRequest].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.DryPump_Vacuum_DoneWait:
                    if (procInputs[EInjectProcInput.DryPump_VacuumDone].Value == false)
                    {
                        break;
                    }

                    procOutputs[EInjectProcOutput.DryPump_VacuumRequest].Value = false;
                    Log.Debug($"Input detect {EInjectProcInput.DryPump_VacuumDone}");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.SDPHead_Work_Request:
                    Log.Debug($"Set Output {EInjectProcOutput.SPDHeadWorkRequest}");

                    UpdateBothJigStatus(EJigStatus.InMolding);

                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.SDPHead_Work_DoneWait:
                    if (IsSPDHeadWorkDone(ESPDHead.SPDHead1) == false ||
                        IsSPDHeadWorkDone(ESPDHead.SPDHead2) == false ||
                        IsSPDHeadWorkDone(ESPDHead.SPDHead3) == false ||
                        IsSPDHeadWorkDone(ESPDHead.SPDHead4) == false)
                    {
                        Wait(100);
                        break;
                    }

                    UpdateBothJigStatus(EJigStatus.MoldingFinish);
                    _needleCleanCount++;

                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = false;
                    Log.Debug($"Input detect EInjectProcInput.SPDHead(1~4)_WorkDone");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.DryPump_Purge_Request:
                    Log.Debug($"Set Output {EInjectProcOutput.DryPump_PurgeRequest}");
                    procOutputs[EInjectProcOutput.DryPump_PurgeRequest].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.DryPump_Purge_DoneWait:
                    if (procInputs[EInjectProcInput.DryPump_PurgeDone].Value == false)
                    {
                        break;
                    }

                    procOutputs[EInjectProcOutput.DryPump_PurgeRequest].Value = false;
                    Log.Debug($"Input detect {EInjectProcInput.DryPump_PurgeDone}");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxis_SafetyPos_Move:
                    ZAxisSafetyPosMove();
                    BellowCyl.Down();

                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInSafetyPos(ref _failHead) && BellowCyl.IsDown());
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!BellowCyl.IsDown()) RaiseWarning(EWarning.BellowCyl_DownFail);
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("ResinInject end");
                    Sequence = ESequence.DummyShot_H1;
                    break;
            }
        }

        private void Sequence_LoadingUnloading(bool isLoading)
        {
            switch ((EMoldProcLoadingUnloadingStep)Step.RunStep)
            {
                case EMoldProcLoadingUnloadingStep.Start:
                    if (isLoading) Log.Info("Loading start");
                    else Log.Info("Unloading start");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.YAxis_ReadyPos_Move:
                    if (YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos))
                    {
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Chamber_CoverOpen;
                        break;
                    }

                    Log.Debug($"Move {YAxis.Name} to ready pos [{_currentRecipe.InjectRecipe.YAxisReadyPos}mm]");
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.YAxis_ReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.YAxis_ReadyPos_MoveTimeOut);
                        break;
                    }

                    Wait(100);

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverOpen:
                    Log.Debug($"Open {ChamberOpenClose} cylinder");
                    ChamberOpenClose.Open();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, ChamberOpenClose.IsOpen);
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverOpenWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Mold_Chamber_OpenFail);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Transfer_Load_SendRequest:
                    if (isLoading) Log.Debug($"Request transfer to LOAD");
                    else Log.Debug($"Request transfer to UNLOAD");

                    // Dryrun
                    Wait(3000);

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Transfer_Load_Wait:
                    if (isLoading) Log.Debug($"Transfer LOAD done");
                    else Log.Debug($"Transfer UNLOAD done");

                    UpdateBothJigStatus(EJigStatus.None);

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.MCR_Read:
                    if (isLoading == false)
                    {
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.End;
                        break;
                    }

                    Log.Debug($"MCR_Read");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverClose:
                    Log.Debug($"Close {ChamberOpenClose} cylinder");
                    ChamberOpenClose.Close();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => ChamberOpenClose.IsClose());
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverCloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Mold_Chamber_CloseFail);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    if (isLoading)
                    {
                        Log.Info("Loading end");
                        Sequence = ESequence.ResinInject;
                        break;
                    }

                    Log.Info("Unloading end");
                    Sequence = ESequence.Loading;

                    break;
            }
        }

        private void Sequence_AtDummyPos(ESPDHead head)
        {
            switch ((EMoldProcDummyShotStep)Step.RunStep)
            {
                case EMoldProcDummyShotStep.Start:
                    Log.Info($"DummyShot for {head} start");
                    Step.RunStep++;
                    break;

                case EMoldProcDummyShotStep.XYAxis_DummyPos_Move:
                    Log.Debug($"Moving XY to dummy shot position for {head}: X={GetXAxisDummyPos(head)}, Y={GetYAxisDummyPos(head)}");
                    XAxis.MoveAbs(GetXAxisDummyPos(head));
                    YAxis.MoveAbs(GetYAxisDummyPos(head));
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(GetXAxisDummyPos(head)) &&
                        YAxis.IsOnPosition(GetYAxisDummyPos(head)));
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.XYAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(GetXAxisDummyPos(head)))
                            RaiseWarning(EWarning.XAxis_DummyPos_MoveTimeOut);
                        if (!YAxis.IsOnPosition(GetYAxisDummyPos(head)))
                            RaiseWarning(EWarning.YAxis_DummyPos_MoveTimeOut);
                        break;
                    }
                    Log.Debug($"Reached dummy shot position for {head}");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Move:
                    Log.Debug("ZAxis move dummy position");
                    ZAxisDummyPosMove(head);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => ZAxisInDummyPos(head));
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_DummyPos_MoveTimeOut, head);
                        break;
                    }
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.SPDHead_InjectResin_Request:
                    Log.Debug($"Sending inject resin request for {head}");
                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = true;
                    Step.RunStep++;
                    break;

                case EMoldProcDummyShotStep.SPDHead_InjectResin_DoneWait:
                    if (IsSPDHeadWorkDone(head) == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug($"Inject Resin done for {head}.");
                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_SafetyPos_Move:
                    if (Sequence == ESequence.HeadAssemble || Sequence == ESequence.HeadDisassemble)
                    {
                        Step.RunStep = (int)EMoldProcDummyShotStep.End;
                        break;
                    }

                    Log.Debug($"Moving Z-Axes to safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_SafetyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety position done");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.End:
                    Log.Info($"{Sequence} for {head} end");
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead4)
                    {
                        Sequence = ESequence.NeedleCleaning;
                        break;
                    }

                    Sequence = (ESequence)((int)Sequence + 1);
                    break;
            }
        }

        private void Sequence_NeedleClean()
        {
            switch ((EMoldProcNeedleCleaningStep)Step.RunStep)
            {
                case EMoldProcNeedleCleaningStep.Start:
                    Log.Info("NeedleClean start");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.CleanCount_Check:
                    if (_nzlCleanCount < 0 || _nzlCleanCount >= ((130 / _currentRecipe.InjectRecipe.NiddleCleanShiftDist) - 1))
                    {
                        Log.Debug("Reset nozzle clean count");
                        _nzlCleanCount = 0;
                    }
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.XY_Axis_NeedleCleanPos_Move:
                    Log.Debug($"XY axis move to needle clean position");
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisNeddleClean);
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisNeddleClean + _nzlCleanCount * _currentRecipe.InjectRecipe.NiddleCleanShiftDist);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisNeddleClean) &&
                        YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisNeddleClean + _nzlCleanCount * _currentRecipe.InjectRecipe.NiddleCleanShiftDist));
                    Step.RunStep++;
                    break;

                case EMoldProcNeedleCleaningStep.XY_Axis_NeedleCleanPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisNeddleClean))
                            RaiseWarning(EWarning.XAxis_MoveNeedleCleanPos_Timeout);
                        if (!YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisNeddleClean))
                            RaiseWarning(EWarning.YAxis_MoveNeedleCleanPos_Timeout);
                        break;
                    }
                    Log.Debug("XY Axis move needle clean done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGrip:
                    Log.Debug("Nozzle clean cylinder Ungrip");
                    NozzleCleanCyl_UnGrip();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => NozzleCleanCyl_UnGrip_Check());
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGripWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Nozzle_CleanCyl_UnGripFail);
                        break;
                    }
                    Log.Debug("Nozzle clean cylinder Ungrip done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.Z_Axis_NeedleCleanPos_Move:
                    Log.Debug("Z axis move to needle clean pos");
                    ZAxisNeedleCleanPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInNeedleCleanPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.Z_Axis_NeedleCleanPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_MoveNeedleCleanPos_Timeout, _failHead);
                        break;
                    }

                    Log.Debug("Z axis move to needle clean pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_Grip:
                    Log.Debug("Nozzle clean cylinder Grip");
                    NozzleCleanCyl_Grip();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => NozzleCleanCyl_Grip_Check());
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_GripWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Nozzle_CleanCyl_GripFail);
                        break;
                    }
                    Log.Debug("Nozzle clean cylinder Grip done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.Z_Axis_Up_AfterClean:
                    Log.Debug("Z axis move up after clean");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.Z_Axis_Up_AfterCleanWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }
                    Log.Debug("Z axis up after clean done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.End:
                    Log.Info("NeedleClean end");
                    _nzlCleanCount++;
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Sequence = ESequence.Unloading;
                    break;
            }
        }

        private void Sequence_DotWeighting(ESPDHead head)
        {
            switch ((EMoldProcDotWeightingStep)Step.RunStep)
            {
                case EMoldProcDotWeightingStep.Start:
                    Log.Debug("DotWeighting start");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.YAxis_ReadyPos_Move:
                    if (YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos))
                    {
                        Step.RunStep = (int)EMoldProcDotWeightingStep.ZAxis_SafetyPos_Move;
                        break;
                    }
                    Log.Debug($"Move {YAxis.Name} to ready pos [{_currentRecipe.InjectRecipe.YAxisReadyPos}mm]");
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.YAxis_ReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.YAxis_ReadyPos_MoveTimeOut);
                        break;
                    }
                    Log.Debug($"{YAxis.Name} move to ready pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_SafetyPos_Move:
                    Log.Debug($"Moving Z-Axes to safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_SafetyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety position done");
                    Log.Debug("Wait SPDHead request");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.WaitSPDHeadRequest:
                    if (procInputs[EInjectProcInput.SPDHead1_RequestDotWaiting].Value)
                    {
                        Step.RunStep = (int)EMoldProcDotWeightingStep.XAxis_DotWeightingPos_Move;
                        break;
                    }

                    if (procInputs[EInjectProcInput.SPDHead2_RequestDotWaiting].Value)
                    {
                        Step.RunStep = (int)EMoldProcDotWeightingStep.XAxis_DotWeightingPos_Move;
                        break;
                    }


                    if (procInputs[EInjectProcInput.SPDHead3_RequestDotWaiting].Value)
                    {
                        Step.RunStep = (int)EMoldProcDotWeightingStep.XAxis_DotWeightingPos_Move;
                        break;
                    }

                    if (procInputs[EInjectProcInput.SPDHead4_RequestDotWaiting].Value)
                    {
                        Step.RunStep = (int)EMoldProcDotWeightingStep.XAxis_DotWeightingPos_Move;
                        break;
                    }

                    if (_machineStatus.MachineCalibration.All(x => x))
                    {
                        Step.RunStep = (int)EMoldProcDotWeightingStep.End;
                        break;
                    }

                    break;
                case EMoldProcDotWeightingStep.XAxis_DotWeightingPos_Move:
                    Log.Debug($"Moving X to dot weighting position for Head ");

                    XAxis.MoveAbs(GetXAxisDotWeightingPos(head));
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(GetXAxisDotWeightingPos(head)));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.XAxis_DotWeightingPos_Move_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.XAxis_DotWeightingPos_MoveTimeOut);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_DotWeightingPos_Move:
                    Log.Debug($"Moving Z to dot weighting position for Head ");
                    ZAxisDotWeightingPosMove(head);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => ZAxisInWeightingPos(head));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_DotWeightingPos_Move_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_MoveDotWeightingPos_Timeout, head);
                        break;
                    }
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.DotWeighting_Request:
                    if (head == ESPDHead.SPDHead1)
                    {
                        Log.Debug("Sending dot weighting request for Head 1.");
                        procOutputs[EInjectProcOutput.H1DotWeightingInPos].Value = true;
                        Step.RunStep++;
                        break;
                    }
                    if (head == ESPDHead.SPDHead2)
                    {
                        Log.Debug("Sending dot weighting request for Head 2.");
                        procOutputs[EInjectProcOutput.H2DotWeightingInPos].Value = true;
                        Step.RunStep++;
                        break;
                    }
                    if (head == ESPDHead.SPDHead3)
                    {
                        Log.Debug("Sending dot weighting request for Head 3.");
                        procOutputs[EInjectProcOutput.H3DotWeightingInPos].Value = true;
                        Step.RunStep++;
                        break;
                    }

                    Log.Debug("Sending dot weighting request for Head 4.");
                    procOutputs[EInjectProcOutput.H4DotWeightingInPos].Value = true;
                    Step.RunStep++;
                    break;

                case EMoldProcDotWeightingStep.ClearFlag_DotWeighting_Request:
                    if (head == ESPDHead.SPDHead1 && procInputs[EInjectProcInput.SPDHead1_RequestDotWaiting].Value == true)
                    {
                        Wait(20);
                        break;
                    }
                    if (head == ESPDHead.SPDHead2 && procInputs[EInjectProcInput.SPDHead2_RequestDotWaiting].Value == true)
                    {
                        Wait(20);
                        break;
                    }
                    if (head == ESPDHead.SPDHead3 && procInputs[EInjectProcInput.SPDHead3_RequestDotWaiting].Value == true)
                    {
                        Wait(20);
                        break;
                    }
                    if (head == ESPDHead.SPDHead4 && procInputs[EInjectProcInput.SPDHead4_RequestDotWaiting].Value == true)
                    {
                        Wait(20);
                        break;
                    }

                    if (head == ESPDHead.SPDHead1)
                    {
                        procOutputs[EInjectProcOutput.H1DotWeightingInPos].Value = false;
                        Step.RunStep++;
                        break;
                    }
                    if (head == ESPDHead.SPDHead2)
                    {
                        procOutputs[EInjectProcOutput.H2DotWeightingInPos].Value = false;
                        Step.RunStep++;
                        break;
                    }
                    if (head == ESPDHead.SPDHead3)
                    {
                        procOutputs[EInjectProcOutput.H3DotWeightingInPos].Value = false;
                        Step.RunStep++;
                        break;
                    }

                    procOutputs[EInjectProcOutput.H4DotWeightingInPos].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_SafetyPos_Move_End:
                    Log.Debug($"Moving Z-Axes to safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_SafetyPos_Move_End_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety position done");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.End:
                    Log.Debug("DotWeighting end");
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Sequence = ESequence.AutoRun;
                    break;
            }
        }
        #endregion

        #region Private Methods
        private IMotion GetZAxis(ESPDHead head) => head switch
        {
            ESPDHead.SPDHead1 => Z1Axis,
            ESPDHead.SPDHead2 => Z2Axis,
            ESPDHead.SPDHead3 => Z3Axis,
            ESPDHead.SPDHead4 => Z4Axis,
            _ => throw new ArgumentOutOfRangeException(nameof(head)),
        };

        private double GetZAxisDummyPos(ESPDHead head) => head switch
        {
            ESPDHead.SPDHead1 => _currentRecipe.SPDHead1_Recipe.ZAxisDummyPos,
            ESPDHead.SPDHead2 => _currentRecipe.SPDHead2_Recipe.ZAxisDummyPos,
            ESPDHead.SPDHead3 => _currentRecipe.SPDHead3_Recipe.ZAxisDummyPos,
            ESPDHead.SPDHead4 => _currentRecipe.SPDHead4_Recipe.ZAxisDummyPos,
            _ => throw new ArgumentOutOfRangeException(nameof(head)),
        };

        private void ZAxisSearchOrigin()
        {
            Z1Axis.SearchOrigin();
            Z2Axis.SearchOrigin();
            Z3Axis.SearchOrigin();
            Z4Axis.SearchOrigin();
        }

        private bool AllZAxisOriginCompleted()
        {
            return Z1Axis.Status.IsHomeDone
                && Z2Axis.Status.IsHomeDone
                && Z3Axis.Status.IsHomeDone
                && Z4Axis.Status.IsHomeDone;
        }

        private void ZAxisSafetyPosMove()
        {
            Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos);
            Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos);
            Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos);
            Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos);
        }

        private bool AllZAxisInSafetyPos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos;
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = Z2Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos;
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = Z3Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos;
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = Z4Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos;
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisInjectPosMove()
        {
            if (!_currentRecipe.SPDHead1_Recipe.HeadSkip)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos);
            if (!_currentRecipe.SPDHead2_Recipe.HeadSkip)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos);
            if (!_currentRecipe.SPDHead3_Recipe.HeadSkip)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos);
            if (!_currentRecipe.SPDHead4_Recipe.HeadSkip)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos);
        }

        private bool AllZAxisInInjectPos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = _currentRecipe.SPDHead1_Recipe.HeadSkip || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = _currentRecipe.SPDHead2_Recipe.HeadSkip || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = _currentRecipe.SPDHead3_Recipe.HeadSkip || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = _currentRecipe.SPDHead4_Recipe.HeadSkip || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisNeedleCleanPosMove()
        {
            if (!_currentRecipe.SPDHead1_Recipe.HeadSkip)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos);
            if (!_currentRecipe.SPDHead2_Recipe.HeadSkip)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos);
            if (!_currentRecipe.SPDHead3_Recipe.HeadSkip)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos);
            if (!_currentRecipe.SPDHead4_Recipe.HeadSkip)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos);
        }

        private bool AllZAxisInNeedleCleanPos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = _currentRecipe.SPDHead1_Recipe.HeadSkip || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = _currentRecipe.SPDHead2_Recipe.HeadSkip || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = _currentRecipe.SPDHead3_Recipe.HeadSkip || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = _currentRecipe.SPDHead4_Recipe.HeadSkip || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisDummyPosMove(ESPDHead head)
        {
            if (_currentRecipe.SPDHead1_Recipe.HeadSkip == false && head == ESPDHead.SPDHead1)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos);
            if (_currentRecipe.SPDHead2_Recipe.HeadSkip == false && head == ESPDHead.SPDHead2)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos);
            if (_currentRecipe.SPDHead3_Recipe.HeadSkip == false && head == ESPDHead.SPDHead3)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos);
            if (_currentRecipe.SPDHead4_Recipe.HeadSkip == false && head == ESPDHead.SPDHead4)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos);
        }
        private bool ZAxisInDummyPos(ESPDHead head)
        {
            if (head == ESPDHead.SPDHead1)
            {
                return _currentRecipe.SPDHead1_Recipe.HeadSkip || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos);
            }

            if (head == ESPDHead.SPDHead2)
            {
                return _currentRecipe.SPDHead2_Recipe.HeadSkip || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos);
            }

            if (head == ESPDHead.SPDHead3)
            {
                return _currentRecipe.SPDHead3_Recipe.HeadSkip || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos);
            }

            if (head == ESPDHead.SPDHead4)
            {
                return _currentRecipe.SPDHead4_Recipe.HeadSkip || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos);
            }

            return true;
        }

        private void ZAxisDotWeightingPosMove(ESPDHead head)
        {
            if (_currentRecipe.SPDHead1_Recipe.HeadSkip == false && head == ESPDHead.SPDHead1)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos);
            if (_currentRecipe.SPDHead2_Recipe.HeadSkip == false && head == ESPDHead.SPDHead2)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos);
            if (_currentRecipe.SPDHead3_Recipe.HeadSkip == false && head == ESPDHead.SPDHead3)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos);
            if (_currentRecipe.SPDHead4_Recipe.HeadSkip == false && head == ESPDHead.SPDHead4)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos);
        }

        private bool ZAxisInWeightingPos(ESPDHead head)
        {
            if (head == ESPDHead.SPDHead1)
            {
                return _currentRecipe.SPDHead1_Recipe.HeadSkip || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos);

            }
            if (head == ESPDHead.SPDHead2)
            {
                return _currentRecipe.SPDHead2_Recipe.HeadSkip || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos);

            }
            if (head == ESPDHead.SPDHead3)
            {
                return _currentRecipe.SPDHead3_Recipe.HeadSkip || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos);

            }
            if (head == ESPDHead.SPDHead4)
            {
                return _currentRecipe.SPDHead4_Recipe.HeadSkip || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos);

            }

            return true;
        }

        private void NozzleCleanCyl_Grip()
        {
            if (!_currentRecipe.SPDHead1_Recipe.HeadSkip)
                NozzleClean_H1.Grip();
            if (!_currentRecipe.SPDHead2_Recipe.HeadSkip)
                NozzleClean_H2.Grip();
            if (!_currentRecipe.SPDHead3_Recipe.HeadSkip)
                NozzleClean_H3.Grip();
            if (!_currentRecipe.SPDHead4_Recipe.HeadSkip)
                NozzleClean_H4.Grip();
        }

        private bool NozzleCleanCyl_Grip_Check()
        {
            return
                (_currentRecipe.SPDHead1_Recipe.HeadSkip || NozzleClean_H1.IsGrip()) &&
                (_currentRecipe.SPDHead2_Recipe.HeadSkip || NozzleClean_H2.IsGrip()) &&
                (_currentRecipe.SPDHead3_Recipe.HeadSkip || NozzleClean_H3.IsGrip()) &&
                (_currentRecipe.SPDHead4_Recipe.HeadSkip || NozzleClean_H4.IsGrip());
        }

        private void NozzleCleanCyl_UnGrip()
        {
            if (!_currentRecipe.SPDHead1_Recipe.HeadSkip)
                NozzleClean_H1.Ungrip();
            if (!_currentRecipe.SPDHead2_Recipe.HeadSkip)
                NozzleClean_H2.Ungrip();
            if (!_currentRecipe.SPDHead3_Recipe.HeadSkip)
                NozzleClean_H3.Ungrip();
            if (!_currentRecipe.SPDHead4_Recipe.HeadSkip)
                NozzleClean_H4.Ungrip();
        }

        private bool NozzleCleanCyl_UnGrip_Check()
        {
            return
                (_currentRecipe.SPDHead1_Recipe.HeadSkip || NozzleClean_H1.IsUngrip()) &&
                (_currentRecipe.SPDHead2_Recipe.HeadSkip || NozzleClean_H2.IsUngrip()) &&
                (_currentRecipe.SPDHead3_Recipe.HeadSkip || NozzleClean_H3.IsUngrip()) &&
                (_currentRecipe.SPDHead4_Recipe.HeadSkip || NozzleClean_H4.IsUngrip());
        }

        private bool IsSPDHeadWorkDone(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => procInputs[EInjectProcInput.SPDHead1_WorkDone].Value || _currentRecipe.SPDHead1_Recipe.HeadSkip,
                ESPDHead.SPDHead2 => procInputs[EInjectProcInput.SPDHead2_WorkDone].Value || _currentRecipe.SPDHead2_Recipe.HeadSkip,
                ESPDHead.SPDHead3 => procInputs[EInjectProcInput.SPDHead3_WorkDone].Value || _currentRecipe.SPDHead3_Recipe.HeadSkip,
                ESPDHead.SPDHead4 => procInputs[EInjectProcInput.SPDHead4_WorkDone].Value || _currentRecipe.SPDHead4_Recipe.HeadSkip,
                _ => throw new Exception($"Invalid head: {head}")
            };
        }

        private void UpdateBothJigStatus(EJigStatus status)
        {
            JigStatuses[(int)EJig.JigLeft] = status;
            JigStatuses[(int)EJig.JigRight] = status;
        }

        private void RaiseHeadWarning(EWarning warning, ESPDHead _failHead)
        {
            RaiseWarning(warning + ((int)(EWarning.Z2Axis_Origin_TimeOut - EWarning.Z1Axis_Origin_TimeOut)) * (_failHead - ESPDHead.SPDHead1));
        }
        #endregion

        #region Privates Properties
        private bool IsHeadOriginDone(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => procInputs[EInjectProcInput.SPDHead1_OriginDone].Value,
                ESPDHead.SPDHead2 => procInputs[EInjectProcInput.SPDHead2_OriginDone].Value,
                ESPDHead.SPDHead3 => procInputs[EInjectProcInput.SPDHead3_OriginDone].Value,
                ESPDHead.SPDHead4 => procInputs[EInjectProcInput.SPDHead4_OriginDone].Value,
                _ => throw new Exception($"{head} is not valid"),
            };
        }

        double GetXAxisDummyPos(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => _currentRecipe.SPDHead1_Recipe.XAxisDummyPos,
                ESPDHead.SPDHead2 => _currentRecipe.SPDHead2_Recipe.XAxisDummyPos,
                ESPDHead.SPDHead3 => _currentRecipe.SPDHead3_Recipe.XAxisDummyPos,
                ESPDHead.SPDHead4 => _currentRecipe.SPDHead4_Recipe.XAxisDummyPos,
                _ => throw new Exception($"{head} is not valid"),
            };
        }

        double GetXAxisDotWeightingPos(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => _currentRecipe.SPDHead1_Recipe.XAxisDotWeightingPos,
                ESPDHead.SPDHead2 => _currentRecipe.SPDHead2_Recipe.XAxisDotWeightingPos,
                ESPDHead.SPDHead3 => _currentRecipe.SPDHead3_Recipe.XAxisDotWeightingPos,
                ESPDHead.SPDHead4 => _currentRecipe.SPDHead4_Recipe.XAxisDotWeightingPos,
                _ => throw new Exception($"{head} is not valid"),
            };
        }

        double GetYAxisDummyPos(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => _currentRecipe.SPDHead1_Recipe.YAxisDummyPos,
                ESPDHead.SPDHead2 => _currentRecipe.SPDHead2_Recipe.YAxisDummyPos,
                ESPDHead.SPDHead3 => _currentRecipe.SPDHead3_Recipe.YAxisDummyPos,
                ESPDHead.SPDHead4 => _currentRecipe.SPDHead4_Recipe.YAxisDummyPos,
                _ => throw new Exception($"{head} is not valid"),
            };
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        private int _needleCleanCount = 0;
        private int _nzlCleanCount = 0;
        private double? _yAxisCleaningTargetMm;
        private long _yAxisCleaningMoveStartTicks;
        private int _yAxisCleaningPhase; // 0=chưa bắt đầu, 1=đang chờ tiến xong, 2=đang chờ lùi xong

        private ESPDHead currentHead;
        private ESPDHead _failHead;
        #endregion
    }
}
