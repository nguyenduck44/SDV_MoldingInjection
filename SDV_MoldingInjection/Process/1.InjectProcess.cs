using EQX.Core.Communication.CIM;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.InOut;
using EQX.UI.Controls;
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

        #region Outputs
        private bool Out_ChamberOpen
        {
            get => _devices.Outputs.VacChamberOpen.Value;
            set => _devices.Outputs.VacChamberOpen.Value = value;
        }
        private bool Out_ChamberClose
        {
            get => _devices.Outputs.VacChamberClose.Value;
            set => _devices.Outputs.VacChamberClose.Value = value;
        }
        #endregion

        #region Process IOs
        private IDInputDevice<EInjectProcInput> procInputs;
        private IDOutputDevice<EInjectProcOutput> procOutputs;
        #endregion

        #region Constructors
        public InjectProcess(Devices devices,
            ProcessIO processIO,
            MachineStatus machineStatus,
            RecipeSelector recipeSelector,
            TactTimeList tactTimeList,
            ICIMMapHelper mapHelper)
        {
            _devices = devices;
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;
            _tactTimeList = tactTimeList;
            _mapHelper = mapHelper;
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
        public override bool ProcessToAlarm()
        {
            DisableChamberOpenCloes();
            return base.ProcessToAlarm();
        }

        public override bool ProcessToStop()
        {
            DisableChamberOpenCloes();
            return base.ProcessToStop();
        }

        public override bool ProcessToWarning()
        {
            DisableChamberOpenCloes();
            return base.ProcessToWarning();
        }

        public override bool ProcessToRun()
        {
            switch ((EMoldProcToRunStep)Step.ToRunStep)
            {
                case EMoldProcToRunStep.Start:
                    Log.Info("ToRun start");
                    Log.Debug($"{procOutputs.Name} ClearOutputs");
                    procOutputs.ClearOutputs();

                    if (Parent!.Sequence != ESequence.AutoRun)
                    {
                        Step.ToRunStep++;
                        break;
                    }

                    Log.Debug("Machine Calibration check");
                    _machineStatus.MachineCalibration[0] |= _currentRecipe.OptionRecipe.SkipHead12 || _machineStatus.MachineCalibrationSkip[0];
                    _machineStatus.MachineCalibration[1] |= _currentRecipe.OptionRecipe.SkipHead12 || _machineStatus.MachineCalibrationSkip[1];
                    _machineStatus.MachineCalibration[2] |= _currentRecipe.OptionRecipe.SkipHead34 || _machineStatus.MachineCalibrationSkip[2];
                    _machineStatus.MachineCalibration[3] |= _currentRecipe.OptionRecipe.SkipHead34 || _machineStatus.MachineCalibrationSkip[3];

                    if (_machineStatus.MachineCalibration.All(x => x) == false &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        RaiseWarning(EWarning.Machine_Need_Calibration);
                        break;
                    }

                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ChamberClose:
                    if (ChamberOpenClose.IsClose())
                    {
                        Log.Debug("Chamber is closed");
                        Step.ToRunStep = (int)EMoldProcToRunStep.Bellow_Down;
                        break;
                    }

                    Log.Debug($"{ChamberOpenClose} moving close");
                    ChamberOpenClose.Close();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => ChamberOpenClose.IsClose());
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ChamberClose_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Mold_Chamber_CloseFail);
                        break;
                    }

                    Log.Debug($"Move {ChamberOpenClose} close done");
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
                    Sequence_AtDummyPos(ESequence.DummyShot, ESPDHead.All);
                    break;
                case ESequence.DummyShot_H1:
                    Sequence_AtDummyPos(ESequence.DummyShot_H1, ESPDHead.SPDHead1);
                    break;
                case ESequence.DummyShot_H2:
                    Sequence_AtDummyPos(ESequence.DummyShot_H2, ESPDHead.SPDHead2);
                    break;
                case ESequence.DummyShot_H3:
                    Sequence_AtDummyPos(ESequence.DummyShot_H3, ESPDHead.SPDHead3);
                    break;
                case ESequence.DummyShot_H4:
                    Sequence_AtDummyPos(ESequence.DummyShot_H4, ESPDHead.SPDHead4);
                    break;
                case ESequence.NeedleCleaning:
                    Sequence_NeedleClean(ESequence.NeedleCleaning, ESPDHead.All);
                    break;
                case ESequence.NeedleCleaning_H1:
                    Sequence_NeedleClean(ESequence.NeedleCleaning_H1, ESPDHead.SPDHead1);
                    break;
                case ESequence.NeedleCleaning_H2:
                    Sequence_NeedleClean(ESequence.NeedleCleaning_H2, ESPDHead.SPDHead2);
                    break;
                case ESequence.NeedleCleaning_H3:
                    Sequence_NeedleClean(ESequence.NeedleCleaning_H3, ESPDHead.SPDHead3);
                    break;
                case ESequence.NeedleCleaning_H4:
                    Sequence_NeedleClean(ESequence.NeedleCleaning_H4, ESPDHead.SPDHead4);
                    break;
                case ESequence.DotWeighting:
                    Sequence_DotWeighting(ESPDHead.All);
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
                case ESequence.HeadAssemble_H1:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_H1, ESPDHead.SPDHead1);
                    break;
                case ESequence.HeadAssemble_H2:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_H2, ESPDHead.SPDHead2);
                    break;
                case ESequence.HeadAssemble_H3:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_H3, ESPDHead.SPDHead3);
                    break;
                case ESequence.HeadAssemble_H4:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_H4, ESPDHead.SPDHead4);
                    break;
                case ESequence.HeadDisassemble_H1:
                    Sequence_AtDummyPos(ESequence.HeadDisassemble_H1, ESPDHead.SPDHead1);
                    break;
                case ESequence.HeadDisassemble_H2:
                    Sequence_AtDummyPos(ESequence.HeadDisassemble_H2, ESPDHead.SPDHead2);
                    break;
                case ESequence.HeadDisassemble_H3:
                    Sequence_AtDummyPos(ESequence.HeadDisassemble_H3, ESPDHead.SPDHead3);
                    break;
                case ESequence.HeadDisassemble_H4:
                    Sequence_AtDummyPos(ESequence.HeadDisassemble_H4, ESPDHead.SPDHead4);
                    break;
                case ESequence.BubbleRemove:
                    Sequence_AtDummyPos(ESequence.BubbleRemove, ESPDHead.All);
                    break;
                case ESequence.BubbleRemove_H1:
                    Sequence_AtDummyPos(ESequence.BubbleRemove_H1, ESPDHead.SPDHead1);
                    break;
                case ESequence.BubbleRemove_H2:
                    Sequence_AtDummyPos(ESequence.BubbleRemove_H2, ESPDHead.SPDHead2);
                    break;
                case ESequence.BubbleRemove_H3:
                    Sequence_AtDummyPos(ESequence.BubbleRemove_H3, ESPDHead.SPDHead3);
                    break;
                case ESequence.BubbleRemove_H4:
                    Sequence_AtDummyPos(ESequence.BubbleRemove_H4, ESPDHead.SPDHead4);
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
                    XAxis.MoveAbs(Recipe.XAxisDummyPos);
                    YAxis.MoveAbs(Recipe.YAxisDummyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(Recipe.XAxisDummyPos) &&
                              YAxis.IsOnPosition(Recipe.YAxisDummyPos));
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_MoveDummyPosWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(Recipe.XAxisDummyPos))
                            RaiseWarning(EWarning.XAxis_DummyPos_MoveTimeOut);
                        if (!YAxis.IsOnPosition(Recipe.YAxisDummyPos))
                            RaiseWarning(EWarning.YAxis_DummyPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug($"Move {XAxis.Name} {YAxis.Name} to dummy position X: {Recipe.XAxisDummyPos}, Y: {Recipe.YAxisDummyPos} done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_DummyPos_Move:
                    Log.Debug("ZAxis move dummy position");
                    Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos);
                    Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos);
                    Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos);
                    Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos) &&
                                                                              Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos) &&
                                                                              Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos) &&
                                                                              Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos));
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos))
                        {
                            RaiseWarning(EWarning.Z1Axis_DummyPos_MoveTimeOut);
                        }
                        if (!Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos))
                        {
                            RaiseWarning(EWarning.Z2Axis_DummyPos_MoveTimeOut);
                        }
                        if (!Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos))
                        {
                            RaiseWarning(EWarning.Z3Axis_DummyPos_MoveTimeOut);
                        }
                        if (!Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos))
                        {
                            RaiseWarning(EWarning.Z4Axis_DummyPos_MoveTimeOut);
                        }

                        break;
                    }

                    Log.Debug("ZAxis move dummy position done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.SetFlag_MoveDummyPosDone:
                    procOutputs[EInjectProcOutput.XYAxisInDummyPos].Value = true;
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ClearFlag_MoveDummyPosDone:
                    if (procInputs[EInjectProcInput.SPDHead1_OriginDone].Value == false ||
                        procInputs[EInjectProcInput.SPDHead2_OriginDone].Value == false ||
                        procInputs[EInjectProcInput.SPDHead3_OriginDone].Value == false ||
                        procInputs[EInjectProcInput.SPDHead4_OriginDone].Value == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug($"Clear flag move dummy pos done");
                    procOutputs[EInjectProcOutput.XYAxisInDummyPos].Value = false;
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_SafetyPos_Move:
                    Log.Debug("Moving Z-Axes to safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_SafetyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug("Z-Axes move to safety position done");
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
                    if ((!LeftJigDetect && _optionRecipe.SkipHead12 == false) ||
                        (!RightJigDetect && _optionRecipe.SkipHead34 == false))
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
                    if (Parent?.Sequence == ESequence.AutoRun)
                    {
                        _tactTimeList.Inject.TaktTimeCounter = Environment.TickCount;
                    }

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
                        IsSPDHeadWorkDone(ESPDHead.SPDHead4) == false ||
                        procInputs[EInjectProcInput.VentComplete].Value == false)
                    {
                        Wait(100);
                        break;
                    }

                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = false;
                    Log.Debug($"Input detect EInjectProcInput.SPDHead(1~4)_WorkDone");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.InjectAddTail_Check:
                    Log.Debug("Inject Add Tail Check");
                    if (_currentRecipe.AdditionalMolding_Recipe.SkipAddTail)
                    {
                        Step.RunStep = (int)EMoldProcResinInjectStep.Update_BothJigStatus;
                        break;
                    }

                    Log.Debug("Wait SPDHead Ready Inject Add Tail");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.Wait_SPDHeadReady_InjectAddTail:
                    if (IsSPDHeadInjectAddTailReady(ESPDHead.SPDHead1) == false ||
                        IsSPDHeadInjectAddTailReady(ESPDHead.SPDHead2) == false ||
                        IsSPDHeadInjectAddTailReady(ESPDHead.SPDHead3) == false ||
                        IsSPDHeadInjectAddTailReady(ESPDHead.SPDHead4) == false)
                    {
                        Wait(20);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.SPDHead_InjectAddTail_Request:
                    Log.Debug($"Set Output {EInjectProcOutput.SPDHead_InjectAddTail_Request}");
                    procOutputs[EInjectProcOutput.SPDHead_InjectAddTail_Request].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxis_UpDistance_Move:
                    Log.Debug("ZAxis move up distance for add tail");
                    ZAxisAddTailPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInAddTailPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxis_UpDistance_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_Up_AddDetailPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug("ZAxis move up distance for add tail done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.SDPHead_AddTail_DoneWait:
                    if (IsSPDHeadInjectAddTailDone(ESPDHead.SPDHead1) == false ||
                        IsSPDHeadInjectAddTailDone(ESPDHead.SPDHead2) == false ||
                        IsSPDHeadInjectAddTailDone(ESPDHead.SPDHead3) == false ||
                        IsSPDHeadInjectAddTailDone(ESPDHead.SPDHead4) == false)
                    {
                        Wait(20);
                        break;
                    }

                    procOutputs[EInjectProcOutput.SPDHead_InjectAddTail_Request].Value = false;
                    Log.Debug("SPDHead Inject AddTail done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.Update_BothJigStatus:
                    Log.Debug($"Update both jig status: {EJigStatus.MoldingFinish}");
                    UpdateBothJigStatus(EJigStatus.MoldingFinish);
                    if(_currentRecipe.OptionRecipe.SkipVentTime == false)
                    {
                        Step.RunStep = (int)EMoldProcResinInjectStep.BellowCylDown;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.Request_DryPump_Purge:
                    Log.Debug("Set flag request dry pump purge");
                    procOutputs[EInjectProcOutput.DryPump_PurgeRequest].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.Wait_DryPump_PurgeEnd:
                    if (procInputs[EInjectProcInput.DryPump_PurgeDone].Value == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug("Detected flag DryPump_PurgeDone");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.BellowCylDown:
                    Log.Debug("Bellow cylinder downing");
                    BellowCyl.Down();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => BellowCyl.IsDown());
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.BellowCylDown_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.BellowCyl_DownFail);
                        break;
                    }

                    Log.Debug("Bellow cylinder down finish");
                    Wait(1000); // Delay 1s after bellow cylinder down
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxis_SafetyPos_Move:
                    Log.Debug("All Z Axis moving to Safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ZAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.ClearFlag:
                    Log.Debug("Clear flag"); 
                    procOutputs[EInjectProcOutput.DryPump_VacuumRequest].Value = false;
                    procOutputs[EInjectProcOutput.DryPump_PurgeRequest].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.End:
                    _needleCleanCount++;

                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("ResinInject end");
                    _tactTimeList.Inject.SetTaktTime();
                    Sequence = ESequence.DummyShot;
                    break;

            }
        }

        private void Sequence_LoadingUnloading(bool isLoading)
        {
            switch ((EMoldProcLoadingUnloadingStep)Step.RunStep)
            {
                case EMoldProcLoadingUnloadingStep.Start:
                    if (isLoading)
                    {
                        Log.Info("Loading start");
                        _tactTimeList.Loading.TaktTimeCounter = Environment.TickCount;
                    }
                    else
                    {
                        Log.Info("Unloading start");
                        _tactTimeList.Unloading.TaktTimeCounter = Environment.TickCount;
                    }
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

                    Log.Debug($"{ChamberOpenClose} Open finish");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Transfer_Load_SendRequest:
                    if (_machineStatus.IsDryRunMode)
                    {
                        Wait(3000);
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Chamber_CoverClose;
                        break;
                    }

                    if (_optionRecipe.InputTypeManual)
                    {
                        if (isLoading && _machineStatus.ConfirmLoadingFinish)
                        {
                            Log.Debug("Loading manual done");
                            _machineStatus.ConfirmLoadingFinish = false;
                            Step.RunStep = Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Jig_Check;
                            break;
                        }
                        if (isLoading == false)
                        {
                            Step.RunStep = Step.RunStep = (int)EMoldProcLoadingUnloadingStep.End;
                            break;
                        }
                    }

                    if (_optionRecipe.InputTypeAuto)
                    {
                        if (isLoading) Log.Debug($"Request transfer to LOAD");
                        else Log.Debug($"Request transfer to UNLOAD");
                        //TODO : Send request to transfer

                        Step.RunStep++;
                        break;
                    }

                    break;
                case EMoldProcLoadingUnloadingStep.Transfer_Load_Wait:
                    if (isLoading)
                    {
                        //TODO: 
                    }
                    else
                    {

                    }

                    if (isLoading) Log.Debug($"Transfer LOAD done");
                    else Log.Debug($"Transfer UNLOAD done");

                    UpdateBothJigStatus(EJigStatus.None);

                    Step.RunStep++;
                    break;

                case EMoldProcLoadingUnloadingStep.Jig_Check:
                    Log.Debug("Jig check");
                    if (isLoading == false && _machineStatus.DisableDetectJig == false && _machineStatus.IsDryRunMode == false)
                    {
#if SIMULATION
                        SimulationInputSetter.SetSimInput(In_Jig1Detect, false);
                        SimulationInputSetter.SetSimInput(In_Jig2Detect, false);
                        SimulationInputSetter.SetSimInput(In_Jig3Detect, false);
                        SimulationInputSetter.SetSimInput(In_Jig4Detect, false);
#endif
                        if ((_optionRecipe.SkipHead12 == false && (In_Jig1Detect.Value || In_Jig2Detect.Value)) ||
                            (_optionRecipe.SkipHead34 == false && (In_Jig3Detect.Value || In_Jig4Detect.Value)))
                        {
                            RaiseWarning(EWarning.Jig_Detected_Unload_Fail);
                            break;
                        }
                    }

                    if (isLoading && _machineStatus.DisableDetectJig == false && _machineStatus.IsDryRunMode == false)
                    {
#if SIMULATION
                        SimulationInputSetter.SetSimInput(In_Jig1Detect, true);
                        SimulationInputSetter.SetSimInput(In_Jig2Detect, true);
                        SimulationInputSetter.SetSimInput(In_Jig3Detect, true);
                        SimulationInputSetter.SetSimInput(In_Jig4Detect, true);
#endif
                        if (_optionRecipe.SkipHead12 == false && _machineStatus.DisableDetectJig == false && _machineStatus.IsDryRunMode == false)
                        {
                            if (LeftJigTiltState)
                            {
                                RaiseWarning(EWarning.Left_Jig_Tilt_State);
                                break;
                            }
                            if (LeftJigDetect == false)
                            {
                                RaiseWarning(EWarning.Left_Jig_Not_Detect);
                                break;
                            }
                        }

                        if (_optionRecipe.SkipHead34 == false && _machineStatus.DisableDetectJig == false && _machineStatus.IsDryRunMode == false)
                        {
                            if (RightJigTiltState)
                            {
                                RaiseWarning(EWarning.Right_Jig_Tilt_State);
                                break;
                            }
                            if (RightJigDetect == false)
                            {
                                RaiseWarning(EWarning.Right_Jig_Not_Detect);
                                break;
                            }
                        }
                    }

                    if (isLoading == false)
                    {
                        UpdateBothJigStatus(EJigStatus.None);
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.End;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.MCR_Read:
                    if (isLoading)
                    {
                        //TODO : MCR read for loading
                        Log.Debug($"MCR_Read");

                    }

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
                        _tactTimeList.Loading.SetTaktTime();
                        Sequence = ESequence.ResinInject;
                        break;
                    }

                    Log.Info("Unloading end");
                    _tactTimeList.Unloading.SetTaktTime();
                    Sequence = ESequence.Loading;

                    break;
            }
        }

        private void Sequence_AtDummyPos(ESequence sequence, ESPDHead head)
        {
            switch ((EMoldProcDummyShotStep)Step.RunStep)
            {
                case EMoldProcDummyShotStep.Start:
                    if ((((head == ESPDHead.SPDHead1 || head == ESPDHead.SPDHead2) && _currentRecipe.OptionRecipe.SkipHead12) ||
                       ((head == ESPDHead.SPDHead3 || head == ESPDHead.SPDHead4) && _currentRecipe.OptionRecipe.SkipHead34)) &&
                       sequence != ESequence.HeadAssemble_H1 &&
                       sequence != ESequence.HeadAssemble_H2 &&
                       sequence != ESequence.HeadAssemble_H3 &&
                       sequence != ESequence.HeadAssemble_H4 &&
                       sequence != ESequence.HeadDisassemble_H1 &&
                       sequence != ESequence.HeadDisassemble_H2 &&
                       sequence != ESequence.HeadDisassemble_H3 &&
                       sequence != ESequence.HeadDisassemble_H4)

                    {
                        Log.Debug("SKIP dummy shot for head " + head);
                        Step.RunStep = (int)EMoldProcDummyShotStep.End;
                        break;
                    }

                    if (sequence == ESequence.DummyShot && Parent?.Sequence == ESequence.AutoRun)
                    {
                        _tactTimeList.DummyShot.TaktTimeCounter = Environment.TickCount;
                    }

                    Log.Info($"Dummy position for {head} start");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.XYAxis_DummyPos_Move:
                    Log.Debug($"Moving XY to dummy shot position for {head}: X={Recipe.XAxisDummyPos}, Y={Recipe.YAxisDummyPos}");
                    XAxis.MoveAbs(Recipe.XAxisDummyPos);
                    YAxis.MoveAbs(Recipe.YAxisDummyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(Recipe.XAxisDummyPos) &&
                        YAxis.IsOnPosition(Recipe.YAxisDummyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.XYAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(Recipe.XAxisDummyPos))
                            RaiseWarning(EWarning.XAxis_DummyPos_MoveTimeOut);
                        if (!YAxis.IsOnPosition(Recipe.YAxisDummyPos))
                            RaiseWarning(EWarning.YAxis_DummyPos_MoveTimeOut);
                        break;
                    }
                    Log.Debug($"Reached dummy shot position for {sequence}");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Move:
                    if (sequence == ESequence.HeadAssemble_H1 || sequence == ESequence.HeadAssemble_H2 ||
                        sequence == ESequence.HeadAssemble_H3 || sequence == ESequence.HeadAssemble_H4 ||
                        sequence == ESequence.HeadDisassemble_H1 || sequence == ESequence.HeadDisassemble_H2 ||
                        sequence == ESequence.HeadDisassemble_H3 || sequence == ESequence.HeadDisassemble_H4)
                    {
                        Log.Debug("ZAxis move assemble/disassemble position");
                        ZAxisAssembleDisassemblePosMove(head);
                        Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => ZAxisInAssembleDisassemblePos(head));
                    }
                    else
                    {
                        Log.Debug("ZAxis move dummy position");
                        ZAxisDummyPosMove(head);
                        Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => ZAxisInDummyPos(head));
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (sequence == ESequence.DummyShot || sequence == ESequence.BubbleRemove)
                        {
                            if (Parent!.Sequence == ESequence.AutoRun)
                            {
                                if (_currentRecipe.OptionRecipe.SkipHead12 == false && !Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.Z1Axis_DummyPos_MoveTimeOut);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead12 == false && !Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.Z2Axis_DummyPos_MoveTimeOut);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead34 == false && !Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.Z3Axis_DummyPos_MoveTimeOut);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead34 == false && !Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.Z4Axis_DummyPos_MoveTimeOut);
                                }
                            }
                            else
                            {
                                if (_machineStatus.IsSkipHead1 == false && !Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.Z1Axis_DummyPos_MoveTimeOut);
                                }
                                if (_machineStatus.IsSkipHead2 == false == false && !Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.Z2Axis_DummyPos_MoveTimeOut);
                                }
                                if (_machineStatus.IsSkipHead3 == false == false && !Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.Z3Axis_DummyPos_MoveTimeOut);
                                }
                                if (_machineStatus.IsSkipHead4 == false == false && !Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.Z4Axis_DummyPos_MoveTimeOut);
                                }
                            }
                        }

                        if (sequence == ESequence.BubbleRemove_H1 ||
                            sequence == ESequence.BubbleRemove_H2 ||
                            sequence == ESequence.BubbleRemove_H3 ||
                            sequence == ESequence.BubbleRemove_H4)
                        {
                            RaiseHeadWarning(EWarning.Z1Axis_MoveBubbleRemovePos_Timeout, head);
                        }
                        else if (sequence == ESequence.HeadAssemble_H1 || sequence == ESequence.HeadAssemble_H2 ||
                                 sequence == ESequence.HeadAssemble_H3 || sequence == ESequence.HeadAssemble_H4 ||
                                 sequence == ESequence.HeadDisassemble_H1 || sequence == ESequence.HeadDisassemble_H2 ||
                                 sequence == ESequence.HeadDisassemble_H3 || sequence == ESequence.HeadDisassemble_H4)
                        {
                            RaiseHeadWarning(EWarning.Z1Axis_MoveAssembleDisassemblePos_Timeout, head);
                        }
                        else
                        {
                            RaiseHeadWarning(EWarning.Z1Axis_DummyPos_MoveTimeOut, head);
                        }


                        break;
                    }
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.SPDHead_InjectResin_Request:
                    Log.Debug($"Sending inject resin request for {head}");
                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = true;
                    Step.RunStep++;
                    break;

                case EMoldProcDummyShotStep.SPDHead_InjectResin_Done_And_VentComplete_Wait:
                    if (sequence == ESequence.HeadAssemble_H1 || sequence == ESequence.HeadAssemble_H2 ||
                        sequence == ESequence.HeadAssemble_H3 || sequence == ESequence.HeadAssemble_H4 ||
                        sequence == ESequence.HeadDisassemble_H1 || sequence == ESequence.HeadDisassemble_H2 ||
                        sequence == ESequence.HeadDisassemble_H3 || sequence == ESequence.HeadDisassemble_H4)
                    {
                        if (IsSPDHeadWorkDoneForAssemble(head) == false)
                        {
                            Wait(50);
                            break;
                        }
                    }
                    else
                    {
                        if (IsSPDHeadWorkDone(head) == false)
                        {
                            Wait(50);
                            break;
                        }
                    }

                    Log.Debug($"Inject Resin done.");
                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_SafetyPos_Move:
                    if (sequence == ESequence.HeadAssemble_H1 || sequence == ESequence.HeadAssemble_H2 ||
                        sequence == ESequence.HeadAssemble_H3 || sequence == ESequence.HeadAssemble_H4 ||
                        sequence == ESequence.HeadDisassemble_H1 || sequence == ESequence.HeadDisassemble_H2 ||
                        sequence == ESequence.HeadDisassemble_H3 || sequence == ESequence.HeadDisassemble_H4)
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
                        MessageBoxEx.Show($"{sequence} Finish!", false);
                        Sequence = ESequence.Stop;
                    }

                    if (sequence == ESequence.DummyShot && Parent!.Sequence == ESequence.AutoRun)
                    {
                        Log.Info("Set next sequence to needdle clean");
                        _tactTimeList.DummyShot.SetTaktTime();
                        Sequence = ESequence.NeedleCleaning;
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
            }
        }

        private void Sequence_NeedleClean(ESequence sequence, ESPDHead head)
        {
            switch ((EMoldProcNeedleCleaningStep)Step.RunStep)
            {
                case EMoldProcNeedleCleaningStep.Start:
                    if (head == ESPDHead.SPDHead1 && _currentRecipe.OptionRecipe.SkipHead12 ||
                        head == ESPDHead.SPDHead2 && _currentRecipe.OptionRecipe.SkipHead12 ||
                        head == ESPDHead.SPDHead3 && _currentRecipe.OptionRecipe.SkipHead34 ||
                        head == ESPDHead.SPDHead4 && _currentRecipe.OptionRecipe.SkipHead34)
                    {
                        Step.RunStep = (int)EMoldProcNeedleCleaningStep.End;
                        break;
                    }

                    Log.Info("NeedleClean start");

                    if (Parent?.Sequence == ESequence.AutoRun)
                    {
                        _tactTimeList.NeedleClean.TaktTimeCounter = Environment.TickCount;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.YAxis_CleanPos_Calculator:
                    _yAxisCleaningTargetMm = Recipe.YAxisNeddleClean - _nzlCleanCount * Recipe.NiddleCleanShiftDist;
                    if (_yAxisCleaningTargetMm < -19 || //Limit sensor
                       (Recipe.YAxisNeddleClean - _yAxisCleaningTargetMm) > 130) //Distance clean
                    {
                        Log.Debug("Reset nozzle clean count");
                        _nzlCleanCount = 0;
                        _yAxisCleaningTargetMm = Recipe.YAxisNeddleClean;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.XY_Axis_NeedleCleanPos_Move:
                    Log.Debug($"XY axis move to needle clean position X: {Recipe.XAxisNeedleCleanPos}, Y: {_yAxisCleaningTargetMm}");
                    XAxis.MoveAbs(Recipe.XAxisNeedleCleanPos);
                    YAxis.MoveAbs(_yAxisCleaningTargetMm);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(Recipe.XAxisNeedleCleanPos) &&
                        YAxis.IsOnPosition(_yAxisCleaningTargetMm));
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.XY_Axis_NeedleCleanPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(Recipe.XAxisNeedleCleanPos))
                            RaiseWarning(EWarning.XAxis_MoveNeedleCleanPos_Timeout);
                        if (!YAxis.IsOnPosition(Recipe.YAxisNeddleClean))
                            RaiseWarning(EWarning.YAxis_MoveNeedleCleanPos_Timeout);
                        break;
                    }

                    Log.Debug("XY Axis move needle clean done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGrip:
                    Log.Debug("Nozzle clean cylinder Ungrip");
                    NozzleCleanCyl_UnGrip(head);
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => NozzleCleanCyl_UnGrip_Check(head));
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
                    ZAxisNeedleCleanPosMove(head);
                    if (sequence == ESequence.NeedleCleaning)
                    {
                        Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInNeedleCleanPos(ref _failHead, ESPDHead.All));
                    }
                    else
                    {
                        Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => ZAxisInNeedleCleanPos(head));
                    }

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
                    NozzleCleanCyl_Grip(head);
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => NozzleCleanCyl_Grip_Check(head));
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
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGrip_AfterClean:
                    Log.Debug("Nozzle clean cylinder Ungrip");
                    NozzleCleanCyl_UnGrip(head);
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => NozzleCleanCyl_UnGrip_Check(head));
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGrip_AfterClean_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.Nozzle_CleanCyl_UnGripFail);
                        break;
                    }
                    Log.Debug("Nozzle clean cylinder Ungrip done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.End:
                    Log.Info("NeedleClean end");
                    _nzlCleanCount++;
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        MessageBoxEx.Show($"{head} Needle Clean Finish!", false);
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("Set next sequence to unloading");
                    _tactTimeList.NeedleClean.SetTaktTime();
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
                case EMoldProcDotWeightingStep.SPDHead_WorkCheck:
                    Log.Debug($"Set current head dot weighting for {head}");
                    if (head == ESPDHead.SPDHead1 || head == ESPDHead.SPDHead3)
                    {
                        currentDotWeightingHead = EDotWeightingHead.HEAD13;
                        Step.RunStep = (int)EMoldProcDotWeightingStep.XYAxis_DotWeightingReadyPos_Move;
                        break;
                    }

                    if (head == ESPDHead.SPDHead2 || head == ESPDHead.SPDHead4)
                    {
                        currentDotWeightingHead = EDotWeightingHead.HEAD24;
                        Step.RunStep = (int)EMoldProcDotWeightingStep.XYAxis_DotWeightingReadyPos_Move;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.SPDHead_DotWeighting_H13_Check:
                    Log.Debug("DotWeighting check head work");
                    if (_machineStatus.IsSkipHead1 && _machineStatus.IsSkipHead3)
                    {
                        Step.RunStep++;
                        break;
                    }

                    currentDotWeightingHead = EDotWeightingHead.HEAD13;
                    Step.RunStep = (int)EMoldProcDotWeightingStep.XYAxis_DotWeightingReadyPos_Move;
                    break;
                case EMoldProcDotWeightingStep.SPDHead_DotWeighting_H24_Check:
                    if (_machineStatus.IsSkipHead2 && _machineStatus.IsSkipHead4)
                    {
                        Step.RunStep = (int)EMoldProcDotWeightingStep.End;
                        break;
                    }

                    currentDotWeightingHead = EDotWeightingHead.HEAD24;
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.XYAxis_DotWeightingReadyPos_Move:
                    Log.Debug($"Move {YAxis.Name} to ready pos [{_currentRecipe.InjectRecipe.YAxisReadyPos}mm]");
                    XAxisMoveDotWeightingPosition();
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos) &&
                        IsXAxisInDotWeightingPosition());
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.XYAxis_DotWeightingReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos) == false)
                        {
                            RaiseWarning(EWarning.YAxis_ReadyPos_MoveTimeOut);
                        }
                        else
                        {
                            if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
                            {
                                RaiseWarning(EWarning.XAxis_H13DotWeightingPos_MoveTimeOut);
                                break;
                            }
                            RaiseWarning(EWarning.XAxis_H24DotWeightingPos_MoveTimeOut);
                        }
                        break;
                    }
                    Log.Debug("StageY Axis move to ready pos done");
                    Log.Debug("XAxis move to dot weighting pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_DotWeightingPos_Move:
                    Log.Debug($"Moving Z to dot weighting position for {head}");
                    ZAxisMoveDotWeightingPosition(head);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => IsZAxisInDotWeightingPosition(head));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_DotWeightingPos_Move_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (head == ESPDHead.All)
                        {
                            if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
                            {
                                if (Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos) == false && _machineStatus.IsSkipHead1)
                                {
                                    RaiseWarning(EWarning.Z1Axis_MoveDotWeightingPos_Timeout);
                                    break;
                                }
                                if (Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos) == false && _machineStatus.IsSkipHead3)
                                {
                                    RaiseWarning(EWarning.Z3Axis_MoveDotWeightingPos_Timeout);
                                    break;
                                }
                            }

                            if (currentDotWeightingHead == EDotWeightingHead.HEAD24)
                            {
                                if (Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos) == false && _machineStatus.IsSkipHead2)
                                {
                                    RaiseWarning(EWarning.Z2Axis_MoveDotWeightingPos_Timeout);
                                    break;
                                }
                                if (Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos) == false && _machineStatus.IsSkipHead4)
                                {
                                    RaiseWarning(EWarning.Z4Axis_MoveDotWeightingPos_Timeout);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            RaiseHeadWarning(EWarning.Z1Axis_MoveDotWeightingPos_Timeout, head);
                            break;
                        }
                    }

                    Log.Debug("Z Axis Move Dot Weighting Position Done");
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.DotWeighting_Request:
                    if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
                    {
                        procOutputs[EInjectProcOutput.Request_H13DotWeighting].Value = true;
                    }
                    else
                    {
                        procOutputs[EInjectProcOutput.Request_H24DotWeighting].Value = true;
                    }
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ClearFlag_DotWeighting_Request:
                    if (head == ESPDHead.All)
                    {
                        if (currentDotWeightingHead == EDotWeightingHead.HEAD13 &&
                        ((procInputs[EInjectProcInput.SPDHead1_DotWeightingDone].Value == false && _machineStatus.IsSkipHead1 == false) ||
                         (procInputs[EInjectProcInput.SPDHead3_DotWeightingDone].Value == false && _machineStatus.IsSkipHead3 == false)))
                        {
                            Wait(50);
                            break;
                        }

                        if (currentDotWeightingHead == EDotWeightingHead.HEAD24 &&
                        ((procInputs[EInjectProcInput.SPDHead2_DotWeightingDone].Value == false && _machineStatus.IsSkipHead2 == false) ||
                         (procInputs[EInjectProcInput.SPDHead4_DotWeightingDone].Value == false && _machineStatus.IsSkipHead4 == false)))
                        {
                            Wait(50);
                            break;
                        }
                    }
                    else
                    {
                        if (IsHeadDotWeightingDone(head) == false)
                        {
                            Wait(50);
                            break;
                        }
                    }

                    procOutputs[EInjectProcOutput.Request_H13DotWeighting].Value = false;
                    procOutputs[EInjectProcOutput.Request_H24DotWeighting].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_SafetyPos_Move:
                    Log.Debug($"Moving Z-Axes to safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.ZAxis_SafetyPos__Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety position done");
                    if (currentDotWeightingHead == EDotWeightingHead.HEAD13 && head == ESPDHead.All)
                    {
                        Step.RunStep = (int)EMoldProcDotWeightingStep.SPDHead_DotWeighting_H24_Check;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.End:
                    Log.Debug("DotWeighting end");
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        MessageBoxEx.Show($"Dot Weighting Finish!", false);
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
            if (!_currentRecipe.OptionRecipe.SkipHead12)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos);
            if (!_currentRecipe.OptionRecipe.SkipHead12)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos);
            if (!_currentRecipe.OptionRecipe.SkipHead34)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos);
            if (!_currentRecipe.OptionRecipe.SkipHead34)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos);
        }

        private bool AllZAxisInInjectPos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisAddTailPosMove()
        {
            double ZAxis_Vel = _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail / _currentRecipe.AdditionalMolding_Recipe.AddTailTime;

            if (!_currentRecipe.OptionRecipe.SkipHead12)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail, ZAxis_Vel);
            if (!_currentRecipe.OptionRecipe.SkipHead12)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail, ZAxis_Vel);
            if (!_currentRecipe.OptionRecipe.SkipHead34)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail, ZAxis_Vel);
            if (!_currentRecipe.OptionRecipe.SkipHead34)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail, ZAxis_Vel);
        }

        private bool AllZAxisInAddTailPos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisNeedleCleanPosMove(ESPDHead head)
        {
            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All)
            {
                if (_machineStatus.IsSkipHead1 == false)
                    Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos);
                if (_machineStatus.IsSkipHead2 == false)
                    Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos);
                if (_machineStatus.IsSkipHead3 == false)
                    Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos);
                if (_machineStatus.IsSkipHead4 == false)
                    Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos);
            }
            else
            {
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos);
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos);
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos);
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
                    Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos);
            }
        }

        private bool ZAxisInNeedleCleanPos(ESPDHead head)
        {
            if (head == ESPDHead.SPDHead1)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos);
            }
            if (head == ESPDHead.SPDHead2)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos);
            }
            if (head == ESPDHead.SPDHead3)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos);
            }
            if (head == ESPDHead.SPDHead4)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos);
            }
            return true;
        }

        private bool AllZAxisInNeedleCleanPos(ref ESPDHead failHead, ESPDHead head)
        {
            bool result = true;
            bool ret = false;

            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All)
            {
                ret = _machineStatus.IsSkipHead1 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead1;

                ret = _machineStatus.IsSkipHead2 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead2;

                ret = _machineStatus.IsSkipHead3 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead3;

                ret = _machineStatus.IsSkipHead4 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead4;
            }
            else
            {
                ret = _currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead1;

                ret = _currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead2;

                ret = _currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead3;

                ret = _currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead4;
            }

            return result;

        }

        private void ZAxisDummyPosMove(ESPDHead head)
        {
            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All)
            {
                if (_machineStatus.IsSkipHead1 == false)
                    Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos);
                if (_machineStatus.IsSkipHead2 == false)
                    Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos);
                if (_machineStatus.IsSkipHead3 == false)
                    Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos);
                if (_machineStatus.IsSkipHead4 == false)
                    Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos);
            }
            else
            {
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos);
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos);
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos);
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
                    Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos);
            }
        }

        private bool ZAxisInDummyPos(ESPDHead head)
        {
            if (head == ESPDHead.All && Parent!.Sequence == ESequence.AutoRun)
            {
                return
                    (_currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos)) &&
                    (_currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos)) &&
                    (_currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos)) &&
                    (_currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos));
            }

            if (head == ESPDHead.All && Parent!.Sequence != ESequence.AutoRun)
            {
                return
                    (_machineStatus.IsSkipHead1 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos)) &&
                    (_machineStatus.IsSkipHead2 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos)) &&
                    (_machineStatus.IsSkipHead3 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos)) &&
                    (_machineStatus.IsSkipHead4 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos));
            }

            if (head == ESPDHead.SPDHead1)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos);
            }

            if (head == ESPDHead.SPDHead2)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos);
            }

            if (head == ESPDHead.SPDHead3)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos);
            }

            if (head == ESPDHead.SPDHead4)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos);
            }

            return true;
        }

        private void ZAxisAssembleDisassemblePosMove(ESPDHead head)
        {
            if (_currentRecipe.OptionRecipe.SkipHead12 == false && head == ESPDHead.SPDHead1)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePos);
            if (_currentRecipe.OptionRecipe.SkipHead12 == false && head == ESPDHead.SPDHead2)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePos);
            if (_currentRecipe.OptionRecipe.SkipHead34 == false && head == ESPDHead.SPDHead3)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePos);
            if (_currentRecipe.OptionRecipe.SkipHead34 == false && head == ESPDHead.SPDHead4)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePos);
        }

        private bool ZAxisInAssembleDisassemblePos(ESPDHead head)
        {
            if (head == ESPDHead.SPDHead1)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead2)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead3)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead4)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePos);
            }

            return true;
        }

        private void NozzleCleanCyl_Grip(ESPDHead head)
        {
            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All)
            {
                if (_machineStatus.IsSkipHead1 == false)
                    NozzleClean_H1.Grip();
                if (_machineStatus.IsSkipHead2 == false)
                    NozzleClean_H2.Grip();
                if (_machineStatus.IsSkipHead3 == false)
                    NozzleClean_H3.Grip();
                if (_machineStatus.IsSkipHead4 == false)
                    NozzleClean_H4.Grip();
            }
            else
            {
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    NozzleClean_H1.Grip();
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    NozzleClean_H2.Grip();
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    NozzleClean_H3.Grip();
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
                    NozzleClean_H4.Grip();
            }
        }

        private bool NozzleCleanCyl_Grip_Check(ESPDHead head)
        {
            if (head == ESPDHead.All)
            {
                if (Parent!.Sequence != ESequence.AutoRun)
                {
                    return (_machineStatus.IsSkipHead1 || NozzleClean_H1.IsGrip()) &&
                        (_machineStatus.IsSkipHead2 || NozzleClean_H2.IsGrip()) &&
                        (_machineStatus.IsSkipHead3 || NozzleClean_H3.IsGrip()) &&
                        (_machineStatus.IsSkipHead4 || NozzleClean_H4.IsGrip());
                }
                else
                {
                    return (_currentRecipe.OptionRecipe.SkipHead12 || NozzleClean_H1.IsGrip()) &&
                        (_currentRecipe.OptionRecipe.SkipHead12 || NozzleClean_H2.IsGrip()) &&
                        (_currentRecipe.OptionRecipe.SkipHead34 || NozzleClean_H3.IsGrip()) &&
                        (_currentRecipe.OptionRecipe.SkipHead34 || NozzleClean_H4.IsGrip());
                }
            }
            if (head == ESPDHead.SPDHead1)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || NozzleClean_H1.IsGrip();
            }
            if (head == ESPDHead.SPDHead2)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || NozzleClean_H2.IsGrip();
            }
            if (head == ESPDHead.SPDHead3)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || NozzleClean_H3.IsGrip();
            }
            if (head == ESPDHead.SPDHead4)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || NozzleClean_H4.IsGrip();
            }

            return true;
        }

        private void NozzleCleanCyl_UnGrip(ESPDHead head)
        {
            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All)
            {
                if (_machineStatus.IsSkipHead1 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    NozzleClean_H1.Ungrip();
                if (_machineStatus.IsSkipHead2 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    NozzleClean_H2.Ungrip();
                if (_machineStatus.IsSkipHead3 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    NozzleClean_H3.Ungrip();
                if (_machineStatus.IsSkipHead4 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
                    NozzleClean_H4.Ungrip();
            }
            else
            {
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    NozzleClean_H1.Ungrip();
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    NozzleClean_H2.Ungrip();
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    NozzleClean_H3.Ungrip();
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
                    NozzleClean_H4.Ungrip();
            }
        }

        private bool NozzleCleanCyl_UnGrip_Check(ESPDHead head)
        {
            if (head == ESPDHead.All)
            {
                if (Parent!.Sequence != ESequence.AutoRun)
                {
                    return (_machineStatus.IsSkipHead1 || NozzleClean_H1.IsUngrip()) &&
                        (_machineStatus.IsSkipHead2 || NozzleClean_H2.IsUngrip()) &&
                        (_machineStatus.IsSkipHead3 || NozzleClean_H3.IsUngrip()) &&
                        (_machineStatus.IsSkipHead4 || NozzleClean_H4.IsUngrip());
                }

                return (_currentRecipe.OptionRecipe.SkipHead12 || NozzleClean_H1.IsUngrip()) &&
                    (_currentRecipe.OptionRecipe.SkipHead12 || NozzleClean_H2.IsUngrip()) &&
                    (_currentRecipe.OptionRecipe.SkipHead34 || NozzleClean_H3.IsUngrip()) &&
                    (_currentRecipe.OptionRecipe.SkipHead34 || NozzleClean_H4.IsUngrip());
            }

            if (head == ESPDHead.SPDHead1) return _currentRecipe.OptionRecipe.SkipHead12 || NozzleClean_H1.IsUngrip();
            if (head == ESPDHead.SPDHead2) return _currentRecipe.OptionRecipe.SkipHead12 || NozzleClean_H2.IsUngrip();
            if (head == ESPDHead.SPDHead3) return _currentRecipe.OptionRecipe.SkipHead34 || NozzleClean_H3.IsUngrip();
            return _currentRecipe.OptionRecipe.SkipHead34 || NozzleClean_H4.IsUngrip();
        }

        private bool IsSPDHeadWorkDone(ESPDHead head)
        {
            if (head == ESPDHead.All)
            {
                if (Parent!.Sequence == ESequence.AutoRun)
                {
                    return
                    (procInputs[EInjectProcInput.SPDHead1_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead12) &&
                    (procInputs[EInjectProcInput.SPDHead2_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead12) &&
                    (procInputs[EInjectProcInput.SPDHead3_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead34) &&
                    (procInputs[EInjectProcInput.SPDHead4_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead34);
                }
                else
                {
                    return
                    (procInputs[EInjectProcInput.SPDHead1_WorkDone].Value || _machineStatus.IsSkipHead1) &&
                    (procInputs[EInjectProcInput.SPDHead2_WorkDone].Value || _machineStatus.IsSkipHead2) &&
                    (procInputs[EInjectProcInput.SPDHead3_WorkDone].Value || _machineStatus.IsSkipHead3) &&
                    (procInputs[EInjectProcInput.SPDHead4_WorkDone].Value || _machineStatus.IsSkipHead4);
                }
            }

            return head switch
            {
                ESPDHead.SPDHead1 => procInputs[EInjectProcInput.SPDHead1_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead12,
                ESPDHead.SPDHead2 => procInputs[EInjectProcInput.SPDHead2_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead12,
                ESPDHead.SPDHead3 => procInputs[EInjectProcInput.SPDHead3_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead34,
                ESPDHead.SPDHead4 => procInputs[EInjectProcInput.SPDHead4_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead34,
                _ => throw new Exception($"Invalid head: {head}")
            };
        }

        private bool IsSPDHeadWorkDoneForAssemble(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => procInputs[EInjectProcInput.SPDHead1_WorkDone].Value,
                ESPDHead.SPDHead2 => procInputs[EInjectProcInput.SPDHead2_WorkDone].Value,
                ESPDHead.SPDHead3 => procInputs[EInjectProcInput.SPDHead3_WorkDone].Value,
                ESPDHead.SPDHead4 => procInputs[EInjectProcInput.SPDHead4_WorkDone].Value,
                _ => throw new Exception($"Invalid head: {head}")
            };
        }

        private bool IsSPDHeadInjectAddTailDone(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => procInputs[EInjectProcInput.SPDHead1_InjectAddTailDone].Value || _currentRecipe.OptionRecipe.SkipHead12,
                ESPDHead.SPDHead2 => procInputs[EInjectProcInput.SPDHead2_InjectAddTailDone].Value || _currentRecipe.OptionRecipe.SkipHead12,
                ESPDHead.SPDHead3 => procInputs[EInjectProcInput.SPDHead3_InjectAddTailDone].Value || _currentRecipe.OptionRecipe.SkipHead34,
                ESPDHead.SPDHead4 => procInputs[EInjectProcInput.SPDHead4_InjectAddTailDone].Value || _currentRecipe.OptionRecipe.SkipHead34,
                _ => throw new Exception($"Invalid head: {head}")
            };
        }

        private bool IsSPDHeadInjectAddTailReady(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => procInputs[EInjectProcInput.Wait_SPDHead1_InjectAddTail].Value || _currentRecipe.OptionRecipe.SkipHead12,
                ESPDHead.SPDHead2 => procInputs[EInjectProcInput.Wait_SPDHead1_InjectAddTail].Value || _currentRecipe.OptionRecipe.SkipHead12,
                ESPDHead.SPDHead3 => procInputs[EInjectProcInput.Wait_SPDHead1_InjectAddTail].Value || _currentRecipe.OptionRecipe.SkipHead34,
                ESPDHead.SPDHead4 => procInputs[EInjectProcInput.Wait_SPDHead1_InjectAddTail].Value || _currentRecipe.OptionRecipe.SkipHead34,
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

        private bool IsHeadDotWeightingDone(ESPDHead head)
        {
            return head switch
            {
                ESPDHead.SPDHead1 => procInputs[EInjectProcInput.SPDHead1_DotWeightingDone].Value,
                ESPDHead.SPDHead2 => procInputs[EInjectProcInput.SPDHead2_DotWeightingDone].Value,
                ESPDHead.SPDHead3 => procInputs[EInjectProcInput.SPDHead3_DotWeightingDone].Value,
                ESPDHead.SPDHead4 => procInputs[EInjectProcInput.SPDHead4_DotWeightingDone].Value,
                _ => throw new Exception($"{head} is not valid"),
            };
        }

        private void XAxisMoveDotWeightingPosition()
        {
            if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
            {
                Log.Debug($"Move {XAxis.Name} to dot weighting for head 1, 3 pos [{_currentRecipe.InjectRecipe.XAxisH13DotWeightingPos}mm]");
                XAxis.MoveAbs(Recipe.XAxisH13DotWeightingPos);
            }
            else
            {
                Log.Debug($"Move {XAxis.Name} to dot weighting for head 2, 4 pos [{_currentRecipe.InjectRecipe.XAxisH13DotWeightingPos}mm]");
                XAxis.MoveAbs(Recipe.XAxisH24DotWeightingPos);
            }
        }

        private bool IsXAxisInDotWeightingPosition()
        {
            if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
            {
                return XAxis.IsOnPosition(Recipe.XAxisH13DotWeightingPos);
            }

            return XAxis.IsOnPosition(Recipe.XAxisH24DotWeightingPos);
        }

        private void ZAxisMoveDotWeightingPosition(ESPDHead head)
        {
            if (head == ESPDHead.SPDHead1)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos);
            if (head == ESPDHead.SPDHead2)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos);
            if (head == ESPDHead.SPDHead3)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos);
            if (head == ESPDHead.SPDHead4)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos);
            if (head == ESPDHead.All)
            {
                if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
                {
                    if (_machineStatus.IsSkipHead1 == false)
                    {
                        Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos);
                    }
                    if (_machineStatus.IsSkipHead3 == false)
                    {
                        Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos);
                    }
                }
                if (currentDotWeightingHead == EDotWeightingHead.HEAD24)
                {
                    if (_machineStatus.IsSkipHead2 == false)
                    {
                        Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos);
                    }
                    if (_machineStatus.IsSkipHead4 == false)
                    {
                        Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos);
                    }
                }
            }
        }

        private bool IsZAxisInDotWeightingPosition(ESPDHead head)
        {
            if (head == ESPDHead.All)
            {
                if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
                {
                    return (Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos) || _machineStatus.IsSkipHead1) &&
                        (Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos) || _machineStatus.IsSkipHead3);
                }

                return (Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos) || _machineStatus.IsSkipHead2) &&
                        (Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos) || _machineStatus.IsSkipHead4);
            }
            if (head == ESPDHead.SPDHead1)
            {
                return Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisWeightingPos);
            }
            if (head == ESPDHead.SPDHead2)
            {
                return Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos);
            }
            if (head == ESPDHead.SPDHead3)
            {
                return Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos);
            }

            return Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos);
        }

        private void DisableChamberOpenCloes()
        {
            if (Out_ChamberOpen == true || Out_ChamberClose == true)
            {
                Log.Debug("Dissable Open Close Chamber");
                Out_ChamberClose = false;
                Out_ChamberOpen = false;
            }

        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
        private readonly TactTimeList _tactTimeList;
        private readonly ICIMMapHelper _mapHelper;

        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        private OptionRecipe _optionRecipe => _currentRecipe.OptionRecipe;
        private InjectRecipe Recipe => _currentRecipe.InjectRecipe;
        private int _needleCleanCount = 0;
        private int _nzlCleanCount = 0;
        private int _yAxisCleaningPhase; // 0=chưa bắt đầu, 1=đang chờ tiến xong, 2=đang chờ lùi xong
        private double _yAxisCleaningTargetMm;
        private long _yAxisCleaningMoveStartTicks;

        private ESPDHead currentHead;
        private ESPDHead _failHead;
        private EDotWeightingHead currentDotWeightingHead;

        #endregion
    }
}
