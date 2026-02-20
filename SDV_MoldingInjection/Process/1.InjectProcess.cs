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
                    Log.Debug("ToRun start");
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
                    if (AllZAxisInSafetyPos())
                    {
                        Step.ToRunStep = (int)EMoldProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"Moving ZAxis safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, AllZAxisInSafetyPos);
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ZAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut);
                        if (!Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z2Axis_SafetyPos_MoveTimeOut);
                        if (!Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z3Axis_SafetyPos_MoveTimeOut);
                        else RaiseWarning(EWarning.Z4Axis_SafetyPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug($"ZAxis move safety position done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.XYAxis_SafetyPos_Move:
                    if (XAxis.IsOnPosition(_currentRecipe.MoldRecipe.XAxisReadyPos)
                        && YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisReadyPos))
                    {
                        Step.ToRunStep = (int)EMoldProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"Moving XAxis/YAxis to ready position");
                    XAxis.MoveAbs(_currentRecipe.MoldRecipe.XAxisReadyPos);
                    YAxis.MoveAbs(_currentRecipe.MoldRecipe.YAxisReadyPos);
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.XYAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.MoldRecipe.XAxisReadyPos))
                            RaiseWarning(EWarning.XAxis_ReadyPos_MoveTimeOut);
                        if (!YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisReadyPos))
                            RaiseWarning(EWarning.YAxis_ReadyPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug($"XAxis/YAxis move to ready position done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.End:
                    procOutputs.ClearOutputs();
                    Log.Debug("ToRun end");
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
                    Sequence_LoadingUnloading(isLoading : true);
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
                    Log.Debug("Searching origin Z Axes");
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

                    Log.Debug("Z Axes origin search done");
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

                    Log.Debug("Both jig detect");
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

        private void Sequence_Loading()
        {
            switch ((EMoldProcLoadingStep)Step.RunStep)
            {
                case EMoldProcLoadingStep.Start:
                    Log.Info("Loading start");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.YAxis_ReadyPos_Move:
                    if (YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisReadyPos))
                    {
                        Step.RunStep = (int)EMoldProcLoadingStep.Chamber_CoverOpen;
                        break;
                    }

                    Log.Info($"Move {YAxis.Name} to ready pos [{_currentRecipe.MoldRecipe.YAxisReadyPos}mm]");
                    YAxis.MoveAbs(_currentRecipe.MoldRecipe.YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisReadyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.YAxis_ReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.YAxis_ReadyPos_MoveTimeOut);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.Chamber_CoverOpen:
                    Log.Info($"Open {ChamberOpenClose} cylinder");
                    ChamberOpenClose.Forward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => ChamberOpenClose.IsForward);
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.Chamber_CoverOpenWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Mold_Chamber_OpenFail);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.Transfer_Load_SendRequest:
                    Log.Info($"Request transfer to load");

                    // Dryrun
                    Wait(5000);

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.Transfer_Load_Wait:
                    Log.Info($"Transfer load done");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.MCR_Read:
                    Log.Info($"MCR_Read");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.Chamber_CoverClose:
                    Log.Info($"Close {ChamberOpenClose} cylinder");
                    ChamberOpenClose.Backward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => ChamberOpenClose.IsBackward);
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.Chamber_CoverCloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Mold_Chamber_CloseFail);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("Loading end");
                    Sequence = ESequence.ResinInject;
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
                    XAxis.MoveAbs(_currentRecipe.MoldRecipe.XAxisInjectPos);
                    YAxis.MoveAbs(_currentRecipe.MoldRecipe.YAxisInjectPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(_currentRecipe.MoldRecipe.XAxisInjectPos) &&
                              YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisInjectPos));
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.XYAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.MoldRecipe.XAxisInjectPos))
                            RaiseWarning(EWarning.XAxis_InjectPos_MoveTimeOut);
                        else
                            RaiseWarning(EWarning.YAxis_InjectPos_MoveTimeOut);
                        break;
                    }

                    Log.Info($"{XAxis.Name}|{YAxis.Name} move to inject pos done");
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
                        if (!Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos))
                            RaiseWarning(EWarning.Z1Axis_InjectPos_MoveTimeOut);
                        if (!Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos))
                            RaiseWarning(EWarning.Z2Axis_InjectPos_MoveTimeOut);
                        if (!Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos))
                            RaiseWarning(EWarning.Z3Axis_InjectPos_MoveTimeOut);
                        else RaiseWarning(EWarning.Z4Axis_InjectPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug($"{BellowCyl} cylinder move up done");
                    Log.Debug($"ZAxis move to inject pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.DryPump_Vacuum_Request:
                    Log.Info($"Set Output {EInjectProcOutput.DryPump_VacuumRequest}");
                    procOutputs[EInjectProcOutput.DryPump_VacuumRequest].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.DryPump_Vacuum_DoneWait:
                    if (procInputs[EInjectProcInput.DryPump_VacuumDone].Value == false)
                    {
                        break;
                    }

                    procOutputs[EInjectProcOutput.DryPump_VacuumRequest].Value = false;
                    Log.Info($"Input detect {EInjectProcInput.DryPump_VacuumDone}");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.SDPHead_Work_Request:
                    Log.Info($"Set Output {EInjectProcOutput.SPDHeadWorkRequest}");
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

                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = false;
                    Log.Info($"Input detect EInjectProcInput.SPDHead(1~4)_WorkDone");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.DryPump_Purge_Request:
                    Log.Info($"Set Output {EInjectProcOutput.DryPump_PurgeRequest}");
                    procOutputs[EInjectProcOutput.DryPump_PurgeRequest].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.DryPump_Purge_DoneWait:
                    if (procInputs[EInjectProcInput.DryPump_PurgeDone].Value == false)
                    {
                        break;
                    }

                    procOutputs[EInjectProcOutput.DryPump_PurgeRequest].Value = false;
                    Log.Info($"Input detect {EInjectProcInput.DryPump_PurgeDone}");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxis_SafetyPos_Move:
                    ZAxisSafetyPosMove();
                    BellowCyl.Down();

                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInSafetyPos() && BellowCyl.IsDown());
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut);
                        if (!Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z2Axis_SafetyPos_MoveTimeOut);
                        if (!Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z3Axis_SafetyPos_MoveTimeOut);
                        if (!Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z4Axis_SafetyPos_MoveTimeOut);
                        else RaiseWarning(EWarning.BellowCyl_DownFail);
                        break;
                    }

                    Log.Debug($"ZAxis move to safety pos done");
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
                    if (YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisReadyPos))
                    {
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Chamber_CoverOpen;
                        break;
                    }

                    Log.Info($"Move {YAxis.Name} to ready pos [{_currentRecipe.MoldRecipe.YAxisReadyPos}mm]");
                    YAxis.MoveAbs(_currentRecipe.MoldRecipe.YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisReadyPos));
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
                    Log.Info($"Open {ChamberOpenClose} cylinder");
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
                    if (isLoading) Log.Info($"Request transfer to LOAD");
                    else Log.Info($"Request transfer to UNLOAD");

                    // Dryrun
                    Wait(3000);

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Transfer_Load_Wait:
                    if (isLoading) Log.Info($"Transfer LOAD done");
                    else Log.Info($"Transfer UNLOAD done");
                    
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.MCR_Read:
                    if (isLoading == false)
                    {
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.End;
                        break;
                    }

                    Log.Info($"MCR_Read");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverClose:
                    Log.Info($"Close {ChamberOpenClose} cylinder");
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
                    Log.Info("Unloading end");
                    Sequence = ESequence.Loading;
                    break;
            }
        }

        private void Sequence_DummyShot()
        {
            switch ((EMoldProcDummyShotStep)Step.RunStep)
            {
                case EMoldProcDummyShotStep.Start:
                    break;
                case EMoldProcDummyShotStep.End:
                    break;
            }
        }

        private void Sequence_NeedleClean()
        {
            switch ((EMoldProcNeedleCleaningStep)Step.RunStep)
            {
                case EMoldProcNeedleCleaningStep.Start:
                    break;
                case EMoldProcNeedleCleaningStep.End:
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
                    break;
                case EMoldProcDotWeightingStep.End:
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

        private bool AllZAxisInSafetyPos()
        {
            return
                Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos &&
                Z2Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos &&
                Z3Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos &&
                Z4Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos;
        }

        private void ZAxisInjectPosMove()
        {
            Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos);
            Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos);
            Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos);
            Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos);
        }

        private bool AllZAxisInInjectPos()
        {
            return
                Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos) &&
                Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos) &&
                Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos) &&
                Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos);
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
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        #endregion
    }
}
