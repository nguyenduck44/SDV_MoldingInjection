using EQX.Core.InOut;
using EQX.Core.Motion;
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
        public InjectProcess(Devices devices, ProcessIO processIO,
            RecipeSelector recipeSelector)
        {
            _devices = devices;
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
                    if (ChamberOpenClose.IsForward)
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
                case ESequence.DummyShot:
                    Sequence_DummyShot();
                    break;
                case ESequence.NeedleCleaning:
                    Sequence_NeedleClean();
                    break;
                case ESequence.DotWeighting:
                    Sequence_DotWeighting();
                    break;
                case ESequence.HeadAssemble:
                    Sequence_HeadAssemble();
                    break;
                case ESequence.HeadDisassemble:
                    Sequence_HeadDisassemble();
                    break;
                case ESequence.BubbleRemove:
                    Sequence_BubbleRemove();
                    break;
            }

            return true;
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
                    if (ChamberOpenClose.IsForward)
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

                    Log.Debug($"{XAxis.Name} {YAxis.Name} origin search done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_MoveDummyPos:
                    Log.Debug($"Move {XAxis.Name} {YAxis.Name} to dummy position");
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisDummyPos);
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisDummyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisDummyPos) &&
                              YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisDummyPos));
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_MoveDummyPosWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisDummyPos))
                            RaiseWarning(EWarning.XAxis_DummyPos_MoveTimeOut);
                        if (!YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisDummyPos))
                            RaiseWarning(EWarning.YAxis_DummyPos_MoveTimeOut);
                        break;
                    }
                    Log.Debug($"Move {XAxis.Name} {YAxis.Name} to dummy position done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.SetFlag_MoveDummyPosDone:
                    Log.Debug("Set flag move dummy pos done");
                    procOutputs[EInjectProcOutput.XYAxisMoveDummyPosFinish].Value = true;
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ClearFlag_MoveDummyPosDone:
                    if (procInputs[EInjectProcInput.SPDHead1_OriginDone].Value == false ||
                        procInputs[EInjectProcInput.SPDHead2_OriginDone].Value == false ||
                        procInputs[EInjectProcInput.SPDHead3_OriginDone].Value == false ||
                        procInputs[EInjectProcInput.SPDHead4_OriginDone].Value == false )
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("Clear flag move dummy pos done");
                    procOutputs[EInjectProcOutput.XYAxisMoveDummyPosFinish].Value = false;
                    Step.OriginStep++;
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
                        () => AllZAxisInInjectPos() && BellowCyl.IsUp());
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxisBellowCyl_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!BellowCyl.IsUp())
                            RaiseWarning(EWarning.BellowCyl_UpFail);
                        if (!_currentRecipe.SPDHead1_Recipe.HeadSkip && !Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos))
                            RaiseWarning(EWarning.Z1Axis_InjectPos_MoveTimeOut);
                        if (!_currentRecipe.SPDHead2_Recipe.HeadSkip && !Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos))
                            RaiseWarning(EWarning.Z2Axis_InjectPos_MoveTimeOut);
                        if (!_currentRecipe.SPDHead3_Recipe.HeadSkip && !Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos))
                            RaiseWarning(EWarning.Z3Axis_InjectPos_MoveTimeOut);
                        if (!_currentRecipe.SPDHead4_Recipe.HeadSkip && !Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos))
                            RaiseWarning(EWarning.Z4Axis_InjectPos_MoveTimeOut);
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
                    Sequence = ESequence.Unloading;
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

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverOpen:
                    Log.Debug($"Open {ChamberOpenClose} cylinder");
                    ChamberOpenClose.Forward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => ChamberOpenClose.IsForward);
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
                    ChamberOpenClose.Backward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => ChamberOpenClose.IsBackward);
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

                    // TODO: Modify this
                    if (_needleCleanCount >= _currentRecipe.InjectRecipe.NiddleCleanCycleCount)
                    {
                        Log.Debug($"Needle clean count [{_needleCleanCount}] exceed recipe setting [{_currentRecipe.InjectRecipe.NiddleCleanCycleCount}], sequence set to {ESequence.NeedleCleaning}");
                        Log.Info("Next sequence NeedleClean");
                        Sequence = ESequence.NeedleCleaning;
                        break;
                    }

                    break;
            }
        }

        private void Sequence_DummyShot()
        {
            switch ((EMoldProcDummyShotStep)Step.RunStep)
            {
                case EMoldProcDummyShotStep.Start:
                    Log.Info("DummyShot start");
                    Step.RunStep++;
                    break;

                case EMoldProcDummyShotStep.XYAxis_DummyPos_Move:
                    Log.Debug($"Moving XY to dummy shot position for Head 1, 3: X={_currentRecipe.InjectRecipe.XAxisDummyPos}, Y={_currentRecipe.InjectRecipe.YAxisDummyPos}");
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisDummyPos);
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisDummyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisDummyPos) &&
                        YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisDummyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.XYAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisDummyPos))
                            RaiseWarning(EWarning.XAxis_DummyPos_MoveTimeOut);
                        if (!YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisDummyPos))
                            RaiseWarning(EWarning.YAxis_DummyPos_MoveTimeOut);
                        break;
                    }
                    Log.Debug("Reached dummy shot position for Head 1, 3.");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Move:
                    Log.Debug("ZAxis move dummy position");
                    ZAxisDummyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, 
                        () => AllZAxisInDummyPos());
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Wait:
                    if(WaitTimeOutOccurred)
                    {
                        if (!_currentRecipe.SPDHead1_Recipe.HeadSkip && !Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos))
                            RaiseWarning(EWarning.Z1Axis_DummyPos_MoveTimeOut);
                        if (!_currentRecipe.SPDHead2_Recipe.HeadSkip && !Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos))
                            RaiseWarning(EWarning.Z2Axis_DummyPos_MoveTimeOut);
                        if (!_currentRecipe.SPDHead3_Recipe.HeadSkip && !Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos))
                            RaiseWarning(EWarning.Z3Axis_DummyPos_MoveTimeOut);
                        if (!_currentRecipe.SPDHead4_Recipe.HeadSkip && !Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos))
                            RaiseWarning(EWarning.Z4Axis_DummyPos_MoveTimeOut);
                        break;
                    }
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.SPDHead_DummyShot_Request:
                    Log.Debug("Sending dummy shot request");
                    procOutputs[EInjectProcOutput.SPDHead1_DummyShotRequest].Value = true;
                    procOutputs[EInjectProcOutput.SPDHead2_DummyShotRequest].Value = true;
                    procOutputs[EInjectProcOutput.SPDHead3_DummyShotRequest].Value = true;
                    procOutputs[EInjectProcOutput.SPDHead4_DummyShotRequest].Value = true;
                    Step.RunStep++;
                    break;

                case EMoldProcDummyShotStep.SPDHead_DummyShot_DoneWait:
                    if (!procInputs[EInjectProcInput.SPDHead1_DummyShotDone].Value ||
                        !procInputs[EInjectProcInput.SPDHead2_DummyShotDone].Value ||
                        !procInputs[EInjectProcInput.SPDHead3_DummyShotDone].Value ||
                        !procInputs[EInjectProcInput.SPDHead4_DummyShotDone].Value)
                    {
                        Wait(50);
                        break;
                    }
                    Log.Debug("Dummy shot done.");
                    procOutputs[EInjectProcOutput.SPDHead1_DummyShotRequest].Value = false;
                    procOutputs[EInjectProcOutput.SPDHead2_DummyShotRequest].Value = false;
                    procOutputs[EInjectProcOutput.SPDHead3_DummyShotRequest].Value = false;
                    procOutputs[EInjectProcOutput.SPDHead4_DummyShotRequest].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("DummyShot end");
                    Sequence = ESequence.NeedleCleaning;
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
                            RaiseAlarm(EAlarm.XAxis_MoveNeedleCleanPos_Timeout);
                        if (!YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisNeddleClean))
                            RaiseAlarm(EAlarm.YAxis_MoveNeedleCleanPos_Timeout);
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
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInNeedleCleanPos());
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.Z_Axis_NeedleCleanPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!_currentRecipe.SPDHead1_Recipe.HeadSkip && !Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos))
                            RaiseAlarm(EAlarm.Z1Axis_MoveNeedleCleanPos_Timeout);
                        if (!_currentRecipe.SPDHead2_Recipe.HeadSkip && !Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos))
                            RaiseAlarm(EAlarm.Z2Axis_MoveNeedleCleanPos_Timeout);
                        if (!_currentRecipe.SPDHead3_Recipe.HeadSkip && !Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos))
                            RaiseAlarm(EAlarm.Z3Axis_MoveNeedleCleanPos_Timeout);
                        if (!_currentRecipe.SPDHead4_Recipe.HeadSkip && !Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos))
                            RaiseAlarm(EAlarm.Z4Axis_MoveNeedleCleanPos_Timeout);
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

                    Sequence = ESequence.Loading;
                    break;
            }
        }

        private void Sequence_BubbleRemove()
        {
            switch ((EMoldProcBubbleRemoveStep)Step.RunStep)
            {
                case EMoldProcBubbleRemoveStep.Start:
                    break;
                case EMoldProcBubbleRemoveStep.End:
                    break;
            }
        }

        private void Sequence_HeadDisassemble()
        {
            switch ((EMoldProcHeadDisassembleStep)Step.RunStep)
            {
                case EMoldProcHeadDisassembleStep.Start:
                    break;
                case EMoldProcHeadDisassembleStep.End:
                    break;
            }
        }

        private void Sequence_HeadAssemble()
        {
            switch ((EMoldProcHeadAssembleStep)Step.RunStep)
            {
                case EMoldProcHeadAssembleStep.Start:
                    break;
                case EMoldProcHeadAssembleStep.End:
                    break;
            }
        }

        private void Sequence_DotWeighting()
        {
            switch ((EMoldProcDotWeightingStep)Step.RunStep)
            {
                case EMoldProcDotWeightingStep.Start:
                    Log.Debug("DotWeighting start");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_Up:
                    Log.Debug("Moving Z axis to safety position for DotWeighting.");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_UpWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }
                    Log.Debug("Z axis at safety position.");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.YAxis_ReadyPos_Move:
                    Log.Debug($"Moving Y axis to ready position");
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.YAxis_ReadyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos))
                            RaiseWarning(EWarning.YAxis_ReadyPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug("Y Axis move ready position done");

                    if(_currentRecipe.SPDHead1_Recipe.HeadSkip && _currentRecipe.SPDHead3_Recipe.HeadSkip)
                    {
                        Log.Debug("Head 1, 3 are skipped, skip to Head 2, 4 dot weighting");
                        Step.RunStep = (int)EMoldProcDotWeightingStep.XAxis_H24DotWeightingPos_Move;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.XAxis_H13DotWeightingPos_Move:
                    Log.Debug($"Moving Y to dot weighting position for Head 1, 3");
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisH13DotWeightingPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisH13DotWeightingPos));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.XAxis_H13DotWeightingPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisH13DotWeightingPos))
                            RaiseWarning(EWarning.XAxis_DotWeightingPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug("XAxis move dot weighting position for Head 1, 3 done");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.Head13_DotWeighting_Request:
                    Log.Debug("Sending dot weighting request for Head 1, 3.");
                    procOutputs[EInjectProcOutput.SPDHead1_DotWeightingRequest].Value = true;
                    procOutputs[EInjectProcOutput.SPDHead3_DotWeightingRequest].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.Head13_DotWeighting_DoneWait:
                    if (!procInputs[EInjectProcInput.SPDHead1_DotWeightingDone].Value || !procInputs[EInjectProcInput.SPDHead3_DotWeightingDone].Value)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("Head 1, 3 dot weighting done.");
                    procOutputs[EInjectProcOutput.SPDHead1_DotWeightingRequest].Value = false;
                    procOutputs[EInjectProcOutput.SPDHead3_DotWeightingRequest].Value = false;
                    Step.RunStep++;
                    break;

                case EMoldProcDotWeightingStep.XAxis_H24DotWeightingPos_Move:
                    if(_currentRecipe.SPDHead2_Recipe.HeadSkip && _currentRecipe.SPDHead4_Recipe.HeadSkip)
                    {
                        Log.Debug("Head 2, 4 are skipped, skip to end");
                        Step.RunStep = (int)EMoldProcDotWeightingStep.End;
                        break;
                    }

                    Log.Debug($"Moving X axis to dot weighting position for Head 2, 4");
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisH24DotWeightingPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisH24DotWeightingPos));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.XAxis_H24DotWeightingPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisH24DotWeightingPos))
                            RaiseWarning(EWarning.XAxis_DotWeightingPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug("XAxis move dot weighting position for Head 2, 4 done");
                    Step.RunStep++;
                    break;

                case EMoldProcDotWeightingStep.Head24_DotWeighting_Request:
                    Log.Debug("Sending dot weighting request for Head 2, 4.");
                    procOutputs[EInjectProcOutput.SPDHead2_DotWeightingRequest].Value = true;
                    procOutputs[EInjectProcOutput.SPDHead4_DotWeightingRequest].Value = true;
                    Step.RunStep++;
                    break;

                case EMoldProcDotWeightingStep.Head24_DotWeighting_DoneWait:
                    if (!procInputs[EInjectProcInput.SPDHead2_DotWeightingDone].Value || !procInputs[EInjectProcInput.SPDHead4_DotWeightingDone].Value)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("Head 2, 4 dot weighting done.");
                    procOutputs[EInjectProcOutput.SPDHead2_DotWeightingRequest].Value = false;
                    procOutputs[EInjectProcOutput.SPDHead4_DotWeightingRequest].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.End:
                    Log.Debug("DotWeighting end");
                    Sequence = ESequence.Stop;
                    break;
            }
        }
        #endregion

        #region Private Methods
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

        private bool AllZAxisInInjectPos()
        {
            return
                (_currentRecipe.SPDHead1_Recipe.HeadSkip || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos)) &&
                (_currentRecipe.SPDHead2_Recipe.HeadSkip || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos)) &&
                (_currentRecipe.SPDHead3_Recipe.HeadSkip || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos)) &&
                (_currentRecipe.SPDHead4_Recipe.HeadSkip || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos));
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

        private bool AllZAxisInNeedleCleanPos()
        {
            return
                (_currentRecipe.SPDHead1_Recipe.HeadSkip || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos)) &&
                (_currentRecipe.SPDHead2_Recipe.HeadSkip || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos)) &&
                (_currentRecipe.SPDHead3_Recipe.HeadSkip || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos)) &&
                (_currentRecipe.SPDHead4_Recipe.HeadSkip || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos));
        }

        private void ZAxisDummyPosMove()
        {
            if (!_currentRecipe.SPDHead1_Recipe.HeadSkip)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos);
            if (!_currentRecipe.SPDHead2_Recipe.HeadSkip)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos);
            if (!_currentRecipe.SPDHead3_Recipe.HeadSkip)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos);
            if (!_currentRecipe.SPDHead4_Recipe.HeadSkip)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos);
        }

        private bool AllZAxisInDummyPos()
        {
            return
                (_currentRecipe.SPDHead1_Recipe.HeadSkip || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos)) &&
                (_currentRecipe.SPDHead2_Recipe.HeadSkip || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos)) &&
                (_currentRecipe.SPDHead3_Recipe.HeadSkip || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos)) &&
                (_currentRecipe.SPDHead4_Recipe.HeadSkip || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos));
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

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        private int _needleCleanCount = 0;
        private int _nzlCleanCount = 0;
        private double? _yAxisCleaningTargetMm;
        private long _yAxisCleaningMoveStartTicks;
        private int _yAxisCleaningPhase; // 0=chưa bắt đầu, 1=đang chờ tiến xong, 2=đang chờ lùi xong

        private ESPDHead _failHead;
        #endregion
    }
}
