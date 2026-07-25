using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Communication.CIM.Custom.WordArea;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.InOut;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using ScottPlot.DataSources;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.Defines.Productions;
using SDV_MoldingInjection.Defines.TeachingPosition;
using SDV_MoldingInjection.Recipe;
using System.Text;
using TOPENG_Device;
using Windows.ApplicationModel.UserDataTasks.DataProvider;

namespace SDV_MoldingInjection.Process
{
    public class InjectProcess : MIProcess
    {
        #region Properties
        /// <summary>
        /// JigLeft / JigRight status
        /// </summary>
        public EJigStatus[] JigStatuses { get; }
        public int IdlePurgeCount { get; private set; }
        private double XAxisReadyPos => _positionList.InjectMaintenanceTeachingPosition.XAxisReadyPos.TargetPos;
        private double XAxisInjectPos => _positionList.InjectMaintenanceTeachingPosition.XAxisInjectPos.TargetPos;
        private double YAxisReadyPos => _positionList.InjectMaintenanceTeachingPosition.YAxisReadyPos.TargetPos;
        private double YAxisInjectPos => _positionList.InjectMaintenanceTeachingPosition.YAxisInjectPos.TargetPos;
        private double XAxisH13DotWeightingPos => _positionList.InjectMaintenanceTeachingPosition.XAxisH13DotWeightingPos.TargetPos;
        private double XAxisH24DotWeightingPos => _positionList.InjectMaintenanceTeachingPosition.XAxisH24DotWeightingPos.TargetPos;
        private double XAxisDummyPos => _positionList.InjectMaintenanceTeachingPosition.XAxisDummyPos.TargetPos;
        private double YAxisDummyPos => _positionList.InjectMaintenanceTeachingPosition.YAxisDummyPos.TargetPos;
        private double XAxisNeedleCleanPos => _positionList.InjectMaintenanceTeachingPosition.XAxisNeedleCleanPos.TargetPos;
        private double YAxisNeedleCleanPos => _positionList.InjectMaintenanceTeachingPosition.YAxisNeedleCleanPos.TargetPos;

        private double Z1AxisSafetyPos => _positionList.SPDHead1MaintenanceTeachingPosition.Z1AxisSafetyPos.TargetPos;
        private double Z2AxisSafetyPos => _positionList.SPDHead2MaintenanceTeachingPosition.Z2AxisSafetyPos.TargetPos;
        private double Z3AxisSafetyPos => _positionList.SPDHead3MaintenanceTeachingPosition.Z3AxisSafetyPos.TargetPos;
        private double Z4AxisSafetyPos => _positionList.SPDHead4MaintenanceTeachingPosition.Z4AxisSafetyPos.TargetPos;

        private double Z1AxisInjectPos => _positionList.SPDHead1MaintenanceTeachingPosition.ZAxisInjectPos.TargetPos;
        private double Z2AxisInjectPos => _positionList.SPDHead2MaintenanceTeachingPosition.ZAxisInjectPos.TargetPos;
        private double Z3AxisInjectPos => _positionList.SPDHead3MaintenanceTeachingPosition.ZAxisInjectPos.TargetPos;
        private double Z4AxisInjectPos => _positionList.SPDHead4MaintenanceTeachingPosition.ZAxisInjectPos.TargetPos;

        private double Z1UpDistancePos => _currentRecipe.SPDHead1_Recipe.ZAxisInjectUpDistance;
        private double Z2UpDistancePos => _currentRecipe.SPDHead2_Recipe.ZAxisInjectUpDistance;
        private double Z3UpDistancePos => _currentRecipe.SPDHead3_Recipe.ZAxisInjectUpDistance;
        private double Z4UpDistancePos => _currentRecipe.SPDHead4_Recipe.ZAxisInjectUpDistance;

        private double Z1SlowSpeed => _currentRecipe.SPDHead1_Recipe.ZAxisInjectSlowSpeed;
        private double Z2SlowSpeed => _currentRecipe.SPDHead2_Recipe.ZAxisInjectSlowSpeed;
        private double Z3SlowSpeed => _currentRecipe.SPDHead3_Recipe.ZAxisInjectSlowSpeed;
        private double Z4SlowSpeed => _currentRecipe.SPDHead4_Recipe.ZAxisInjectSlowSpeed;

        private double Z1AxisDummyPos => _positionList.SPDHead1MaintenanceTeachingPosition.ZAxisDummyPos.TargetPos;
        private double Z2AxisDummyPos => _positionList.SPDHead2MaintenanceTeachingPosition.ZAxisDummyPos.TargetPos;
        private double Z3AxisDummyPos => _positionList.SPDHead3MaintenanceTeachingPosition.ZAxisDummyPos.TargetPos;
        private double Z4AxisDummyPos => _positionList.SPDHead4MaintenanceTeachingPosition.ZAxisDummyPos.TargetPos;

        private double Z1AxisWeightingPos => _positionList.SPDHead1MaintenanceTeachingPosition.ZAxisWeightingPos.TargetPos;
        private double Z2AxisWeightingPos => _positionList.SPDHead2MaintenanceTeachingPosition.ZAxisWeightingPos.TargetPos;
        private double Z3AxisWeightingPos => _positionList.SPDHead3MaintenanceTeachingPosition.ZAxisWeightingPos.TargetPos;
        private double Z4AxisWeightingPos => _positionList.SPDHead4MaintenanceTeachingPosition.ZAxisWeightingPos.TargetPos;

        private double Z1AxisNeedleCleanPos => _positionList.SPDHead1MaintenanceTeachingPosition.ZAxisNeedleCleanPos.TargetPos;
        private double Z2AxisNeedleCleanPos => _positionList.SPDHead2MaintenanceTeachingPosition.ZAxisNeedleCleanPos.TargetPos;
        private double Z3AxisNeedleCleanPos => _positionList.SPDHead3MaintenanceTeachingPosition.ZAxisNeedleCleanPos.TargetPos;
        private double Z4AxisNeedleCleanPos => _positionList.SPDHead4MaintenanceTeachingPosition.ZAxisNeedleCleanPos.TargetPos;

        private double Z1AxisAssembleDisassemblePos => _positionList.SPDHead1MaintenanceTeachingPosition.ZAxisAssembleDisassemblePos.TargetPos;
        private double Z2AxisAssembleDisassemblePos => _positionList.SPDHead2MaintenanceTeachingPosition.ZAxisAssembleDisassemblePos.TargetPos;
        private double Z3AxisAssembleDisassemblePos => _positionList.SPDHead3MaintenanceTeachingPosition.ZAxisAssembleDisassemblePos.TargetPos;
        private double Z4AxisAssembleDisassemblePos => _positionList.SPDHead4MaintenanceTeachingPosition.ZAxisAssembleDisassemblePos.TargetPos;

        private bool CellId1SendOn => CIMAddressMap.ReadCIMBit("B2107") == 1;
        private bool CellId2SendOn => CIMAddressMap.ReadCIMBit("B2108") == 1;
        private bool UnloadJig1SendOn => CIMAddressMap.ReadCIMBit("B210A") == 1;
        private bool UnloadJig2SendOn => CIMAddressMap.ReadCIMBit("B210B") == 1;

        private bool CellId1Reply
        {
            set
            {
                CCLinkIEHelper.SetBit(0x2007, value);
            }
        }

        private bool CellId2Reply
        {
            set
            {
                CCLinkIEHelper.SetBit(0x2008, value);
            }
        }

        private bool CellIdTrackInStatus
        {
            set
            {
                CCLinkIEHelper.SetBit(0x2009, value);
            }
        }
        private CellJobProcessCimToPlcArea CellJobProcessInfo_Left { get; set; }
        private CellJobProcessCimToPlcArea CellJobProcessInfo_Right { get; set; }

        public MaterialAssemblyPlcToCimArea MaterialAssemblyArea_Left_1 { get; set; }
        public MaterialAssemblyPlcToCimArea MaterialAssemblyArea_Left_2 { get; set; }
        public MaterialAssemblyPlcToCimArea MaterialAssemblyArea_Right_1 { get; set; }
        public MaterialAssemblyPlcToCimArea MaterialAssemblyArea_Right_2 { get; set; }

        public string LeftJigID { get; set; } = "";
        public string RightJigID { get; set; } = "";
        public string LeftCellId { get; set; }
        public string RightCellId { get; set; }

