using EQX.Core.Communication.CIM;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.InOut;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Productions;
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
        private IDInput In_ChamberClose => _devices.Inputs.ChamberClose;
        private IDInput In_ChamberOpen => _devices.Inputs.ChamberOpen;

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
            ICIMMapHelper mapHelper,
            ProductionService productionService,
            InOutHandler inOutHandler)
        {
            _devices = devices;
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;
            _tactTimeList = tactTimeList;
            _mapHelper = mapHelper;
            _productionService = productionService;
            _inOutHandler = inOutHandler;
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
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Head_Use_Check:
                    Log.Debug("Check head use");
                    if (_currentRecipe.OptionRecipe.SkipHead12 && _currentRecipe.OptionRecipe.SkipHead34)
                    {
                        RaiseWarning(EWarning.ET_MOLD_HEAD_USE_ERROR);
                        break;
                    }

                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Machine_Calibration_Check:
                    Log.Debug("Check machine calibration");
                    if (Parent!.Sequence != ESequence.AutoRun)
                    {
                        Step.ToRunStep++;
                        break;
                    }
#if !SIMULATION
                    Log.Debug("Machine Calibration check");
                    _machineStatus.MachineCalibration[0] |= _currentRecipe.OptionRecipe.SkipHead12 || _machineStatus.MachineCalibrationSkip[0];
                    _machineStatus.MachineCalibration[1] |= _currentRecipe.OptionRecipe.SkipHead12 || _machineStatus.MachineCalibrationSkip[1];
                    _machineStatus.MachineCalibration[2] |= _currentRecipe.OptionRecipe.SkipHead34 || _machineStatus.MachineCalibrationSkip[2];
                    _machineStatus.MachineCalibration[3] |= _currentRecipe.OptionRecipe.SkipHead34 || _machineStatus.MachineCalibrationSkip[3];

                    if (_machineStatus.MachineCalibration.All(x => x) == false &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        RaiseWarning(EWarning.VA_MACHINE_NEED_CALIBRATION);
                        break;
                    }
#endif
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Wait_DryPump_Request_Run:
                    if (ChamberOpenClose.IsOpen())
                    {
                        Log.Debug("Chamber is opened");
                        Step.ToRunStep = (int)EMoldProcToRunStep.SetFlag_ChamberReadyOut;
                        break;
                    }

                    if (procInputs[EInjectProcInput.DryPump_PurgeDone].Value == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug($"Input detect {EInjectProcInput.DryPump_PurgeDone}");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.SetFlag_ChamberReadyOut:
                    Log.Debug($"Set Output {EInjectProcOutput.ChamberReadyOut}");
                    procOutputs[EInjectProcOutput.ChamberReadyOut].Value = true;
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
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.ChamberOpenCloseCylinderMoveDelay,
                        () => ChamberOpenClose.IsClose());
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ChamberClose_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_CLOSE_FAIL);
                        break;
                    }

                    Log.Debug($"Move {ChamberOpenClose} close done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Bellow_Down:
                    if (BellowCyl.IsDown())
                    {
                        Log.Debug($"{BellowCyl} is down already");
                        Step.ToRunStep = (int)EMoldProcToRunStep.NozzleClean_Cyl_UnGrip;
                        break;
                    }

                    Log.Debug($"{BellowCyl} moving down");
                    BellowCyl.Down();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.BellowCylinderMoveDelay,
                        () => BellowCyl.IsDown());
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Bellow_DownWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_BELLOW_DOWN_FAIL);
                        break;
                    }

                    Log.Debug($"Move {BellowCyl} down done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.NozzleClean_Cyl_UnGrip:
                    if (NozzleCleanCyl_All_UnGrip_Check())
                    {
                        Log.Debug("Nozzle clean cylinder ungrip already");
                        Step.ToRunStep = (int)EMoldProcToRunStep.ZAxis_SafetyPos_Move;
                        break;
                    }

                    Log.Debug("Nozzle clean cylinder ungrip");
                    NozzleCleanCyl_All_UnGrip();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.NozzleCleanCylinderMoveDelay_1, () => NozzleCleanCyl_All_UnGrip_Check());
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.NozzleClean_Cyl_UnGrip_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_NOZZLE_CLEAN_UNGRIP_FAIL);
                        break;
                    }

                    Log.Debug("Nozzle clean cylinder ungrip done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ZAxis_SafetyPos_Move:
                    if (AllZAxisInSafetyPos(ref _failHead))
                    {
                        Log.Debug($"ZAxis is on safety position already");
                        Step.ToRunStep = (int)EMoldProcToRunStep.ClearFlag_ChamberReadyOut;
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
                        RaiseHeadWarning(EWarning.MO_Z1_AXIS_SAFE_POS_TIMEOUT, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety position done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ClearFlag_ChamberReadyOut:
                    if (procInputs[EInjectProcInput.DryPump_PurgeDone].Value == true)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug($"Clear Output ChamberReadyOut");
                    procOutputs[EInjectProcOutput.ChamberReadyOut].Value = false;
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
                case ESequence.ResinInjectAddTail:
                    Sequence_InjectAddTail();
                    break;
                case ESequence.Unloading:
                    Sequence_LoadingUnloading(isLoading: false);
                    break;
                case ESequence.DrainShot:
                    Sequence_AtDummyPos(ESequence.DummyShot, ESPDHead.All);
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
                case ESequence.HeadAssemble:
                    Sequence_AtDummyPos(ESequence.HeadAssemble, ESPDHead.All);
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
                case ESequence.HeadDisassemble:
                    Sequence_AtDummyPos(ESequence.HeadDisassemble, ESPDHead.All);
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
                case ESequence.IdlePurge:
                    Sequence_AtDummyPos(ESequence.IdlePurge, ESPDHead.All);
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
#if SIMULATION
                    SimulationInputSetter.SetSimInput(In_ChamberClose, true);
                    SimulationInputSetter.SetSimInput(In_ChamberOpen, false);
#endif
                    if (ChamberOpenClose.IsOpen())
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_OPEN_WARNING);
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
                        if (!Z1Axis.Status.IsHomeDone) RaiseWarning(EWarning.MO_Z1_AXIS_HOME_TIMEOUT);
                        if (!Z2Axis.Status.IsHomeDone) RaiseWarning(EWarning.MO_Z2_AXIS_HOME_TIMEOUT);
                        if (!Z3Axis.Status.IsHomeDone) RaiseWarning(EWarning.MO_Z3_AXIS_HOME_TIMEOUT);
                        if (!Z4Axis.Status.IsHomeDone) RaiseWarning(EWarning.MO_Z4_AXIS_HOME_TIMEOUT);
                        break;
                    }

                    Log.Debug("Z-Axes origin search done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.Bellow_Down:
                    if (BellowCyl.IsDown())
                    {
                        Step.OriginStep = (int)EMoldProcOriginStep.NozzleClean_Cyl_UnGrip;
                        break;
                    }

                    Log.Debug($"{BellowCyl} moving down");
                    BellowCyl.Down();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.BellowCylinderMoveDelay,
                        () => BellowCyl.IsDown());
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.Bellow_DownWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_BELLOW_DOWN_FAIL);
                        break;
                    }

                    Log.Debug($"Move {BellowCyl} down done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.NozzleClean_Cyl_UnGrip:
                    if (NozzleCleanCyl_All_UnGrip_Check())
                    {
                        Step.OriginStep = (int)EMoldProcOriginStep.XYAxis_Origin;
                        break;
                    }

                    Log.Debug("Nozzle clean cylinder ungrip");
                    NozzleCleanCyl_All_UnGrip();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.NozzleCleanCylinderMoveDelay_1, () => NozzleCleanCyl_All_UnGrip_Check());
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.NozzleClean_Cyl_UnGrip_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_NOZZLE_CLEAN_UNGRIP_FAIL);
                        break;
                    }

                    Log.Debug("Nozzle clean cylinder ungrip done");
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
                        if (!XAxis.Status.IsHomeDone) RaiseWarning(EWarning.MO_X_AXIS_HOME_TIMEOUT);
                        if (!YAxis.Status.IsHomeDone) RaiseWarning(EWarning.MO_Y_AXIS_HOME_TIMEOUT);
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
                            RaiseWarning(EWarning.MO_X_AXIS_DUMMY_POS_TIMEOUT);
                        if (!YAxis.IsOnPosition(Recipe.YAxisDummyPos))
                            RaiseWarning(EWarning.MO_Y_AXIS_DUMMY_POS_TIMEOUT);
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
                            RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DUMMY_POS_TIMEOUT);
                        }
                        if (!Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos))
                        {
                            RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DUMMY_POS_TIMEOUT);
                        }
                        if (!Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos))
                        {
                            RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DUMMY_POS_TIMEOUT);
                        }
                        if (!Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos))
                        {
                            RaiseWarning(EWarning.MO_SPD_H04_Z4_AXIS_DUMMY_POS_TIMEOUT);
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
                        RaiseHeadWarning(EWarning.MO_Z1_AXIS_SAFE_POS_TIMEOUT, _failHead);
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
            switch ((EMoldProcReadyStep)Step.RunStep)
            {
                case EMoldProcReadyStep.Start:
                    Log.Info("Ready start");
                    Step.RunStep++;
                    break;
                case EMoldProcReadyStep.BellowCyl_Down:
                    if (BellowCyl.IsDown())
                    {
                        Log.Debug($"{BellowCyl} is down already");
                        Step.RunStep = (int)EMoldProcReadyStep.XYAxis_ReadyPos_Move;
                        break;
                    }

                    Log.Debug($"{BellowCyl} moving down");
                    BellowCyl.Down();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.BellowCylinderMoveDelay,
                        () => BellowCyl.IsDown());
                    Step.RunStep++;
                    break;
                case EMoldProcReadyStep.BellowCyl_Down_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_BELLOW_DOWN_FAIL);
                        break;
                    }

                    Log.Debug($"Move {BellowCyl} down done");
                    Step.RunStep++;
                    break;
                case EMoldProcReadyStep.XYAxis_ReadyPos_Move:
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisReadyPos);
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisReadyPos) &&
                              YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcReadyStep.XYAxis_ReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisReadyPos))
                            RaiseWarning(EWarning.MO_X_AXIS_READY_POS_TIMEOUT);
                        else
                            RaiseWarning(EWarning.MO_Y_AXIS_READY_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{XAxis.Name}|{YAxis.Name} move to ready pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcReadyStep.End:
                    Log.Debug("Ready run end");
                    Sequence = ESequence.Stop;
                    break;
                default:
                    Wait(20);
                    break;
            }
        }

        private void Sequence_AutoRun()
        {
            switch ((EMoldProcAutoRunStep)Step.RunStep)
            {
                case EMoldProcAutoRunStep.Start:
                    Log.Debug("AutoRun start");
                    _tactTimeList.Chamber.CycleTimeCounter = Environment.TickCount;
                    Step.RunStep++;
                    break;
                case EMoldProcAutoRunStep.JigDetect_Check:
                    if (LeftJigTiltState)
                    {
                        RaiseWarning(EWarning.SE_CHAMBER_LEFT_JIG_TILT_DETECT);
                        break;
                    }
                    if (RightJigTiltState)
                    {
                        RaiseWarning(EWarning.SE_CHAMBER_RIGHT_JIG_TILT_DETECT);
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
                        RaiseWarning(EWarning.EF_CHAMBER_LEFT_JIG_INJECT_NOT_FINISHED);
                        break;
                    }
                    if (JigStatuses[(int)EJig.JigRight] == EJigStatus.InMolding)
                    {
                        RaiseWarning(EWarning.EF_CHAMBER_RIGHT_JIG_INJECT_NOT_FINISHED);
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
                    inputCount = 0;
                    if (_optionRecipe.SkipHead12 == false)
                    {
                        inputCount++;
                    }
                    if (_optionRecipe.SkipHead34 == false)
                    {
                        inputCount++;
                    }

                    Log.Debug($"Write data input count: {outputCount}");
                    _productionService.WriteData(EProductionWriteType.Input, inputCount);
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
                case EMoldProcResinInjectStep.InitQueue:
                    if (_optionRecipe.InjectAfterOpenAngleValve)
                    {
                        MoldResinInjectSteps = new Queue<EMoldProcResinInjectStep>(ProcessesWorkSequence.MoldResinInjectSequence_InjectAfterOpenAngleValve);

                    }
                    else
                    {
                        MoldResinInjectSteps = new Queue<EMoldProcResinInjectStep>(ProcessesWorkSequence.MoldResinInjectSequence);
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.StepQueue_EmptyCheck:
                    if (MoldResinInjectSteps.Count <= 0)
                    {
                        Step.RunStep = (int)EMoldProcResinInjectStep.Update_BothJigStatus;
                        break;
                    }

                    Step.RunStep = (int)MoldResinInjectSteps.Dequeue();
                    break;
                case EMoldProcResinInjectStep.XYAxis_InjectPos_Move:
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisInjectPos);
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisInjectPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisInjectPos) &&
                              YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisInjectPos));
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.XYAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisInjectPos))
                            RaiseWarning(EWarning.MO_X_AXIS_INJECT_POS_TIMEOUT);
                        else
                            RaiseWarning(EWarning.MO_Y_AXIS_INJECT_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{XAxis.Name}|{YAxis.Name} move to inject pos done");
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.ZAxisBellowCyl_InjectPos_Move:
                    ZAxisInjectPosMove();
                    BellowCyl.Up();

                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInInjectPos(ref _failHead) && BellowCyl.IsUp());
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.ZAxisBellowCyl_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!BellowCyl.IsUp())
                            RaiseWarning(EWarning.CY_BELLOW_UP_FAIL);
                        RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_INJECT_POS_TIMEOUT, _failHead);
                        break;
                    }

                    Log.Debug($"{BellowCyl} cylinder move up done");
                    Log.Debug($"ZAxis move to inject pos done");
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.SDPHead_Work_Request:
                    Log.Debug($"Set Output {EInjectProcOutput.SPDHeadWorkRequest}");

                    UpdateBothJigStatus(EJigStatus.InMolding);

                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = true;
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.DelayAfter_AngleValve_Open:
                    Log.Debug($"Delay {_currentRecipe.InjectTimeRecipe.DelayAfterOpenAngleValve}s after angle valve open");
                    Wait(_currentRecipe.InjectTimeRecipe.DelayAfterOpenAngleValve);
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.DryPump_Vacuum_Request:
                    Log.Debug($"Set Output {EInjectProcOutput.DryPump_VacuumRequest}");
                    procOutputs[EInjectProcOutput.DryPump_VacuumRequest].Value = true;
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.DryPump_Vacuum_DoneWait:
                    if (procInputs[EInjectProcInput.DryPump_VacuumDone].Value == false)
                    {
                        break;
                    }

                    Log.Debug($"Input detect {EInjectProcInput.DryPump_VacuumDone}");
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.SDPHead_Work_DoneWait:
                    if (IsSPDHeadWorkDone(ESequence.ResinInject, ESPDHead.SPDHead1) == false ||
                        IsSPDHeadWorkDone(ESequence.ResinInject, ESPDHead.SPDHead2) == false ||
                        IsSPDHeadWorkDone(ESequence.ResinInject, ESPDHead.SPDHead3) == false ||
                        IsSPDHeadWorkDone(ESequence.ResinInject, ESPDHead.SPDHead4) == false)
                    {
                        Wait(50);
                        break;
                    }

                    if (_currentRecipe.OptionRecipe.SkipVentTime == false &&
                        procInputs[EInjectProcInput.VentComplete].Value == false &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        Wait(20);
                        break;
                    }

                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = false;
                    Log.Debug($"Input detect EInjectProcInput.SPDHead(1~4)_WorkDone");
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.Update_BothJigStatus:
                    if (_currentRecipe.AdditionalMolding_Recipe.UseAddTail == false)
                    {
                        Log.Debug($"Update both jig status: {EJigStatus.MoldingFinish}");
                        UpdateBothJigStatus(EJigStatus.MoldingFinish);
                    }
                    if (_currentRecipe.OptionRecipe.SkipVentTime == false)
                    {
                        Step.RunStep = (int)EMoldProcResinInjectStep.BellowCylDown;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.Delay_AfterInjectFinish:
                    if (_optionRecipe.DelayAfterInjectFinish)
                    {
                        Log.Debug("Delay after inject finish");
                        Wait(_currentRecipe.InjectTimeRecipe.DelayAfterInjectFinish);
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
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.BellowCylinderMoveDelay, () => BellowCyl.IsDown());
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.BellowCylDown_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_BELLOW_DOWN_FAIL);
                        break;
                    }

                    Log.Debug("Bellow cylinder down finish");
                    Wait(1000); // Delay 1s after bellow cylinder down
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.AddTail_Check:
                    if (_currentRecipe.AdditionalMolding_Recipe.UseAddTail)
                    {
                        Step.RunStep = (int)EMoldProcResinInjectStep.ClearFlag;
                        break;
                    }

                    Log.Debug("Add tail NOT USE");
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
                        RaiseHeadWarning(EWarning.MO_Z1_AXIS_SAFE_POS_TIMEOUT, _failHead);
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
                    if (_currentRecipe.AdditionalMolding_Recipe.UseAddTail)
                    {
                        Sequence = ESequence.ResinInjectAddTail;
                        break;
                    }
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("ResinInject end");
                    Sequence = ESequence.DummyShot;
                    break;

            }
        }

        private void Sequence_InjectAddTail()
        {
            switch ((EMoldProcResinInjectAddTailStep)Step.RunStep)
            {
                case EMoldProcResinInjectAddTailStep.Start:
                    Log.Info("Inject add tail start");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.ZAxis_UpDistance_Move:
                    Log.Debug("ZAxis move up distance for add tail");
                    ZAxisAddTailPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInAddTailPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.ZAxis_UpDistance_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_ADDTAIL_UP_TIMEOUT, _failHead);
                        break;
                    }

                    Log.Debug("ZAxis move up distance for add tail done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.InjectAddTail_Request:
                    Log.Debug($"Set Output {EInjectProcOutput.InjectAddTail_Request}");
                    procOutputs[EInjectProcOutput.InjectAddTail_Request].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.Wait_SDPHead_AddTail_Done:
                    if (IsSPDHeadInjectAddTailDone(ESPDHead.SPDHead1) == false ||
                        IsSPDHeadInjectAddTailDone(ESPDHead.SPDHead2) == false ||
                        IsSPDHeadInjectAddTailDone(ESPDHead.SPDHead3) == false ||
                        IsSPDHeadInjectAddTailDone(ESPDHead.SPDHead4) == false)
                    {
                        Wait(20);
                        break;
                    }

                    procOutputs[EInjectProcOutput.InjectAddTail_Request].Value = false;
                    Log.Debug("SPDHead Inject AddTail done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.Update_BothJigStatus:
                    Log.Debug($"Update both jig status: {EJigStatus.MoldingFinish}");
                    UpdateBothJigStatus(EJigStatus.MoldingFinish);
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.ZAxis_SafetyPos_Move:
                    Log.Debug("All Z Axis moving to Safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.ZAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.MO_Z1_AXIS_SAFE_POS_TIMEOUT, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("ResinInject end");
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
                        _tactTimeList.Chamber.CycleTimeCounter = Environment.TickCount;
                        Log.Info("Loading start");
                        _machineStatus.ConfirmLoadingFinish = false;
                    }
                    else
                    {
                        Log.Info("Unloading start");
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
                        RaiseWarning(EWarning.MO_Y_AXIS_READY_POS_TIMEOUT);
                        break;
                    }

                    Wait(100);

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverOpen:
                    Log.Debug($"Open {ChamberOpenClose} cylinder");
                    ChamberOpenClose.Open();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.ChamberOpenCloseCylinderMoveDelay, ChamberOpenClose.IsOpen);
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverOpenWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_OPEN_FAIL);
                        break;
                    }

                    if (isLoading == false)
                    {
                        _tactTimeList.Chamber.SetTactTime();
                        _tactTimeList.TactCounterEnd();
                    }

                    Log.Debug($"{ChamberOpenClose} Open finish");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Transfer_Load_SendRequest:
                    if (_machineStatus.IsDryRunMode)
                    {
                        if (isLoading)
                        {
                            Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Chamber_CoverClose;
                            break;
                        }

                        Wait(3000);
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.End;
                        break;
                    }

                    if (_optionRecipe.InputTypeManual)
                    {
                        if (isLoading && _machineStatus.ConfirmLoadingFinish)
                        {
                            Log.Debug("Loading manual done");
                            Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Jig_Check;
                            break;
                        }
                        if (isLoading == false)
                        {
                            outputCount = 0;
                            Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Jig_Check;
                            break;
                        }
                    }

                    if (_optionRecipe.InputTypeAuto)
                    {
                        if (isLoading)
                        {
                            Step.RunStep++;
                            break;
                        }

                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Update_Jig_Status;
                        break;
                    }

                    break;
                case EMoldProcLoadingUnloadingStep.Wait_InOutHandlerStart_Request:
                    if (_machineStatus.InOutHandlerStartRequest == false)
                    {
                        Wait(20);
                        break;
                    }

                    _machineStatus.InOutHandlerStartRequest = false;
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Update_Jig_Status:
                    if (isLoading)
                    {
                        UpdateBothJigStatus(EJigStatus.None);
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Jig_Check:
                    if (isLoading && _machineStatus.DisableDetectJig == false && _machineStatus.IsDryRunMode == false)
                    {
                        Log.Debug("Jig check");
                        inputCount = 0;
#if SIMULATION
                        SimulationInputSetter.SetSimInput(In_Jig1Detect, true);
                        SimulationInputSetter.SetSimInput(In_Jig2Detect, true);
                        SimulationInputSetter.SetSimInput(In_Jig3Detect, true);
                        SimulationInputSetter.SetSimInput(In_Jig4Detect, true);
#endif
                        if (_optionRecipe.SkipHead12 == false)
                        {
                            if (LeftJigTiltState)
                            {
                                RaiseWarning(EWarning.SE_LEFFT_JIG_TILT_STATE);
                                break;
                            }
                            if (LeftJigDetect == false)
                            {
                                RaiseWarning(EWarning.SE_LEFT_JIG_NOT_DETECT);
                                break;
                            }

                            Log.Debug("Left Jig Loading Done");
                            inputCount++;
                        }

                        if (_optionRecipe.SkipHead34 == false)
                        {
                            if (RightJigTiltState)
                            {
                                RaiseWarning(EWarning.SE_RIGHT_JIG_TILT_STATE);
                                break;
                            }
                            if (RightJigDetect == false)
                            {
                                RaiseWarning(EWarning.SE_RIGHT_JIG_NOT_DETECT);
                                break;
                            }

                            Log.Debug("Right Jig Loading Done");
                            inputCount++;
                        }

                        Log.Debug($"Write data input count: {inputCount}");
                        _productionService.WriteData(EProductionWriteType.Input, inputCount);
                    }

                    if (isLoading == false && _machineStatus.IsDryRunMode == false)
                    {
                        if (_optionRecipe.SkipHead12 == false &&
                            In_Jig1Detect.Value == false &&
                            In_Jig2Detect.Value == false &&
                            JigStatuses[0] != EJigStatus.None)
                        {
                            Log.Debug("Left Jig Unload Done");
                            JigStatuses[0] = EJigStatus.None;
                            outputCount++;
                        }
                        if (_optionRecipe.SkipHead34 == false &&
                            In_Jig3Detect.Value == false &&
                            In_Jig4Detect.Value == false &&
                            JigStatuses[1] != EJigStatus.None)
                        {
                            Log.Debug("Right Jig Unload Done");
                            JigStatuses[1] = EJigStatus.None;
                            outputCount++;
                        }

                        if ((_optionRecipe.SkipHead12 == false && JigStatuses[0] != EJigStatus.None) ||
                            (_optionRecipe.SkipHead34 == false && JigStatuses[1] != EJigStatus.None))
                        {
                            Wait(20);
                            break;
                        }

                        Log.Debug($"Write data output count: {outputCount}");
                        _productionService.WriteData(EProductionWriteType.Output, outputCount);
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
                    else
                    {
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.End;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverClose:
                    _tactTimeList.Chamber.TactTimeCounter = Environment.TickCount;
                    _tactTimeList.TactCounterStart();
                    Log.Debug($"Close {ChamberOpenClose} cylinder");
                    ChamberOpenClose.Close();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.ChamberOpenCloseCylinderMoveDelay,
                        () => ChamberOpenClose.IsClose());
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverCloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_CLOSE_FAIL);
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
                    _tactTimeList.Chamber.SetCycleTime();
                    Sequence = ESequence.Loading;

                    break;
            }
        }

        private void Sequence_AtDummyPos(ESequence sequence, ESPDHead head)
        {
            switch ((EMoldProcDummyShotStep)Step.RunStep)
            {
                case EMoldProcDummyShotStep.Start:
                    if (sequence == ESequence.IdlePurge)
                    {
                        Log.Debug("Idle Purge start");
                    }

                    Log.Info($"Move dummy position start");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.XYAxis_DummyPos_Move:
                    Log.Debug($"Moving XY to dummy shot position: X={Recipe.XAxisDummyPos}, Y={Recipe.YAxisDummyPos}");
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
                            RaiseWarning(EWarning.MO_X_AXIS_DUMMY_POS_TIMEOUT);
                        if (!YAxis.IsOnPosition(Recipe.YAxisDummyPos))
                            RaiseWarning(EWarning.MO_Y_AXIS_DUMMY_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"Reached dummy shot position for {sequence}");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Move:
                    if (IsSequenceAssembleOrDisassemble(sequence))
                    {
                        Log.Debug("ZAxis move assemble/disassemble position");
                        ZAxisAssembleDisassemblePosMove(head);
                        Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => ZAxisInAssembleDisassemblePos(head));
                    }
                    else
                    {
                        Log.Debug("ZAxis move dummy position");
                        ZAxisDummyPosMove(sequence, head);
                        Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => ZAxisInDummyPos(head));
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (sequence == ESequence.DummyShot || sequence == ESequence.BubbleRemove || sequence == ESequence.IdlePurge)
                        {
                            if (Parent!.Sequence == ESequence.AutoRun || sequence == ESequence.IdlePurge)
                            {
                                if (_currentRecipe.OptionRecipe.SkipHead12 == false && !Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead12 == false && !Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead34 == false && !Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead34 == false && !Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H04_Z4_AXIS_DUMMY_POS_TIMEOUT);
                                }
                            }
                            else
                            {
                                if (_machineStatus.IsSkipHead1 == false && !Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_machineStatus.IsSkipHead2 == false == false && !Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_machineStatus.IsSkipHead3 == false == false && !Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_machineStatus.IsSkipHead4 == false == false && !Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H04_Z4_AXIS_DUMMY_POS_TIMEOUT);
                                }
                            }
                        }

                        if (sequence == ESequence.BubbleRemove_H1 ||
                            sequence == ESequence.BubbleRemove_H2 ||
                            sequence == ESequence.BubbleRemove_H3 ||
                            sequence == ESequence.BubbleRemove_H4)
                        {
                            RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_BUBBLE_POS_TIMEOUT, head);
                        }
                        else if (IsSequenceAssembleOrDisassemble(sequence))
                        {
                            RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_ASSEMBLE_POS_TIMEOUT, head);
                        }
                        else
                        {
                            RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_DUMMY_POS_TIMEOUT, head);
                        }
                        break;
                    }

                    Log.Debug($"Z-Axis move target position for {sequence} done");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.SPDHead_Working_Request:
                    Log.Debug($"Sending working request for {head}");
                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = true;
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.SPDHead_Work_Done_And_VentComplete_Wait:
                    if (IsSequenceAssembleOrDisassemble(sequence))
                    {
                        if (IsSPDHeadWorkDoneForAssemble(head) == false)
                        {
                            Wait(50);
                            break;
                        }
                    }
                    else
                    {
                        if (IsSPDHeadWorkDone(sequence, head) == false)
                        {
                            Wait(50);
                            break;
                        }
                    }

                    Log.Debug($"Detected SPDHead work done.");
                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_SafetyPos_Move:
                    if (IsSequenceAssembleOrDisassemble(sequence))
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
                        RaiseHeadWarning(EWarning.MO_Z1_AXIS_SAFE_POS_TIMEOUT, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety position done");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.End:
                    Log.Info($"{sequence} for {head} end");
                    _machineStatus.MachineIdleTick = Environment.TickCount;
                    if (Parent?.Sequence != ESequence.AutoRun && sequence != ESequence.IdlePurge)
                    {
                        MessageBoxEx.Show($"{sequence} Finish!", false);
                        Sequence = ESequence.Stop;
                    }

                    if (sequence == ESequence.DummyShot && Parent!.Sequence == ESequence.AutoRun)
                    {
                        Log.Info("Set next sequence to needdle clean");
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
                            RaiseWarning(EWarning.MO_X_AXIS_NEEDLE_CLEAN_TIMEOUT);
                        if (!YAxis.IsOnPosition(Recipe.YAxisNeddleClean))
                            RaiseWarning(EWarning.MO_Y_AXIS_NEEDLE_CLEAN_TIMEOUT);
                        break;
                    }

                    Log.Debug("XY Axis move needle clean done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGrip:
                    Log.Debug("Nozzle clean cylinder Ungrip");
                    NozzleCleanCyl_UnGrip(head);
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.NozzleCleanCylinderMoveDelay_1, () => NozzleCleanCyl_UnGrip_Check(head));
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGripWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_NOZZLE_CLEAN_UNGRIP_FAIL);
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
                        RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_NEEDLE_CLEAN_POS_TIMEOUT, _failHead);
                        break;
                    }

                    Log.Debug("Z axis move to needle clean pos done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_Grip:
                    Log.Debug("Nozzle clean cylinder Grip");
                    NozzleCleanCyl_Grip(head);
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.NozzleCleanCylinderMoveDelay_1, () => NozzleCleanCyl_Grip_Check(head));
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_GripWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_NOZZLE_CLEAN_GRIP_FAIL);
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
                        RaiseHeadWarning(EWarning.MO_Z1_AXIS_SAFE_POS_TIMEOUT, _failHead);
                        break;
                    }
                    
                    Log.Debug("Z axis up after clean done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGrip_AfterClean:
                    Log.Debug("Nozzle clean cylinder Ungrip");
                    NozzleCleanCyl_UnGrip(head);
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.NozzleCleanCylinderMoveDelay_1, () => NozzleCleanCyl_UnGrip_Check(head));
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.NzlCleanCyl_UnGrip_AfterClean_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_NOZZLE_CLEAN_UNGRIP_FAIL);
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
                            RaiseWarning(EWarning.MO_Y_AXIS_READY_POS_TIMEOUT);
                        }
                        else
                        {
                            if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
                            {
                                RaiseWarning(EWarning.MO_X_AXIS_H13_DOT_WEIGHT_POS_TIMEOUT);
                                break;
                            }
                            RaiseWarning(EWarning.MO_X_AXIS_H24_DOT_WEIGHT_POS_TIMEOUT);
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
                                    RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DOT_WEIGHT_POS_TIMEOUT);
                                    break;
                                }
                                if (Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisWeightingPos) == false && _machineStatus.IsSkipHead3)
                                {
                                    RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DOT_WEIGHT_POS_TIMEOUT);
                                    break;
                                }
                            }

                            if (currentDotWeightingHead == EDotWeightingHead.HEAD24)
                            {
                                if (Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisWeightingPos) == false && _machineStatus.IsSkipHead2)
                                {
                                    RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DOT_WEIGHT_POS_TIMEOUT);
                                    break;
                                }
                                if (Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisWeightingPos) == false && _machineStatus.IsSkipHead4)
                                {
                                    RaiseWarning(EWarning.MO_SPD_H04_Z4_AXIS_DOT_WEIGHT_POS_TIMEOUT);
                                    break;
                                }
                            }
                        }
                        else
                        {
                            RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_DOT_WEIGHT_POS_TIMEOUT, head);
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
                        RaiseHeadWarning(EWarning.MO_Z1_AXIS_SAFE_POS_TIMEOUT, _failHead);
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

        private bool IsSequenceAssembleOrDisassemble(ESequence sequence)
        {
            return sequence == ESequence.HeadAssemble || sequence == ESequence.HeadDisassemble ||
                   sequence == ESequence.HeadAssemble_H1 || sequence == ESequence.HeadAssemble_H2 ||
                   sequence == ESequence.HeadAssemble_H3 || sequence == ESequence.HeadAssemble_H4 ||
                   sequence == ESequence.HeadDisassemble_H1 || sequence == ESequence.HeadDisassemble_H2 ||
                   sequence == ESequence.HeadDisassemble_H3 || sequence == ESequence.HeadDisassemble_H4;
        }

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
            {
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead12)
            {
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead34)
            {
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead34)
            {
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos);
            }
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
            if (!_currentRecipe.OptionRecipe.SkipHead12)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            if (!_currentRecipe.OptionRecipe.SkipHead12)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            if (!_currentRecipe.OptionRecipe.SkipHead34)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            if (!_currentRecipe.OptionRecipe.SkipHead34)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
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

        private void ZAxisDummyPosMove(ESequence sequence, ESPDHead head)
        {
            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All && sequence != ESequence.IdlePurge)
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
            if (head == ESPDHead.All)
            {
                if (_machineStatus.IsSkipHead1 == false)
                    Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePos);
                if (_machineStatus.IsSkipHead2 == false)
                    Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePos);
                if (_machineStatus.IsSkipHead3 == false)
                    Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePos);
                if (_machineStatus.IsSkipHead4 == false)
                    Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePos);
            }
            if (head == ESPDHead.SPDHead1)
                Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePos);
            if (head == ESPDHead.SPDHead2)
                Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePos);
            if (head == ESPDHead.SPDHead3)
                Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePos);
            if (head == ESPDHead.SPDHead4)
                Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePos);
        }

        private bool ZAxisInAssembleDisassemblePos(ESPDHead head)
        {
            if (head == ESPDHead.All)
            {
                return
                    (_machineStatus.IsSkipHead1 || Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePos)) &&
                    (_machineStatus.IsSkipHead2 || Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePos)) &&
                    (_machineStatus.IsSkipHead3 || Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePos)) &&
                    (_machineStatus.IsSkipHead4 || Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePos));
            }

            if (head == ESPDHead.SPDHead1)
            {
                return Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead2)
            {
                return Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead3)
            {
                return Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead4)
            {
                return Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisAssembleDisassemblePos);
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

        private void NozzleCleanCyl_All_UnGrip()
        {
            NozzleClean_H1.Ungrip();
            NozzleClean_H2.Ungrip();
            NozzleClean_H3.Ungrip();
            NozzleClean_H4.Ungrip();
        }

        private bool NozzleCleanCyl_All_UnGrip_Check()
        {
#if SIMULATION
            SimulationInputSetter.SetSimInput(_devices.Inputs.Nozzle1Clean, false);
            SimulationInputSetter.SetSimInput(_devices.Inputs.Nozzle2Clean, false);
            SimulationInputSetter.SetSimInput(_devices.Inputs.Nozzle3Clean, false);
            SimulationInputSetter.SetSimInput(_devices.Inputs.Nozzle4Clean, false);
#endif
            return (NozzleClean_H1.IsUngrip() &&
                    NozzleClean_H2.IsUngrip() &&
                    NozzleClean_H3.IsUngrip() &&
                    NozzleClean_H4.IsUngrip());
        }

        private bool IsSPDHeadWorkDone(ESequence sequence, ESPDHead head)
        {
            if (head == ESPDHead.All)
            {
                if (Parent!.Sequence == ESequence.AutoRun || sequence == ESequence.IdlePurge)
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
            if (head == ESPDHead.All)
            {
                return (procInputs[EInjectProcInput.SPDHead1_WorkDone].Value || _machineStatus.IsSkipHead1) &&
                        (procInputs[EInjectProcInput.SPDHead2_WorkDone].Value || _machineStatus.IsSkipHead2) &&
                        (procInputs[EInjectProcInput.SPDHead3_WorkDone].Value || _machineStatus.IsSkipHead3) &&
                        (procInputs[EInjectProcInput.SPDHead4_WorkDone].Value || _machineStatus.IsSkipHead4);
            }
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

        private void UpdateBothJigStatus(EJigStatus status)
        {
            if (_optionRecipe.SkipHead12 == false)
            {
                JigStatuses[(int)EJig.JigLeft] = status;
            }
            if (_optionRecipe.SkipHead34 == false)
            {
                JigStatuses[(int)EJig.JigRight] = status;
            }
        }

        private void RaiseHeadWarning(EWarning warning, ESPDHead _failHead)
        {
            RaiseWarning(warning + ((int)(EWarning.MO_Z2_AXIS_HOME_TIMEOUT - EWarning.MO_Z1_AXIS_HOME_TIMEOUT)) * (_failHead - ESPDHead.SPDHead1));
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
        private readonly ProductionService _productionService;
        private readonly InOutHandler _inOutHandler;

        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        private OptionRecipe _optionRecipe => _currentRecipe.OptionRecipe;
        private InjectRecipe Recipe => _currentRecipe.InjectRecipe;
        private int inputCount;
        private int outputCount;
        private int _needleCleanCount = 0;
        private int _nzlCleanCount = 0;
        private int _yAxisCleaningPhase; // 0=chưa bắt đầu, 1=đang chờ tiến xong, 2=đang chờ lùi xong
        private double _yAxisCleaningTargetMm;
        private long _yAxisCleaningMoveStartTicks;
        public int IdlePurgeCount { get; private set; }
        public int IdlePurgeLastShotTick { get; private set; } = Environment.TickCount;

        private ESPDHead currentHead;
        private ESPDHead _failHead;
        private EDotWeightingHead currentDotWeightingHead;
        private Queue<EMoldProcResinInjectStep> MoldResinInjectSteps = new Queue<EMoldProcResinInjectStep>();
        #endregion
    }
}
