using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.InOut;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Process
{
    public class DryPumpProcess : MIProcess
    {
        #region Inputs
        private IDInput In_DryPumpRun => _devices.Inputs.DryPumpRun;
        #endregion

        #region Outputs
        private IDOutput Out_DryPumpRun => _devices.Outputs.DryPumpRun;
        private IDOutput Out_ChamberPurgeOn => _devices.Outputs.ChamberPurgeOn;
        #endregion

        #region AInputs
        #endregion

        #region Process IOs
        private IDInputDevice<EDryPumpProcInput> procInputs;
        private IDOutputDevice<EDryPumpProcOutput> procOutputs;
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
        private ICylinder AngleValve => _devices.Cylinders.AngleValve;
        #endregion

        #region Constructors
        public DryPumpProcess(Devices devices, RecipeSelector recipeSelector,
            ProcessIO processIO, MachineStatus machineStatus,
            CarrierJigStatusList carrierJigStatusList)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            _machineStatus = machineStatus;
            _carrierJigStatusList = carrierJigStatusList;
            procInputs = processIO.DryPumpProcInput;
            procOutputs = processIO.DryPumpProcOutput;
        }
        #endregion

        #region Process Methods
        public override bool ProcessToRun()
        {
            switch ((EDryPumpProcToRunStep)Step.ToRunStep)
            {
                case EDryPumpProcToRunStep.Start:
                    Log.Debug("ToRun start");
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.AngleValve_Close:
                    if (AngleValve.IsClose())
                    {
                        Step.ToRunStep = (int)EDryPumpProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"Closing {AngleValve.Name}");
                    AngleValve.Close();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => AngleValve.IsClose());
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.AngleValve_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.AngleValve_CloseFail);
                        break;
                    }

                    Log.Debug($"{AngleValve.Name} closed");
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.DryPump_Run:
                    if (In_DryPumpRun.Value)
                    {
                        Step.ToRunStep = (int)EDryPumpProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"{Out_DryPumpRun.Name} run");
                    Out_DryPumpRun.Value = true;
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.End:
                    procOutputs.ClearOutputs();

                    Log.Debug("ToRun end");
                    Step.ToRunStep++;
                    base.ProcessToRun();
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
            switch ((EDryPumpProcOriginStep)Step.OriginStep)
            {
                case EDryPumpProcOriginStep.Start:
                    Step.OriginStep++;
                    break;
                case EDryPumpProcOriginStep.End:
                    Step.OriginStep++;
                    base.ProcessOrigin();
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
                    Sequence_ResinInject();
                    break;
                case ESequence.ResinInject:
                    Sequence_ResinInject();
                    break;
                default:
                    Sequence = ESequence.Stop;
                    break;
            }
            return true;
        }
        #endregion

        #region Sequence Methods
        private void Sequence_Ready()
        {
            switch ((EDryPumpProcReadyStep)Step.RunStep)
            {
                case EDryPumpProcReadyStep.Start:
                    Log.Info("Ready start");
                    Step.RunStep++;
                    break;
                case EDryPumpProcReadyStep.ZAxis_ReadyPos_Move:
                    if (AllZAxisInSafetyPos(ref _failHead))
                    {
                        Log.Debug($"ZAxis is on safety position already");
                        Step.RunStep = (int)EDryPumpProcReadyStep.XYAxis_ReadyPos_Move;
                        break;
                    }

                    Log.Debug($"Moving Z-Axes to safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => AllZAxisInSafetyPos(ref _failHead));
                    Step.RunStep++;
                    break;
                case EDryPumpProcReadyStep.ZAxis_ReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut, _failHead);
                        break;
                    }

                    Log.Debug($"Z-Axes move to safety position done");
                    Step.RunStep++;
                    break;
                case EDryPumpProcReadyStep.XYAxis_ReadyPos_Move:
                    XAxis.MoveAbs(_currentRecipe.InjectRecipe.XAxisReadyPos);
                    YAxis.MoveAbs(_currentRecipe.InjectRecipe.YAxisReadyPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisReadyPos) &&
                              YAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos));
                    Step.RunStep++;
                    break;
                case EDryPumpProcReadyStep.XYAxis_ReadyPos_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.InjectRecipe.XAxisReadyPos))
                            RaiseWarning(EWarning.XAxis_ReadyPos_MoveTimeOut);
                        else
                            RaiseWarning(EWarning.YAxis_ReadyPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug($"{XAxis.Name}|{YAxis.Name} move to ready pos done");
                    Step.RunStep++;
                    break;
                case EDryPumpProcReadyStep.End:
                    Log.Debug("Ready run end");
                    Sequence = ESequence.Stop;
                    break;
                default:
                    Wait(20);
                    break;
            }

        }

        private void Sequence_ResinInject()
        {
            switch ((EDryPumpProcResinInjectStep)Step.RunStep)
            {
                case EDryPumpProcResinInjectStep.Start:
                    Log.Info("ResinInject start");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Run:
                    if (In_DryPumpRun.Value)
                    {
                        Step.RunStep = (int)EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait;
                        break;
                    }

                    Log.Debug($"{Out_DryPumpRun.Name} run");
                    Out_DryPumpRun.Value = true;
                    Wait(10000, () => In_DryPumpRun.Value);
#if SIMULATION
                    SimulationInputSetter.SetSimInput(_devices.Inputs.DryPumpRun, true);
#endif

                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_RunWait:
                    if (WaitTimeOutOccurred &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        RaiseWarning(EWarning.DryPump_Run_Timeout);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait:
                    if (procInputs[EDryPumpProcInput.Vacuum_WorkRequest].Value == false)
                    {
                        break;
                    }

                    Log.Info($"Input detect {EDryPumpProcInput.Vacuum_WorkRequest}");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_Open:
                    Log.Debug($"Opening {AngleValve.Name}");
                    AngleValve.Open();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, AngleValve.IsOpen);
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_OpenWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.AngleValve_OpenFail);
                        break;
                    }

                    Log.Debug($"{AngleValve.Name} Opened");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.VacuumGauge_SpecIn_Wait:
                    if (_devices.AnalogInputs.VacuumPressureInTorr > _currentRecipe.DryPumpRecipe.VacuumPressureSpec &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Info($"DryPump vacuum in-spec {_devices.AnalogInputs.VacuumPressureInTorr} Torr");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_Close:
                    AngleValve.Close();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, AngleValve.IsClose);
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.AngleValve_CloseFail);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_VacuumDone_Send:
                    procOutputs[EDryPumpProcOutput.ChamberVacuumSuccess].Value = true;
                    Log.Info($"Set output {EDryPumpProcOutput.ChamberVacuumSuccess}");
                    Log.Debug("Wait Vent");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_WaitVentTime:
                    if (((_currentRecipe.InjectRecipe.InjectTime - _carrierJigStatusList.CarrierJigStatusH1.InjectTime > _currentRecipe.InjectRecipe.VentTime) && !_currentRecipe.SPDHead1_Recipe.HeadSkip) ||
                        ((_currentRecipe.InjectRecipe.InjectTime - _carrierJigStatusList.CarrierJigStatusH2.InjectTime > _currentRecipe.InjectRecipe.VentTime) && !_currentRecipe.SPDHead2_Recipe.HeadSkip) ||
                        ((_currentRecipe.InjectRecipe.InjectTime - _carrierJigStatusList.CarrierJigStatusH3.InjectTime > _currentRecipe.InjectRecipe.VentTime) && !_currentRecipe.SPDHead3_Recipe.HeadSkip) ||
                        ((_currentRecipe.InjectRecipe.InjectTime - _carrierJigStatusList.CarrierJigStatusH4.InjectTime > _currentRecipe.InjectRecipe.VentTime) && !_currentRecipe.SPDHead4_Recipe.HeadSkip))
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("Vent start");
                    _ventStartTick = Environment.TickCount;
                    Out_ChamberPurgeOn.Value = true;
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_PurgeAndWait:
                    if (((Environment.TickCount - _ventStartTick) / 1000.0) < _currentRecipe.InjectRecipe.InjectTime)
                    {
                        Wait(10);
                        break;
                    }

                    Log.Debug($"Vent Complete: {_currentRecipe.InjectRecipe.VentTime}s");
                    procOutputs[EDryPumpProcOutput.VentComplete].Value = true;
                    Out_ChamberPurgeOn.Value = false;
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.WaitToClear_ProcOutput:
                    if (procInputs[EDryPumpProcInput.Vacuum_WorkRequest].Value == true)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Info($"Clear output {EDryPumpProcOutput.ChamberVacuumSuccess}");
                    procOutputs[EDryPumpProcOutput.ChamberVacuumSuccess].Value = false;
                    procOutputs[EDryPumpProcOutput.VentComplete].Value = false;
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("ResinInject end, starting new cycle");
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.DryPump_Run;
                    break;
            }
        }
        #endregion

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

        private void RaiseHeadWarning(EWarning warning, ESPDHead _failHead)
        {
            RaiseWarning(warning + ((int)(EWarning.Z2Axis_Origin_TimeOut - EWarning.Z1Axis_Origin_TimeOut)) * (_failHead - ESPDHead.SPDHead1));
        }

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private readonly MachineStatus _machineStatus;
        private readonly CarrierJigStatusList _carrierJigStatusList;

        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        private ESPDHead _failHead;
        private double _ventStartTick;
        #endregion
    }
}