        private bool SPDHead1OriginSelected => Parent!.Childs!.OfType<SPDHeadProcess>().First(p => p.Name == EProcess.SPDHead1.ToString()).IsOriginOrInitSelected;
        private bool SPDHead2OriginSelected => Parent!.Childs!.OfType<SPDHeadProcess>().First(p => p.Name == EProcess.SPDHead2.ToString()).IsOriginOrInitSelected;
        private bool SPDHead3OriginSelected => Parent!.Childs!.OfType<SPDHeadProcess>().First(p => p.Name == EProcess.SPDHead3.ToString()).IsOriginOrInitSelected;
        private bool SPDHead4OriginSelected => Parent!.Childs!.OfType<SPDHeadProcess>().First(p => p.Name == EProcess.SPDHead4.ToString()).IsOriginOrInitSelected;
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
            InOutHandler inOutHandler,
            PositionList positionList,
            CIMCollection cIMCollection)
        {
            _devices = devices;
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;
            _tactTimeList = tactTimeList;
            _mapHelper = mapHelper;
            _productionService = productionService;
            _inOutHandler = inOutHandler;
            _positionList = positionList;
            _cIMCollection = cIMCollection;
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
        public override bool PreProcess()
        {
            if (In_Jig1Detect.Value == false &&
                In_Jig2Detect.Value == false &&
                In_Jig3Detect.Value == false &&
                In_Jig4Detect.Value == false)
            {
                //FDCHelper.WriteFDCValue(EFDCValue.DISPENSING_STEP_ID, 0);
            }
            
            return base.PreProcess();
        }
        public override bool ProcessToAlarm()
        {
            DisableChamberOpenClose();
            return base.ProcessToAlarm();
        }

        public override bool ProcessToStop()
        {
            DisableChamberOpenClose();
            return base.ProcessToStop();
        }

        public override bool ProcessToWarning()
        {
            DisableChamberOpenClose();
            return base.ProcessToWarning();
        }

        public override bool ProcessToRun()
        {
            switch ((EMoldProcToRunStep)Step.ToRunStep)
            {
                case EMoldProcToRunStep.Start:
                    Log.Info("ToRun start");
                    Log.Debug($"{procOutputs.Name} ClearOutputs");
                    isJigLeftLoaded = false;
                    isJigRightLoaded = false;
                    isLoadingDry = true;
                    CellId1Reply = false;
                    CellId2Reply = false;
                    CellIdTrackInStatus = false;
                    _machineStatus.InOutHandlerStartRequest = false;
                    _machineStatus.SetDummyShotBeforeInject = false;
                    procOutputs.ClearOutputs();
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Head_Use_Check:
                    Log.Debug("Check head use");
                    //if (_currentRecipe.OptionRecipe.SkipHead12 && _currentRecipe.OptionRecipe.SkipHead34)
                    //{
                    //    RaiseWarning(EWarning.ET_MOLD_HEAD_USE_ERROR);
                    //    break;
                    //}

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
                    _machineStatus.MachineCalibration[0] = _currentRecipe.OptionRecipe.SkipHead12 || _machineStatus.MachineCalibrationSkip[0];
                    _machineStatus.MachineCalibration[1] = _currentRecipe.OptionRecipe.SkipHead12 || _machineStatus.MachineCalibrationSkip[1];
                    _machineStatus.MachineCalibration[2] = _currentRecipe.OptionRecipe.SkipHead34 || _machineStatus.MachineCalibrationSkip[2];
                    _machineStatus.MachineCalibration[3] = _currentRecipe.OptionRecipe.SkipHead34 || _machineStatus.MachineCalibrationSkip[3];

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
                case ESequence.HeadAssemble_Step1:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step1, ESPDHead.All);
                    break;
                case ESequence.HeadAssemble_Step1_H1:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step1_H1, ESPDHead.SPDHead1);
                    break;
                case ESequence.HeadAssemble_Step1_H2:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step1_H2, ESPDHead.SPDHead2);
                    break;
                case ESequence.HeadAssemble_Step1_H3:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step1_H3, ESPDHead.SPDHead3);
                    break;
                case ESequence.HeadAssemble_Step1_H4:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step1_H4, ESPDHead.SPDHead4);
                    break;
                case ESequence.HeadAssemble_Step2:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step2, ESPDHead.All);
                    break;
                case ESequence.HeadAssemble_Step2_H1:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step2_H1, ESPDHead.SPDHead1);
                    break;
                case ESequence.HeadAssemble_Step2_H2:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step2_H2, ESPDHead.SPDHead2);
                    break;
                case ESequence.HeadAssemble_Step2_H3:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step2_H3, ESPDHead.SPDHead3);
                    break;
                case ESequence.HeadAssemble_Step2_H4:
                    Sequence_AtDummyPos(ESequence.HeadAssemble_Step2_H4, ESPDHead.SPDHead4);
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
                    _cIMCollection.WriteMCC_OnlyStart(EMCCAction.IJ01_INIT_IJ01);
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

                    Log.Debug($"{XAxis.Name} {YAxis.Name} origin search done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_MoveDummyPos:
                    Log.Debug($"Move {XAxis.Name} {YAxis.Name} to dummy position");
                    XAxis.MoveAbs(XAxisDummyPos);
                    YAxis.MoveAbs(YAxisDummyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(XAxisDummyPos) &&
                              YAxis.IsOnPosition(YAxisDummyPos));
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_MoveDummyPosWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(XAxisDummyPos))
                            RaiseWarning(EWarning.MO_X_AXIS_DUMMY_POS_TIMEOUT);
                        if (!YAxis.IsOnPosition(YAxisDummyPos))
                            RaiseWarning(EWarning.MO_Y_AXIS_DUMMY_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"Move {XAxis.Name} {YAxis.Name} to dummy position X: {XAxisDummyPos}, Y: {YAxisDummyPos} done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_DummyPos_Move:
                    Log.Debug("ZAxis move dummy position");
                    if (SPDHead1OriginSelected)
                        Z1Axis.MoveAbs(Z1AxisDummyPos);
                    if (SPDHead2OriginSelected)
                        Z2Axis.MoveAbs(Z2AxisDummyPos);
                    if (SPDHead3OriginSelected)
                        Z3Axis.MoveAbs(Z3AxisDummyPos);
                    if (SPDHead4OriginSelected)
                        Z4Axis.MoveAbs(Z4AxisDummyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => (Z1Axis.IsOnPosition(Z1AxisDummyPos) || SPDHead1OriginSelected == false) &&
                                                                              (Z2Axis.IsOnPosition(Z2AxisDummyPos) || SPDHead2OriginSelected == false) &&
                                                                              (Z3Axis.IsOnPosition(Z3AxisDummyPos) || SPDHead3OriginSelected == false) &&
                                                                              (Z4Axis.IsOnPosition(Z4AxisDummyPos) || SPDHead4OriginSelected == false));
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!Z1Axis.IsOnPosition(Z1AxisDummyPos) && SPDHead1OriginSelected)
                        {
                            RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DUMMY_POS_TIMEOUT);
                        }
                        if (!Z2Axis.IsOnPosition(Z2AxisDummyPos) && SPDHead2OriginSelected)
                        {
                            RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DUMMY_POS_TIMEOUT);
                        }
                        if (!Z3Axis.IsOnPosition(Z3AxisDummyPos) && SPDHead3OriginSelected)
                        {
                            RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DUMMY_POS_TIMEOUT);
                        }
                        if (!Z4Axis.IsOnPosition(Z4AxisDummyPos) && SPDHead4OriginSelected)
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
                    if ((procInputs[EInjectProcInput.SPDHead1_OriginDone].Value == false && SPDHead1OriginSelected) ||
                        (procInputs[EInjectProcInput.SPDHead2_OriginDone].Value == false && SPDHead2OriginSelected) ||
                        (procInputs[EInjectProcInput.SPDHead3_OriginDone].Value == false && SPDHead3OriginSelected) ||
                        (procInputs[EInjectProcInput.SPDHead4_OriginDone].Value == false && SPDHead4OriginSelected))
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
                    _cIMCollection.WriteMCC_OnlyEnd(EMCCAction.IJ01_INIT_IJ01);
                    _waitTimeNoProduct = Environment.TickCount;
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
                    XAxis.MoveAbs(XAxisReadyPos);
                    YAxis.MoveAbs(YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(XAxisReadyPos) &&
                              YAxis.IsOnPosition(YAxisReadyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcReadyStep.XYAxis_ReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(XAxisReadyPos))
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
                    if (LeftJigDetect == false)
                    {
                        LeftJigID = string.Empty;
                        JigStatuses[(int)EJig.JigLeft] = EJigStatus.None;
                    }
                    if (RightJigDetect == false)
                    {
                        RightJigID = string.Empty;
                        JigStatuses[(int)EJig.JigRight] = EJigStatus.None;
                    }

                    FDCHelper.WriteFDCValue(EFDCValue.DISPENSING_CELL_ID, String.Empty);
                    FDCHelper.WriteFDCValue(EFDCValue.DISPENSING_STEP_ID, 0);

                    TactTime.CycleTimeCounter = Environment.TickCount;
                    Step.RunStep++;
                    break;
                case EMoldProcAutoRunStep.Wait_HeadUse:
                    if (_optionRecipe.SkipHead12 && _optionRecipe.SkipHead34)
                    {
                        Wait(100);
                        break;
                    }

                    Log.Debug("Deteced Head Use");
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
                    if (JigStatuses.Any(jig => jig == EJigStatus.MoldingFinish) ||
                        JigStatuses.Any(jig => jig == EJigStatus.InMolding))
                    {
                        Log.Info($"Detected Jig Finish or Not Finish -> Sequence set to {ESequence.Unloading}");
                        Sequence = ESequence.Unloading;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcAutoRunStep.ResinInject:
                    Log.Debug($"Write data input count: {inputCount}");
                    _productionService.WriteData(EProductionWriteType.Input, inputCount);
                    inputCount = 0;

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
                    _machineStatus.SetDummyShotBeforeInject = false;
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.Chamber_Close:
                    if (ChamberOpenClose.IsClose())
                    {
                        Log.Debug("Chamber is closed");
                        Step.RunStep = (int)EMoldProcResinInjectStep.InitQueue;
                        break;
                    }

                    Log.Debug($"{ChamberOpenClose} moving close");
                    ChamberOpenClose.Close();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.ChamberOpenCloseCylinderMoveDelay,
                        () => ChamberOpenClose.IsClose());
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectStep.Chamber_Close_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_CLOSE_FAIL);
                        break;
                    }

                    Log.Debug($"Move {ChamberOpenClose} close done");
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
                    Log.Debug($"Move XY Axis to Inject Position X:[${XAxisInjectPos},Y:[${YAxisInjectPos}]");
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_X_Y_TO_INJECT_POS);
                    XAxis.MoveAbs(XAxisInjectPos);
                    YAxis.MoveAbs(YAxisInjectPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(XAxisInjectPos) &&
                              YAxis.IsOnPosition(YAxisInjectPos));
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.XYAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(XAxisInjectPos))
                            RaiseWarning(EWarning.MO_X_AXIS_INJECT_POS_TIMEOUT);
                        else
                            RaiseWarning(EWarning.MO_Y_AXIS_INJECT_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{XAxis.Name}|{YAxis.Name} move to inject pos done");
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.ZAxisBellowCyl_InjectPos_UpDistance_Move:
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_Z_TO_INJECT_POS);
                    ZAxisInjectUpDistancePosMove();
                    BellowCyl.Up();

                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInInjectUpDistancePos(ref _failHead) && BellowCyl.IsUp());
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.ZAxisBellowCyl_InjectPos_UpDistance_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!BellowCyl.IsUp())
                            RaiseWarning(EWarning.CY_BELLOW_UP_FAIL);
                        RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_INJECT_UP_DISTANCE_POS_TIMEOUT, _failHead);
                        break;
                    }

                    Log.Debug($"{BellowCyl} cylinder move up done");
                    Log.Debug($"All ZAxis Move Inject Up Distance Position Done");
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.ZAxis_InjectPos_Move:
                    ZAxisInjectPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => AllZAxisInInjectPos(ref _failHead));
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.ZAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.MO_SPD_H01_Z1_AXIS_INJECT_POS_TIMEOUT, _failHead);
                        break;
                    }

                    Log.Debug($"All ZAxis Move Inject Position Done");
                    Step.RunStep = (int)EMoldProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EMoldProcResinInjectStep.SDPHead_Work_Request:
                    WriteMCC(LastAction, EMCCAction.IJ01_WAIT_INJECT_AND_VENT_TIME);
                    Log.Debug($"Set Output {EInjectProcOutput.SPDHeadWorkRequest}");
                    UpdateAllJigStatus(EJigStatus.InMolding);
                    _machineStatus.DummyShotCount++;
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

                    if (_recipeSelector.CurrentRecipe.OptionRecipe.UseCIM)
                    {
                        if (_recipeSelector.CurrentRecipe.OptionRecipe.SkipHead12 == false)
                        {
                            _cIMCollection.MaterialPorts[0].MaterialAssemble(_recipeSelector.RecipeSetting.CurrentRecipe, LeftCellId);
                            Thread.Sleep(300);
                            _cIMCollection.MaterialPorts[1].MaterialAssemble(_recipeSelector.RecipeSetting.CurrentRecipe, LeftCellId);

                            MaterialAssemblyArea_Left_1 = _cIMCollection.MaterialPorts[0].MaterialAssemblyArea;
                            MaterialAssemblyArea_Left_2 = _cIMCollection.MaterialPorts[1].MaterialAssemblyArea;
                        }

                        if (_recipeSelector.CurrentRecipe.OptionRecipe.SkipHead34 == false)
                        {
                            Thread.Sleep(300);
                            _cIMCollection.MaterialPorts[2].MaterialAssemble(_recipeSelector.RecipeSetting.CurrentRecipe, RightCellId);
                            Thread.Sleep(300);
                            _cIMCollection.MaterialPorts[3].MaterialAssemble(_recipeSelector.RecipeSetting.CurrentRecipe, RightCellId);

                            MaterialAssemblyArea_Right_1 = _cIMCollection.MaterialPorts[2].MaterialAssemblyArea;
                            MaterialAssemblyArea_Right_2 = _cIMCollection.MaterialPorts[3].MaterialAssemblyArea;
                        }

                        string portShortageWarning = string.Empty;
                        foreach (var materialPort in _cIMCollection.MaterialPorts)
                        {
                            //Auto Cancel Material
                            if (materialPort.RemainQty == 0)
                            {
                                materialPort.MaterialCancel?.Execute(null);
                            }
                            //MaterialWarning
                            if (materialPort.RemainQty == _recipeSelector.CurrentRecipe.CommonRecipe.MaterialWarningCount)
                            {
                                portShortageWarning += materialPort.Name + ",";
                                EquipEventHelpers.MaterialWarning(materialPort);
                                Thread.Sleep(300);
                            }
                        }

                        if (_cIMCollection.MaterialPorts.Any(mp => mp.RemainQty == _recipeSelector.CurrentRecipe.CommonRecipe.MaterialWarningCount))
                        {
                            MessageBoxEx.Show(portShortageWarning + "MATERIAL SHORTAGE WARNING!!");
                        }
                    }

                    if (_currentRecipe.OptionRecipe.SkipVentTime == false &&
                        procInputs[EInjectProcInput.VentComplete].Value == false)
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
                        UpdateAllJigStatus(EJigStatus.MoldingFinish);

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
                    Wait(500); // Delay 0.5s after bellow cylinder down
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
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_Z_TO_SAFETY_AFTER_INJECT_POS);
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
                    NeedleCleanCount++;
                    if (_currentRecipe.AdditionalMolding_Recipe.UseAddTail)
                    {
                        Log.Info("Next Sequence To Inject Add Tail");
                        Sequence = ESequence.ResinInjectAddTail;
                        break;
                    }

                    Log.Info("ResinInject end");
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    if (_optionRecipe.SkipDummyShot || _machineStatus.DummyShotCount < Recipe.DummyShotCycleCount)
                    {
                        if (_optionRecipe.SkipNeedleClean ||
                            NeedleCleanCount < Recipe.NiddleCleanCycleCount)
                        {
                            Log.Info("Next Sequence To Unloading");
                            Sequence = ESequence.Unloading;
                            break;
                        }

                        Log.Info("Skip DummyShot Next Sequence To NeddleClean");
                        NeedleCleanCount = 0;
                        Sequence = ESequence.NeedleCleaning;
                        break;
                    }

                    Log.Info("Next Sequence To DummyShot");
                    _machineStatus.DummyShotCount = 0;
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
                    UpdateAllJigStatus(EJigStatus.MoldingFinish);
                    Step.RunStep++;
                    break;
                case EMoldProcResinInjectAddTailStep.ZAxis_SafetyPos_Move:
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_Z_TO_SAFETY_AFTER_INJECT_POS);
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
                    Log.Info("ResinInject end");
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    if (_optionRecipe.SkipDummyShot)
                    {
                        if (_optionRecipe.SkipNeedleClean ||
                            NeedleCleanCount < Recipe.NiddleCleanCycleCount)
                        {
                            Log.Info("Next Sequence To Unloading");
                            Sequence = ESequence.Unloading;
                            break;
                        }

                        Log.Info("Skip DummyShot Next Sequence To NeddleClean");
                        NeedleCleanCount = 0;
                        Sequence = ESequence.NeedleCleaning;
                        break;
                    }

                    Log.Info("Next Sequence To DummyShot");
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
                        TactTime.CycleTimeCounter = Environment.TickCount;
                        _machineStatus.ConfirmLoadingFinish = false;
                    }
                    else
                    {
                        Log.Info("Unloading start");
                    }

                    TactTime.TactTimeCounter = Environment.TickCount;
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.YAxis_ReadyPos_Move:
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_Y_TO_READY_POS);
                    if (YAxis.IsOnPosition(YAxisReadyPos))
                    {
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Chamber_CoverOpen;
                        break;
                    }

                    Log.Debug($"Move {YAxis.Name} to ready pos [{YAxisReadyPos}mm]");
                    YAxis.MoveAbs(YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => YAxis.IsOnPosition(YAxisReadyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.YAxis_ReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.MO_Y_AXIS_READY_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{YAxis.Name} move to ready pos [{YAxisReadyPos}mm] DONE");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverOpen:
                    Log.Debug($"Open {ChamberOpenClose} cylinder");
                    WriteMCC(LastAction, EMCCAction.IJ01_CHAMBER_COVER_OPEN);

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
                        Log.Debug("Set TactTimeCounter End");
                        _tactTimeList.TactCounterEnd(((LeftJigDetect && RightJigDetect) || _machineStatus.IsDryRunMode) ? 2 : 1);
                    }

                    TactTime.SetTactTime();
                    Log.Debug($"{ChamberOpenClose} Open finish");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Transfer_LoadUnload_SendRequest:
                    WriteMCC(LastAction, EMCCAction.IJ01_OUT_WAIT_TIME_JIG);

                    if (isLoading)
                    {
                        //LOADING MANUAL
                        if (_optionRecipe.InputTypeManual)
                        {
                            if (_machineStatus.ConfirmLoadingFinish == false &&
                                _machineStatus.IsDryRunMode == false)
                            {
                                Wait(20);
                                break;
                            }
                        }
                    }

                    //UNLOADING
                    if (isLoading == false)
                    {
                        //SEMI SEQUENCE CHECK
                        if (Parent!.Sequence != ESequence.AutoRun)
                        {
                            Step.RunStep = (int)EMoldProcLoadingUnloadingStep.End;
                            break;
                        }

                        FDCHelper.WriteFDCValue(EFDCValue.DISPENSING_CELL_ID, String.Empty);
                        FDCHelper.WriteFDCValue(EFDCValue.DISPENSING_STEP_ID, 0);

                        if (_optionRecipe.InputTypeAuto)
                        {
                            Log.Info($"Request InOut Machine Unload: LEFT_{JigStatuses[0]}, RIGHT_{JigStatuses[1]}");
                            _inOutHandler.Handler_Send_End(JigStatuses);
                        }
                    }

                    Log.Debug("Next Step To Check InputType");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.CheckInputType:
                    //LOADING
                    if (_optionRecipe.InputTypeManual)
                    {
                        if (isLoading)
                        {
                            Log.Debug("Input Manual Next Step -> Load_Jig_Check");
                            Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Load_Jig_Check;
                            break;
                        }
                        else
                        {
                            Log.Debug("Unload Manual Next Step -> Unload Manual");
                            Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Unloading_Manual;
                            break;
                        }
                    }

                    Log.Debug("Wait InOut Machine Working And Request Start");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Wait_InOutHandler_Working_RequestStart:
                    //LOADING
                    if (CellId1SendOn ||
                       (_machineStatus.IsDryRunMode && isJigLeftLoaded == false && isLoadingDry && _machineStatus.RunWithoutInOut))
                    {
                        isJigLeftLoaded = true;
                        if ((LeftJigDetect || LeftJigTiltState) &&
                            _machineStatus.RunWithoutInOut == false)
                        {
                            RaiseWarning(EWarning.INOUT_MACHINE_REQUEST_LOAD_JIG_LEFT);
                            break;
                        }

                        Log.Debug("Cell ID 1 (B2107) Bit On -> Get Cell ID 1");
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.GetCellId1;
                        break;
                    }

                    if (CellId2SendOn ||
                       (_machineStatus.IsDryRunMode && isJigRightLoaded == false && isLoadingDry && _machineStatus.RunWithoutInOut))
                    {
                        isJigRightLoaded = true;
                        if ((RightJigDetect || RightJigTiltState) &&
                            _machineStatus.RunWithoutInOut == false)
                        {
                            RaiseWarning(EWarning.INOUT_MACHINE_REQUEST_LOAD_JIG_RIGHT);
                            break;
                        }

                        Log.Debug("Cell ID 2 Bit (B2108) On -> Get Cell ID 2");
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.GetCellId2;
                        break;
                    }

                    //UNLOADING
                    if ((LeftJigDetect == false || _machineStatus.RunWithoutInOut) &&
                        _optionRecipe.SkipHead12 == false &&
                        JigStatuses[(int)EJig.JigLeft] == EJigStatus.MoldingFinish)
                    {
                        Log.Debug("Unloading Left Jig");
                        LeftJigID = string.Empty;
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.CIM_TrackOut_LeftJig;
                        break;
                    }

                    if ((RightJigDetect == false || _machineStatus.RunWithoutInOut) &&
                        _optionRecipe.SkipHead34 == false &&
                        JigStatuses[(int)EJig.JigRight] == EJigStatus.MoldingFinish)
                    {
                        Log.Debug("Unloading Right Jig");
                        RightJigID = string.Empty;
                        isLoadingDry = true;
                        Step.RunStep = (int)EMoldProcLoadingUnloadingStep.CIM_TrackOut_RightJig;
                        break;
                    }

                    if ((_machineStatus.InOutHandlerStartRequest == false &&
                         _machineStatus.RunWithoutInOut == false) ||
                        (_optionRecipe.SkipHead12 && _optionRecipe.SkipHead34))
                    {
                        Wait(20);
                        break;
                    }

                    isJigLeftLoaded = false;
                    isJigRightLoaded = false;
                    isLoadingDry = false;
                    Log.Info("Received Start Request From InOut Machine");
                    _inOutHandler.SendTransmitCTST();
                    Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Load_Jig_Check;
                    break;
                case EMoldProcLoadingUnloadingStep.GetCellId1:
                    string LeftJigIDDryRun = "LEFT" + random.Next(100, 1000).ToString();
                    LeftJigID = _machineStatus.RunWithoutInOut ? LeftJigIDDryRun : GetCellId(isJigLeft: true);
                    Log.Info($"Get Left Jig ID '{LeftJigID}'");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Write_JigID_1:
                    Log.Info($"Write Left Jig ID : {LeftJigID}");
#if !SIMULATION
                    _cIMCollection.WriteMCC_CellID(EMCCUnit.IJ01, LeftJigID);
                    //Wait(5000, () => _cIMCollection.ReadMCC_CellID(EMCCUnit.IJ01) == LeftJigID);
#endif
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Wait_Write_JigIID_1:
                    //if (WaitTimeOutOccurred)
                    //{
                    //    RaiseWarning(EWarning.MCC_IJ01_WRITE_JIG_ID_TIMEOUT);
                    //    break;
                    //}

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Validation_Reply_Jig1:
                    CellIdTrackInStatus = CIMValidation(isJigLeft: true);
                    CellId1Reply = true;
                    Log.Debug($"Cell 1 Reply Bit (B2007) On");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Wait_Cell1SendBitOff:
                    Log.Debug("Wait InOutMachine OFF B2107");
                    Wait(30000, () => CellId1SendOn == false);
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Wait_Cell1SendBitOff_TimeOut:
                    if (WaitTimeOutOccurred && _machineStatus.RunWithoutInOut == false)
                    {
                        RaiseWarning(EWarning.INOUT_MACHINE_REPLY_TIMEOUT);
                        break;
                    }

                    Log.Debug("Cell 1 Send Bit (B2107) Off");
                    CellId1Reply = false;
                    Log.Debug("Cell 1 Reply Bit (B2007) Off");
                    CellIdTrackInStatus = false;
                    Log.Debug("Cell TrackIn Status Bit (B2009) Off");
                    UpdateBothJigStatus(EJigStatus.Ready, isLeftJig: true);
                    Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Wait_InOutHandler_Working_RequestStart;
                    break;
                case EMoldProcLoadingUnloadingStep.GetCellId2:
                    RightJigID = _machineStatus.RunWithoutInOut ? "RIGHT" + random.Next(100, 1000).ToString() : GetCellId(isJigLeft: false);
                    Log.Info($"Get Right Jig ID '{RightJigID}'");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Write_JigID_2:
                    Log.Info($"Write Right Jig ID : {RightJigID}");
#if !SIMULATION
                    _cIMCollection.WriteMCC_CellID(EMCCUnit.IJ02, RightJigID);
                    //Wait(5000, () => _cIMCollection.ReadMCC_CellID(EMCCUnit.IJ01) == RightJigID);
#endif
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Wait_Write_JigIID_2:
                    //if (WaitTimeOutOccurred)
                    //{
                    //    RaiseWarning(EWarning.MCC_IJ01_WRITE_JIG_ID_TIMEOUT);
                    //    break;
                    //}

                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Validation_Reply_Jig2:
                    CellIdTrackInStatus = CIMValidation(isJigLeft: false);
                    CellId2Reply = true;
                    Log.Debug("Cell 2 Reply Bit (B2008) On");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Wait_Cell2SendBitOff:
                    Log.Debug("Wait InOutMachine OFF B2108");
                    Wait(30000, () => CellId2SendOn == false);
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Wait_Cell2SendBitOff_TimeOut:
                    if (WaitTimeOutOccurred && _machineStatus.RunWithoutInOut == false)
                    {
                        RaiseWarning(EWarning.INOUT_MACHINE_REPLY_TIMEOUT);
                        break;
                    }

                    Log.Debug("Cell 2 Send Bit (B2108) Off");
                    CellId2Reply = false;
                    Log.Debug("Cell 2 Reply Bit (B2008) Off");
                    CellIdTrackInStatus = false;
                    Log.Debug("Cell TrackIn Status Bit (B2009) Off");
                    UpdateBothJigStatus(EJigStatus.Ready, isLeftJig: false);
                    Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Wait_InOutHandler_Working_RequestStart;
                    break;
                case EMoldProcLoadingUnloadingStep.CIM_TrackOut_LeftJig:
                    Log.Debug("Left Jig TrackOut");
                    if (_recipeSelector.CurrentRecipe.OptionRecipe.UseCIM && _machineStatus.IsAutoRunMode)
                    {
                        if (CellJobProcessInfo_Left == null)
                        {
                            RaiseWarning(EWarning.CIM_LEFT_CELL_LOST_INFORMATION);
                            break;
                        }
                        Log.Info($"Tracking Out Cell '{CellJobProcessInfo_Left.CellJobProcessCellID}'");
                        if (EquipEventHelpers.CellTrackOut(1, CellJobProcessInfo_Left, false, new List<MaterialAssemblyPlcToCimArea> { MaterialAssemblyArea_Left_1, MaterialAssemblyArea_Left_2 }) == false)
                        {
                            RaiseWarning(EWarning.CIM_CELL_TRACKOUT_FAIL);
                            break;
                        }
                    }

                    Log.Debug($"Update Status Left Jig: {EJigStatus.None}");
                    Log.Debug("CIM TrackOut Left Jig Done. Wait InOut Working");
                    UpdateBothJigStatus(EJigStatus.None, isLeftJig: true);
                    if (_machineStatus.IsDryRunMode == false)
                    {
                        outputCount++;
                    }
                   
                    Step.RunStep = _optionRecipe.SkipHead34 ?
                        (int)EMoldProcLoadingUnloadingStep.Unloading_WriteData :
                        (int)EMoldProcLoadingUnloadingStep.Wait_InOutHandler_Working_RequestStart;
                    break;
                case EMoldProcLoadingUnloadingStep.CIM_TrackOut_RightJig:
                    if (_recipeSelector.CurrentRecipe.OptionRecipe.UseCIM && _machineStatus.IsAutoRunMode)
                    {
                        if (CellJobProcessInfo_Right == null)
                        {
                            RaiseWarning(EWarning.CIM_RIGHT_CELL_LOST_INFORMATION);
                            break;
                        }
                        Log.Info($"Tracking Out Cell '{CellJobProcessInfo_Right.CellJobProcessCellID}'");
                        if (EquipEventHelpers.CellTrackOut(2, CellJobProcessInfo_Right, false, new List<MaterialAssemblyPlcToCimArea> { MaterialAssemblyArea_Right_1, MaterialAssemblyArea_Right_2 }) == false)
                        {
                            RaiseWarning(EWarning.CIM_CELL_TRACKOUT_FAIL);
                            break;
                        }
                    }

                    Log.Debug($"Update Status Right Jig: {EJigStatus.None}");
                    Log.Debug("CIM TrackOut Right Jig Done. Wait InOut Working");
                    UpdateBothJigStatus(EJigStatus.None, isLeftJig: false);
                    if (_machineStatus.IsDryRunMode == false)
                    {
                        outputCount++;
                    }

                    Step.RunStep = (int)EMoldProcLoadingUnloadingStep.Unloading_WriteData;
                    break;
                case EMoldProcLoadingUnloadingStep.Unloading_Manual:
                    if (In_Jig1Detect.Value == false &&
                        In_Jig2Detect.Value == false &&
                        _optionRecipe.SkipHead12 == false &&
                        JigStatuses[(int)EJig.JigLeft] != EJigStatus.None)
                    {
                        JigStatuses[(int)EJig.JigLeft] = EJigStatus.None;
                        LeftJigID = String.Empty;
                        outputCount++;
                    }

                    if (In_Jig3Detect.Value == false &&
                        In_Jig4Detect.Value == false &&
                        _optionRecipe.SkipHead34 == false &&
                        JigStatuses[(int)EJig.JigRight] != EJigStatus.None)
                    {
                        JigStatuses[(int)EJig.JigRight] = EJigStatus.None;
                        LeftJigID = String.Empty;
                        outputCount++;
                    }

                    if ((_optionRecipe.SkipHead12 == false && JigStatuses[(int)EJig.JigLeft] != EJigStatus.None) ||
                        (_optionRecipe.SkipHead34 == false && JigStatuses[(int)EJig.JigRight] != EJigStatus.None))
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug("Unload Manual Done");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Unloading_WriteData:
                    Log.Debug($"Write data output count: {outputCount}");
                    _productionService.WriteData(EProductionWriteType.Output, outputCount);
                    //if(IsFirstCycle)
                    //{
                    //    IsFirstCycle = false;
                    //    _tactTimeList.TactCounterStart();
                    //}
                    //else
                    //{
                    //    _tactTimeList.TactCounterEnd(_machineStatus.IsDryRunMode ? 2 : outputCount);
                    //}

                    outputCount = 0;

                    if (LeftJigDetect == false && RightJigDetect == false)
                    {
                        Log.Info("Unloading end. Wait InOut Working And Request Start.");
                        TactTime.SetCycleTime();
                    }

                    Step.RunStep = _optionRecipe.InputTypeAuto ? (int)EMoldProcLoadingUnloadingStep.Wait_InOutHandler_Working_RequestStart :
                                                                 (int)EMoldProcLoadingUnloadingStep.End;
                    break;
                case EMoldProcLoadingUnloadingStep.Load_Jig_Check:
                    //TODO: Dry run
                    Log.Debug("Load Jig Check");
                    inputCount = 0;
#if SIMULATION
                    SimulationInputSetter.SetSimInput(In_Jig1Detect, true);
                    SimulationInputSetter.SetSimInput(In_Jig2Detect, true);
                    SimulationInputSetter.SetSimInput(In_Jig3Detect, true);
                    SimulationInputSetter.SetSimInput(In_Jig4Detect, true);
#endif
                    if (_optionRecipe.SkipHead12 == false &&
                        _machineStatus.DisableDetectJig == false &&
                        _machineStatus.IsDryRunMode == false)
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

                    if (_optionRecipe.SkipHead34 == false &&
                        _machineStatus.DisableDetectJig == false &&
                         _machineStatus.IsDryRunMode == false)
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
                    inputCount = 0;
                    Log.Debug("Set TactTimeCounter Start");
                    _tactTimeList.TactCounterStart();
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.Chamber_CoverClose:
                    LastAction = EMCCAction.IJ01_ACT_WAIT_TRANSFER_REQ_START;
                    WriteMCC(LastAction, EMCCAction.IJ01_CHAMPER_COVER_CLOSE);

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

                    Log.Debug("Chamber Closed");
                    Step.RunStep++;
                    break;
                case EMoldProcLoadingUnloadingStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    if (isLoading == false && _optionRecipe.InputTypeManual)
                    {
                        Log.Info("Set Next Sequence To Loading");
                        Sequence = ESequence.Loading;
                        break;
                    }

                    Log.Info("Loading End");
                    if ((Environment.TickCount - _waitTimeNoProduct) >= _currentRecipe.InjectTimeRecipe.DummyShotBeforeInject * 60000 &&
                        _optionRecipe.DummyBeforeInject &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        Log.Info("Set Next Sequence To DummyShot");
                        _machineStatus.SetDummyShotBeforeInject = true;
                        Sequence = ESequence.DummyShot;
                        break;
                    }

                    Log.Info("Set Next Sequence To Resin Inject");
                    Sequence = ESequence.ResinInject;
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
                        IdlePurgeCount = 0;
                        _devices.Outputs.DryPumpRun.Value = false;
                    }

                    Log.Info($"Move dummy position start");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.Chamber_Close:
                    if (ChamberOpenClose.IsClose())
                    {
                        Log.Debug("Chamber is closed");
                        Step.RunStep = (int)EMoldProcDummyShotStep.XYAxis_DummyPos_Move;
                        break;
                    }

                    Log.Debug($"{ChamberOpenClose} moving close");
                    ChamberOpenClose.Close();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.ChamberOpenCloseCylinderMoveDelay,
                        () => ChamberOpenClose.IsClose());
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.Chamber_Close_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_CLOSE_FAIL);
                        break;
                    }

                    Log.Debug($"Move {ChamberOpenClose} close done");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.XYAxis_DummyPos_Move:
                    Log.Debug($"Moving XY to dummy shot position: X={XAxisDummyPos}, Y={YAxisDummyPos}");

                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_X_Y_TO_DUMMY_SHOT_POS);

                    XAxis.MoveAbs(XAxisDummyPos);
                    YAxis.MoveAbs(YAxisDummyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(XAxisDummyPos) &&
                        YAxis.IsOnPosition(YAxisDummyPos));
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.XYAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(XAxisDummyPos))
                            RaiseWarning(EWarning.MO_X_AXIS_DUMMY_POS_TIMEOUT);
                        if (!YAxis.IsOnPosition(YAxisDummyPos))
                            RaiseWarning(EWarning.MO_Y_AXIS_DUMMY_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"Reached dummy shot position for {sequence}");
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Move:
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_Z_TO_DUMMY_SHOT_POS);

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
                        Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => ZAxisInDummyPos(sequence, head));
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_DummyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (sequence == ESequence.IdlePurge)
                        {
                            if (!Z1Axis.IsOnPosition(Z1AxisDummyPos))
                            {
                                RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DUMMY_POS_TIMEOUT);
                            }
                            if (!Z2Axis.IsOnPosition(Z2AxisDummyPos))
                            {
                                RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DUMMY_POS_TIMEOUT);
                            }
                            if (!Z3Axis.IsOnPosition(Z3AxisDummyPos))
                            {
                                RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DUMMY_POS_TIMEOUT);
                            }
                            if (!Z4Axis.IsOnPosition(Z4AxisDummyPos))
                            {
                                RaiseWarning(EWarning.MO_SPD_H04_Z4_AXIS_DUMMY_POS_TIMEOUT);
                            }
                        }
                        else if (sequence == ESequence.DummyShot || sequence == ESequence.BubbleRemove)
                        {
                            if (Parent!.Sequence == ESequence.AutoRun || sequence == ESequence.IdlePurge)
                            {
                                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead1 == false && !Z1Axis.IsOnPosition(Z1AxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead2 == false && !Z2Axis.IsOnPosition(Z2AxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead3 == false && !Z3Axis.IsOnPosition(Z3AxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead4 == false && !Z4Axis.IsOnPosition(Z4AxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H04_Z4_AXIS_DUMMY_POS_TIMEOUT);
                                }
                            }
                            else
                            {
                                if (_machineStatus.IsSkipHead1 == false && !Z1Axis.IsOnPosition(Z1AxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_machineStatus.IsSkipHead2 == false == false && !Z2Axis.IsOnPosition(Z2AxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_machineStatus.IsSkipHead3 == false == false && !Z3Axis.IsOnPosition(Z3AxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DUMMY_POS_TIMEOUT);
                                }
                                if (_machineStatus.IsSkipHead4 == false == false && !Z4Axis.IsOnPosition(Z4AxisDummyPos))
                                {
                                    RaiseWarning(EWarning.MO_SPD_H04_Z4_AXIS_DUMMY_POS_TIMEOUT);
                                }
                            }
                        }
                        else if (sequence == ESequence.BubbleRemove_H1 ||
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
                    WriteMCC(LastAction, EMCCAction.IJ01_WAIT_DUMMY_SHOT);
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
                    IdlePurgeCount++;
                    procOutputs[EInjectProcOutput.SPDHeadWorkRequest].Value = false;
                    Step.RunStep++;
                    break;
                case EMoldProcDummyShotStep.ZAxis_SafetyPos_Move:
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_Z_TO_SAFETY_AFTER_DUMMY_POS);

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
                    _waitTimeNoProduct = Environment.TickCount;

                    if (Parent?.Sequence == ESequence.AutoRun)
                    {
                        if (sequence == ESequence.DummyShot)
                        {
                            if (_optionRecipe.SkipNeedleClean ||
                                NeedleCleanCount < Recipe.NiddleCleanCycleCount)
                            {
                                Log.Info("Set Next Sequence To Unloading");
                                Sequence = ESequence.Unloading;
                                break;
                            }

                            Log.Info("Set Next Sequence To NeeddleClean");
                            NeedleCleanCount = 0;
                            Sequence = ESequence.NeedleCleaning;
                            break;
                        }
                    }

                    if (sequence == ESequence.IdlePurge)
                    {
                        Log.Debug("Wait Next Cycle IdlePurge");
                        Wait(_currentRecipe.IdlePurgeRecipe.IdlePurgeCycleTime * 60);
                        Step.RunStep = (int)EMoldProcDummyShotStep.Chamber_Close;
                        break;
                    }

                    MessageBoxEx.Show($"{sequence} Finish!", false);
                    Sequence = ESequence.Stop;
                    break;
            }
        }

        private void Sequence_NeedleClean(ESequence sequence, ESPDHead head)
        {
            switch ((EMoldProcNeedleCleaningStep)Step.RunStep)
            {
                case EMoldProcNeedleCleaningStep.Start:
                    Log.Info("NeedleClean start");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.Chamber_Close:
                    if (ChamberOpenClose.IsClose())
                    {
                        Log.Debug("Chamber is closed");
                        Step.RunStep = (int)EMoldProcNeedleCleaningStep.YAxis_CleanPos_Calculator;
                        break;
                    }

                    Log.Debug($"{ChamberOpenClose} moving close");
                    ChamberOpenClose.Close();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.ChamberOpenCloseCylinderMoveDelay,
                        () => ChamberOpenClose.IsClose());
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.Chamber_Close_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_CLOSE_FAIL);
                        break;
                    }

                    Log.Debug($"Move {ChamberOpenClose} close done");
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.YAxis_CleanPos_Calculator:
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_X_Y_TO_NOZZLE_CLEAN_POS);

                    _yAxisCleaningTargetMm = YAxisNeedleCleanPos - _nzlCleanCount * Recipe.NiddleCleanShiftDist;
                    if (_yAxisCleaningTargetMm < -19 || //Limit sensor
                       (YAxisNeedleCleanPos - _yAxisCleaningTargetMm) > 130) //Distance clean
                    {
                        Log.Debug("Reset nozzle clean count");
                        _nzlCleanCount = 0;
                        _yAxisCleaningTargetMm = YAxisNeedleCleanPos;
                    }

                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.XY_Axis_NeedleCleanPos_Move:
                    Log.Debug($"XY axis move to needle clean position X: {XAxisNeedleCleanPos}, Y: {_yAxisCleaningTargetMm}");
                    XAxis.MoveAbs(XAxisNeedleCleanPos);
                    YAxis.MoveAbs(_yAxisCleaningTargetMm);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        XAxis.IsOnPosition(XAxisNeedleCleanPos) &&
                        YAxis.IsOnPosition(_yAxisCleaningTargetMm));
                    Step.RunStep++;
                    break;
                case EMoldProcNeedleCleaningStep.XY_Axis_NeedleCleanPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(XAxisNeedleCleanPos))
                            RaiseWarning(EWarning.MO_X_AXIS_NEEDLE_CLEAN_TIMEOUT);
                        if (!YAxis.IsOnPosition(YAxisNeedleCleanPos))
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
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_Z_TO_NOZZLE_CLEAN_POS);

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

                    WriteMCC(LastAction, EMCCAction.IJ01_WAIT_NOZZLE_CLEAN);

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
                    WriteMCC(LastAction, EMCCAction.IJ01_MOVE_Z_TO_SAFET_AFTER_CLEANY_POS);

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

                    if (_machineStatus.SetDummyShotBeforeInject)
                    {
                        Log.Info("Set next sequence to Resin Inject");
                        Sequence = ESequence.ResinInject;
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
                case EMoldProcDotWeightingStep.Chamber_Close:
                    if (ChamberOpenClose.IsClose())
                    {
                        Log.Debug("Chamber is closed");
                        Step.RunStep = (int)EMoldProcDotWeightingStep.SPDHead_WorkCheck;
                        break;
                    }

                    Log.Debug($"{ChamberOpenClose} moving close");
                    ChamberOpenClose.Close();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.ChamberOpenCloseCylinderMoveDelay,
                        () => ChamberOpenClose.IsClose());
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.Chamber_Close_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_CLOSE_FAIL);
                        break;
                    }

                    Log.Debug($"Move {ChamberOpenClose} close done");
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
                    Log.Debug($"Move {YAxis.Name} to ready pos [{YAxisReadyPos}mm]");
                    XAxisMoveDotWeightingPosition();
                    YAxis.MoveAbs(YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                        YAxis.IsOnPosition(YAxisReadyPos) &&
                        IsXAxisInDotWeightingPosition());
                    Step.RunStep++;
                    break;
                case EMoldProcDotWeightingStep.XYAxis_DotWeightingReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (YAxis.IsOnPosition(YAxisReadyPos) == false)
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
                                if (Z1Axis.IsOnPosition(Z1AxisWeightingPos) == false && _machineStatus.IsSkipHead1)
                                {
                                    RaiseWarning(EWarning.MO_SPD_H01_Z1_AXIS_DOT_WEIGHT_POS_TIMEOUT);
                                    break;
                                }
                                if (Z3Axis.IsOnPosition(Z3AxisWeightingPos) == false && _machineStatus.IsSkipHead3)
                                {
                                    RaiseWarning(EWarning.MO_SPD_H03_Z3_AXIS_DOT_WEIGHT_POS_TIMEOUT);
                                    break;
                                }
                            }

                            if (currentDotWeightingHead == EDotWeightingHead.HEAD24)
                            {
                                if (Z2Axis.IsOnPosition(Z2AxisWeightingPos) == false && _machineStatus.IsSkipHead2)
                                {
                                    RaiseWarning(EWarning.MO_SPD_H02_Z2_AXIS_DOT_WEIGHT_POS_TIMEOUT);
                                    break;
                                }
                                if (Z4Axis.IsOnPosition(Z4AxisWeightingPos) == false && _machineStatus.IsSkipHead4)
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
            return sequence == ESequence.HeadAssemble_Step1 ||
                   sequence == ESequence.HeadAssemble_Step1_H1 || sequence == ESequence.HeadAssemble_Step1_H2 ||
                   sequence == ESequence.HeadAssemble_Step1_H3 || sequence == ESequence.HeadAssemble_Step1_H4 ||
                   sequence == ESequence.HeadAssemble_Step2 ||
                   sequence == ESequence.HeadAssemble_Step2_H1 || sequence == ESequence.HeadAssemble_Step2_H2 ||
                   sequence == ESequence.HeadAssemble_Step2_H3 || sequence == ESequence.HeadAssemble_Step2_H4 ||
                   sequence == ESequence.HeadDisassemble ||
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
            Z1Axis.MoveAbs(Z1AxisSafetyPos);
            Z2Axis.MoveAbs(Z2AxisSafetyPos);
            Z3Axis.MoveAbs(Z3AxisSafetyPos);
            Z4Axis.MoveAbs(Z4AxisSafetyPos);
        }

        private bool AllZAxisInSafetyPos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = Z1Axis.Status.ActualPosition >= Z1AxisSafetyPos;
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = Z2Axis.Status.ActualPosition >= Z2AxisSafetyPos;
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = Z3Axis.Status.ActualPosition >= Z3AxisSafetyPos;
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = Z4Axis.Status.ActualPosition >= Z4AxisSafetyPos;
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisInjectUpDistancePosMove()
        {
            if (!_currentRecipe.OptionRecipe.SkipHead12 && !_optionRecipe.SkipHead1)
            {
                Log.Debug($"Z1 Axis Move Inject Up Distance Position [{Z1AxisInjectPos + Z1UpDistancePos}]");
                Z1Axis.MoveAbs(Z1AxisInjectPos + Z1UpDistancePos);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead12 && !_optionRecipe.SkipHead2)
            {
                Log.Debug($"Z2 Axis Move Inject Up Distance Position [{Z2AxisInjectPos + Z2UpDistancePos}]");
                Z2Axis.MoveAbs(Z2AxisInjectPos + Z2UpDistancePos);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead34 && !_optionRecipe.SkipHead3)
            {
                Log.Debug($"Z3 Axis Move Inject Up Distance Position [{Z3AxisInjectPos + Z3UpDistancePos}]");
                Z3Axis.MoveAbs(Z3AxisInjectPos + Z3UpDistancePos);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead34 && !_optionRecipe.SkipHead4)
            {
                Log.Debug($"Z4 Axis Move Inject Up Distance Position [{Z4AxisInjectPos + Z4UpDistancePos}]");
                Z4Axis.MoveAbs(Z4AxisInjectPos + Z4UpDistancePos);
            }
        }

        private bool AllZAxisInInjectUpDistancePos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1 || Z1Axis.IsOnPosition(Z1AxisInjectPos + Z1UpDistancePos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2 || Z2Axis.IsOnPosition(Z2AxisInjectPos + Z2UpDistancePos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3 || Z3Axis.IsOnPosition(Z3AxisInjectPos + Z3UpDistancePos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4 || Z4Axis.IsOnPosition(Z4AxisInjectPos + Z4UpDistancePos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisInjectPosMove()
        {
            if (!_currentRecipe.OptionRecipe.SkipHead12 && !_optionRecipe.SkipHead1)
            {
                Log.Debug($"Z1 Axis Move Inject Position [{Z1AxisInjectPos}], Speed [{Z1SlowSpeed}]");
                Z1Axis.MoveAbs(Z1AxisInjectPos, Z1SlowSpeed);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead12 && !_optionRecipe.SkipHead2)
            {
                Log.Debug($"Z2 Axis Move Inject Position [{Z2AxisInjectPos}], Speed [{Z2SlowSpeed}]");
                Z2Axis.MoveAbs(Z2AxisInjectPos, Z2SlowSpeed);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead34 && !_optionRecipe.SkipHead3)
            {
                Log.Debug($"Z3 Axis Move Inject Position [{Z3AxisInjectPos}], Speed [{Z3SlowSpeed}]");
                Z3Axis.MoveAbs(Z3AxisInjectPos, Z3SlowSpeed);
            }
            if (!_currentRecipe.OptionRecipe.SkipHead34 && !_optionRecipe.SkipHead4)
            {
                Log.Debug($"Z4 Axis Move Inject Position [{Z4AxisInjectPos}], Speed [{Z4SlowSpeed}]");
                Z4Axis.MoveAbs(Z4AxisInjectPos, Z4SlowSpeed);
            }
        }

        private bool AllZAxisInInjectPos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1 || Z1Axis.IsOnPosition(Z1AxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2 || Z2Axis.IsOnPosition(Z2AxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3 || Z3Axis.IsOnPosition(Z3AxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4 || Z4Axis.IsOnPosition(Z4AxisInjectPos);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisAddTailPosMove()
        {
            if (!_currentRecipe.OptionRecipe.SkipHead12)
                Z1Axis.MoveAbs(Z1AxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            if (!_currentRecipe.OptionRecipe.SkipHead12)
                Z2Axis.MoveAbs(Z2AxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            if (!_currentRecipe.OptionRecipe.SkipHead34)
                Z3Axis.MoveAbs(Z3AxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            if (!_currentRecipe.OptionRecipe.SkipHead34)
                Z4Axis.MoveAbs(Z4AxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
        }

        private bool AllZAxisInAddTailPos(ref ESPDHead failHead)
        {
            bool result = true;
            bool ret = false;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(Z1AxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead1;

            ret = _currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(Z2AxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead2;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(Z3AxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead3;

            ret = _currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(Z4AxisInjectPos + _currentRecipe.AdditionalMolding_Recipe.ZUpDistanceAddTail);
            result &= ret;
            if (!ret) failHead = ESPDHead.SPDHead4;

            return result;
        }

        private void ZAxisNeedleCleanPosMove(ESPDHead head)
        {
            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All)
            {
                if (_machineStatus.IsSkipHead1 == false)
                    Z1Axis.MoveAbs(Z1AxisNeedleCleanPos);
                if (_machineStatus.IsSkipHead2 == false)
                    Z2Axis.MoveAbs(Z2AxisNeedleCleanPos);
                if (_machineStatus.IsSkipHead3 == false)
                    Z3Axis.MoveAbs(Z3AxisNeedleCleanPos);
                if (_machineStatus.IsSkipHead4 == false)
                    Z4Axis.MoveAbs(Z4AxisNeedleCleanPos);
            }
            else
            {
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead1 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    Z1Axis.MoveAbs(Z1AxisNeedleCleanPos);
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead2 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    Z2Axis.MoveAbs(Z2AxisNeedleCleanPos);
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead3 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    Z3Axis.MoveAbs(Z3AxisNeedleCleanPos);
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead4 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
                    Z4Axis.MoveAbs(Z4AxisNeedleCleanPos);
            }
        }

        private bool ZAxisInNeedleCleanPos(ESPDHead head)
        {
            if (head == ESPDHead.SPDHead1)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1 || Z1Axis.IsOnPosition(Z1AxisNeedleCleanPos);
            }
            if (head == ESPDHead.SPDHead2)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2 || Z2Axis.IsOnPosition(Z2AxisNeedleCleanPos);
            }
            if (head == ESPDHead.SPDHead3)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3 || Z3Axis.IsOnPosition(Z3AxisNeedleCleanPos);
            }
            if (head == ESPDHead.SPDHead4)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4 || Z4Axis.IsOnPosition(Z4AxisNeedleCleanPos);
            }
            return true;
        }

        private bool AllZAxisInNeedleCleanPos(ref ESPDHead failHead, ESPDHead head)
        {
            bool result = true;
            bool ret = false;

            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All)
            {
                ret = _machineStatus.IsSkipHead1 || Z1Axis.IsOnPosition(Z1AxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead1;

                ret = _machineStatus.IsSkipHead2 || Z2Axis.IsOnPosition(Z2AxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead2;

                ret = _machineStatus.IsSkipHead3 || Z3Axis.IsOnPosition(Z3AxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead3;

                ret = _machineStatus.IsSkipHead4 || Z4Axis.IsOnPosition(Z4AxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead4;
            }
            else
            {
                ret = _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1 || Z1Axis.IsOnPosition(Z1AxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead1;

                ret = _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2 || Z2Axis.IsOnPosition(Z2AxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead2;

                ret = _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3 || Z3Axis.IsOnPosition(Z3AxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead3;

                ret = _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4 || Z4Axis.IsOnPosition(Z4AxisNeedleCleanPos);
                result &= ret;
                if (!ret) failHead = ESPDHead.SPDHead4;
            }

            return result;

        }

        private void ZAxisDummyPosMove(ESequence sequence, ESPDHead head)
        {
            if (sequence == ESequence.IdlePurge)
            {
                Z1Axis.MoveAbs(Z1AxisDummyPos);
                Z2Axis.MoveAbs(Z2AxisDummyPos);
                Z3Axis.MoveAbs(Z3AxisDummyPos);
                Z4Axis.MoveAbs(Z4AxisDummyPos);
            }

            if (Parent!.Sequence != ESequence.AutoRun && head == ESPDHead.All && sequence != ESequence.IdlePurge)
            {
                if (_machineStatus.IsSkipHead1 == false)
                    Z1Axis.MoveAbs(Z1AxisDummyPos);
                if (_machineStatus.IsSkipHead2 == false)
                    Z2Axis.MoveAbs(Z2AxisDummyPos);
                if (_machineStatus.IsSkipHead3 == false)
                    Z3Axis.MoveAbs(Z3AxisDummyPos);
                if (_machineStatus.IsSkipHead4 == false)
                    Z4Axis.MoveAbs(Z4AxisDummyPos);
            }
            else
            {
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead1 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    Z1Axis.MoveAbs(Z1AxisDummyPos);
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead2 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    Z2Axis.MoveAbs(Z2AxisDummyPos);
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead3 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    Z3Axis.MoveAbs(Z3AxisDummyPos);
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead4 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
                    Z4Axis.MoveAbs(Z4AxisDummyPos);
            }
        }

        private bool ZAxisInDummyPos(ESequence sequence, ESPDHead head)
        {
            if (sequence == ESequence.IdlePurge)
            {
                return Z1Axis.IsOnPosition(Z1AxisDummyPos) &&
                        Z2Axis.IsOnPosition(Z2AxisDummyPos) &&
                        Z3Axis.IsOnPosition(Z3AxisDummyPos) &&
                        Z4Axis.IsOnPosition(Z4AxisDummyPos);
            }

            if (head == ESPDHead.All && Parent!.Sequence == ESequence.AutoRun)
            {
                return
                    (_currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1 || Z1Axis.IsOnPosition(Z1AxisDummyPos)) &&
                    (_currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2 || Z2Axis.IsOnPosition(Z2AxisDummyPos)) &&
                    (_currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3 || Z3Axis.IsOnPosition(Z3AxisDummyPos)) &&
                    (_currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4 || Z4Axis.IsOnPosition(Z4AxisDummyPos));
            }

            if (head == ESPDHead.All && Parent!.Sequence != ESequence.AutoRun)
            {
                return
                    (_machineStatus.IsSkipHead1 || Z1Axis.IsOnPosition(Z1AxisDummyPos)) &&
                    (_machineStatus.IsSkipHead2 || Z2Axis.IsOnPosition(Z2AxisDummyPos)) &&
                    (_machineStatus.IsSkipHead3 || Z3Axis.IsOnPosition(Z3AxisDummyPos)) &&
                    (_machineStatus.IsSkipHead4 || Z4Axis.IsOnPosition(Z4AxisDummyPos));
            }

            if (head == ESPDHead.SPDHead1)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || Z1Axis.IsOnPosition(Z1AxisDummyPos);
            }

            if (head == ESPDHead.SPDHead2)
            {
                return _currentRecipe.OptionRecipe.SkipHead12 || Z2Axis.IsOnPosition(Z2AxisDummyPos);
            }

            if (head == ESPDHead.SPDHead3)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || Z3Axis.IsOnPosition(Z3AxisDummyPos);
            }

            if (head == ESPDHead.SPDHead4)
            {
                return _currentRecipe.OptionRecipe.SkipHead34 || Z4Axis.IsOnPosition(Z4AxisDummyPos);
            }

            return true;
        }

        private void ZAxisAssembleDisassemblePosMove(ESPDHead head)
        {
            if (head == ESPDHead.All)
            {
                if (_machineStatus.IsSkipHead1 == false)
                    Z1Axis.MoveAbs(Z1AxisAssembleDisassemblePos);
                if (_machineStatus.IsSkipHead2 == false)
                    Z2Axis.MoveAbs(Z2AxisAssembleDisassemblePos);
                if (_machineStatus.IsSkipHead3 == false)
                    Z3Axis.MoveAbs(Z3AxisAssembleDisassemblePos);
                if (_machineStatus.IsSkipHead4 == false)
                    Z4Axis.MoveAbs(Z4AxisAssembleDisassemblePos);
            }
            if (head == ESPDHead.SPDHead1)
                Z1Axis.MoveAbs(Z1AxisAssembleDisassemblePos);
            if (head == ESPDHead.SPDHead2)
                Z2Axis.MoveAbs(Z2AxisAssembleDisassemblePos);
            if (head == ESPDHead.SPDHead3)
                Z3Axis.MoveAbs(Z3AxisAssembleDisassemblePos);
            if (head == ESPDHead.SPDHead4)
                Z4Axis.MoveAbs(Z4AxisAssembleDisassemblePos);
        }

        private bool ZAxisInAssembleDisassemblePos(ESPDHead head)
        {
            if (head == ESPDHead.All)
            {
                return
                    (_machineStatus.IsSkipHead1 || Z1Axis.IsOnPosition(Z1AxisAssembleDisassemblePos)) &&
                    (_machineStatus.IsSkipHead2 || Z2Axis.IsOnPosition(Z2AxisAssembleDisassemblePos)) &&
                    (_machineStatus.IsSkipHead3 || Z3Axis.IsOnPosition(Z3AxisAssembleDisassemblePos)) &&
                    (_machineStatus.IsSkipHead4 || Z4Axis.IsOnPosition(Z4AxisAssembleDisassemblePos));
            }

            if (head == ESPDHead.SPDHead1)
            {
                return Z1Axis.IsOnPosition(Z1AxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead2)
            {
                return Z2Axis.IsOnPosition(Z2AxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead3)
            {
                return Z3Axis.IsOnPosition(Z3AxisAssembleDisassemblePos);
            }

            if (head == ESPDHead.SPDHead4)
            {
                return Z4Axis.IsOnPosition(Z4AxisAssembleDisassemblePos);
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
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead1 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    NozzleClean_H1.Grip();
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead2 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    NozzleClean_H2.Grip();
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead3 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    NozzleClean_H3.Grip();
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead4 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
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
                    return (_currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1 || NozzleClean_H1.IsGrip()) &&
                        (_currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2 || NozzleClean_H2.IsGrip()) &&
                        (_currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3 || NozzleClean_H3.IsGrip()) &&
                        (_currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4 || NozzleClean_H4.IsGrip());
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
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead1 == false && (head == ESPDHead.SPDHead1 || head == ESPDHead.All))
                    NozzleClean_H1.Ungrip();
                if (_currentRecipe.OptionRecipe.SkipHead12 == false && _optionRecipe.SkipHead2 == false && (head == ESPDHead.SPDHead2 || head == ESPDHead.All))
                    NozzleClean_H2.Ungrip();
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead3 == false && (head == ESPDHead.SPDHead3 || head == ESPDHead.All))
                    NozzleClean_H3.Ungrip();
                if (_currentRecipe.OptionRecipe.SkipHead34 == false && _optionRecipe.SkipHead4 == false && (head == ESPDHead.SPDHead4 || head == ESPDHead.All))
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

                return (_currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1 || NozzleClean_H1.IsUngrip()) &&
                       (_currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2 || NozzleClean_H2.IsUngrip()) &&
                       (_currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3 || NozzleClean_H3.IsUngrip()) &&
                       (_currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4 || NozzleClean_H4.IsUngrip());
            }

            if (head == ESPDHead.SPDHead1) return _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1 || NozzleClean_H1.IsUngrip();
            if (head == ESPDHead.SPDHead2) return _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2 || NozzleClean_H2.IsUngrip();
            if (head == ESPDHead.SPDHead3) return _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3 || NozzleClean_H3.IsUngrip();
            return _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4 || NozzleClean_H4.IsUngrip();
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
                if (sequence == ESequence.IdlePurge)
                {
                    return procInputs[EInjectProcInput.SPDHead1_WorkDone].Value &&
                            procInputs[EInjectProcInput.SPDHead2_WorkDone].Value &&
                            procInputs[EInjectProcInput.SPDHead3_WorkDone].Value &&
                            procInputs[EInjectProcInput.SPDHead4_WorkDone].Value;
                }
                else if (Parent!.Sequence == ESequence.AutoRun)
                {
                    return
                    (procInputs[EInjectProcInput.SPDHead1_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1) &&
                    (procInputs[EInjectProcInput.SPDHead2_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2) &&
                    (procInputs[EInjectProcInput.SPDHead3_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3) &&
                    (procInputs[EInjectProcInput.SPDHead4_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4);
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
                ESPDHead.SPDHead1 => procInputs[EInjectProcInput.SPDHead1_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead1,
                ESPDHead.SPDHead2 => procInputs[EInjectProcInput.SPDHead2_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead12 || _optionRecipe.SkipHead2,
                ESPDHead.SPDHead3 => procInputs[EInjectProcInput.SPDHead3_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead3,
                ESPDHead.SPDHead4 => procInputs[EInjectProcInput.SPDHead4_WorkDone].Value || _currentRecipe.OptionRecipe.SkipHead34 || _optionRecipe.SkipHead4,
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

        private void UpdateBothJigStatus(EJigStatus status, bool isLeftJig)
        {
            JigStatuses[isLeftJig ? (int)EJig.JigLeft : (int)EJig.JigRight] = status;
        }

        private void UpdateAllJigStatus(EJigStatus status)
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

        private string GetCellId(bool isJigLeft)
        {
            int[] iReciveData = new int[20 / 2];
            int iiReciveDataLength = 20 * 2;

            MCCLinkIE.mdreceiveex(151, 0, 255, 24, isJigLeft ? 0xCCC8 : 0xCD34, ref iiReciveDataLength, ref iReciveData[0]);

            short[] buffer = new short[20];
            for (int i = 0; i < iReciveData.Length; i++)
            {
                string sHex = Convert.ToString(iReciveData[i], 16).ToUpper().PadLeft(8, '0');
                buffer[0 + (2 * i)] = (short)Convert.ToInt32(sHex.Substring(4, 4), 16);
                buffer[1 + (2 * i)] = (short)Convert.ToInt32(sHex.Substring(0, 4), 16);
            }

            string message = "";
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i] == 0) continue;
                message += System.Text.Encoding.ASCII.GetString(BitConverter.GetBytes(buffer[i]));
            }
            return message.Trim();
        }

        private bool CIMValidation(bool isJigLeft)
        {
            if (_recipeSelector.CurrentRecipe.OptionRecipe.UseCIM == false ||
                _machineStatus.IsDryRunMode ||
                _machineStatus.IsPassRunMode)
            {
                return true;
            }

            string outCellID = "";

            string jigID = isJigLeft ? LeftJigID : RightJigID;

            Log.Info($"Specific Validationing JIG '{jigID}'");
            if (EquipEventHelpers.SpecificValidation(isJigLeft ? 1 : 2, jigID, ref outCellID) == false)
            {
                Log.Error("CIM_JIG_SPECIFIC_VALIDATION_FAIL");
                return false;
            }

            Log.Info($"Specific Validation JIG '{jigID}' successed");

            if (isJigLeft)
            {
                LeftCellId = outCellID;
            }
            else
            {
                RightCellId = outCellID;
            }

            string cellId = isJigLeft ? LeftCellId : RightCellId;

            Log.Info($"Cell ID '{cellId}' tracking");
            if (EquipEventHelpers.CellTrackIn(isJigLeft ? 1 : 2, isJigLeft ? LeftCellId : RightCellId, 9, _cIMCollection.CimFunction.CellTrackingState == OnOffNothing.ON ? 2 : 3) == false)
            {
                Log.Error("CIM_CELL_TRACKING_FAIL");
                return false;
            }
            Log.Info($"Cell ID '{cellId}' tracking successed");


            Log.Info($"Cell ID '{cellId}' Job Process Start");
            CellJobProcessCimToPlcArea cellJobProcess = EquipEventHelpers.CellJobProcessConfirm(isJigLeft ? 1 : 2, cellId, _cIMCollection.CimFunction.CellTrackingState != OnOffNothing.ON);

            if (isJigLeft)
            {
                CellJobProcessInfo_Left = cellJobProcess;
            }
            else
            {
                CellJobProcessInfo_Right = cellJobProcess;
            }

            if (_cIMCollection.CimFunction.CellTrackingState != OnOffNothing.ON)
            {
                if (isJigLeft)
                {
                    CellJobProcessInfo_Left.CellJobProcessCellID = LeftCellId;
                }
                else
                {
                    CellJobProcessInfo_Right.CellJobProcessCellID = RightCellId;
                }
            }

            if (_cIMCollection.CimFunction.CellTrackingState == OnOffNothing.ON && cellJobProcess.CellJobProcessRCMD != $"{(int)ECellJobProcessRCMD.CellJobProcessStart}")
            {        
                if (isJigLeft)
                {
                    CellJobProcessInfo_Left.UserDefine_TrackOutJudge = "O";
                    CellJobProcessInfo_Left.UserDefine_TrackOutDescription = "VALIDATION_FAIL";
                    EquipEventHelpers.CellTrackOut(1, CellJobProcessInfo_Left, true, new List<MaterialAssemblyPlcToCimArea> { MaterialAssemblyArea_Left_1, MaterialAssemblyArea_Left_2 });
                }
                else
                {
                    CellJobProcessInfo_Right.UserDefine_TrackOutJudge = "O";
                    CellJobProcessInfo_Right.UserDefine_TrackOutDescription = "VALIDATION_FAIL";
                    EquipEventHelpers.CellTrackOut(2, CellJobProcessInfo_Right, true, new List<MaterialAssemblyPlcToCimArea> { MaterialAssemblyArea_Right_1, MaterialAssemblyArea_Right_2 });
                }

                Log.Error("CIM_CELL_JOB_PROCESS_START_FAIL");
                return false;
            }

            Log.Info($"Cell ID '{cellId}' Job Process Start Successed");
            return true;
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
                Log.Debug($"Move {XAxis.Name} to dot weighting for head 1, 3 pos [{XAxisH13DotWeightingPos}mm]");
                XAxis.MoveAbs(XAxisH13DotWeightingPos);
            }
            else
            {
                Log.Debug($"Move {XAxis.Name} to dot weighting for head 2, 4 pos [{XAxisH24DotWeightingPos}mm]");
                XAxis.MoveAbs(XAxisH24DotWeightingPos);
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
                Z1Axis.MoveAbs(Z1AxisWeightingPos);
            if (head == ESPDHead.SPDHead2)
                Z2Axis.MoveAbs(Z2AxisWeightingPos);
            if (head == ESPDHead.SPDHead3)
                Z3Axis.MoveAbs(Z3AxisWeightingPos);
            if (head == ESPDHead.SPDHead4)
                Z4Axis.MoveAbs(Z4AxisWeightingPos);
            if (head == ESPDHead.All)
            {
                if (currentDotWeightingHead == EDotWeightingHead.HEAD13)
                {
                    if (_machineStatus.IsSkipHead1 == false)
                    {
                        Z1Axis.MoveAbs(Z1AxisWeightingPos);
                    }
                    if (_machineStatus.IsSkipHead3 == false)
                    {
                        Z3Axis.MoveAbs(Z3AxisWeightingPos);
                    }
                }
                if (currentDotWeightingHead == EDotWeightingHead.HEAD24)
                {
                    if (_machineStatus.IsSkipHead2 == false)
                    {
                        Z2Axis.MoveAbs(Z2AxisWeightingPos);
                    }
                    if (_machineStatus.IsSkipHead4 == false)
                    {
                        Z4Axis.MoveAbs(Z4AxisWeightingPos);
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
                    return (Z1Axis.IsOnPosition(Z1AxisWeightingPos) || _machineStatus.IsSkipHead1) &&
                        (Z3Axis.IsOnPosition(Z3AxisWeightingPos) || _machineStatus.IsSkipHead3);
                }

                return (Z2Axis.IsOnPosition(Z2AxisWeightingPos) || _machineStatus.IsSkipHead2) &&
                        (Z4Axis.IsOnPosition(Z4AxisWeightingPos) || _machineStatus.IsSkipHead4);
            }
            if (head == ESPDHead.SPDHead1)
            {
                return Z1Axis.IsOnPosition(Z1AxisWeightingPos);
            }
            if (head == ESPDHead.SPDHead2)
            {
                return Z2Axis.IsOnPosition(Z2AxisWeightingPos);
            }
            if (head == ESPDHead.SPDHead3)
            {
                return Z3Axis.IsOnPosition(Z3AxisWeightingPos);
            }

            return Z4Axis.IsOnPosition(Z4AxisWeightingPos);
        }

        private void DisableChamberOpenClose()
        {
            if (Out_ChamberOpen == true || Out_ChamberClose == true)
            {
                Log.Debug("Dissable Open Close Chamber");
                Out_ChamberClose = false;
                Out_ChamberOpen = false;
            }

        }

        private void WriteMCC(EMCCAction lastAction, EMCCAction action)
        {
            _cIMCollection.WriteMCC(lastAction, action);
            LastAction = action;
        }

        private void WriteMCC(EMCCAction lastAction, EMCCAction action, EMCCUnit unit, string jigId)
        {
            _cIMCollection.WriteMCC(lastAction, action, unit, jigId);
            LastAction = action;
        }
        #endregion

        #region Privates
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        private OptionRecipe _optionRecipe => _currentRecipe.OptionRecipe;
        private InjectRecipe Recipe => _currentRecipe.InjectRecipe;
        private TactTime TactTime => _tactTimeList.Chamber;

        private bool IsFirstCycle { get; set; } = true;
        private readonly Devices _devices;
        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
        private readonly TactTimeList _tactTimeList;
        private readonly ICIMMapHelper _mapHelper;
        private readonly ProductionService _productionService;
        private readonly InOutHandler _inOutHandler;
        private readonly PositionList _positionList;
        private readonly CIMCollection _cIMCollection;
        private int inputCount;
        private int outputCount;
        private int NeedleCleanCount = 0;
        private int _nzlCleanCount = 0;
        private double _yAxisCleaningTargetMm;
        private double _waitTimeNoProduct;
        private bool isJigLeftLoaded;
        private bool isJigRightLoaded;
        private bool isLoadingDry;
        private ESPDHead _failHead;
        private EMCCAction LastAction;
        private EDotWeightingHead currentDotWeightingHead;
        private Queue<EMoldProcResinInjectStep> MoldResinInjectSteps = new Queue<EMoldProcResinInjectStep>();
        Random random = new Random();
        #endregion
    }
}
