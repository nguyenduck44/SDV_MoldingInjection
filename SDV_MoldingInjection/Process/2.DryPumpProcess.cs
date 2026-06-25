using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.InOut;
using Microsoft.Extensions.Configuration;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.MVVM.Models;
using SDV_MoldingInjection.Recipe;
using System.IO;
using WinRT;

namespace SDV_MoldingInjection.Process
{
    public class DryPumpProcess : MIProcess
    {
        #region Inputs
        private IDInput In_DryPumpRun => _devices.Inputs.DryPumpRun;
        private IDInput In_ChamberPurgeOn => _devices.Inputs.ChamberPurgeOn;
        #endregion

        #region Outputs
        private IDOutput Out_DryPumpRun => _devices.Outputs.DryPumpRun;
        private IDOutput Out_ChamberPurgeOn => _devices.Outputs.ChamberPurgeOn;
        private IDOutput Out_DryPumpResetAlarm => _devices.Outputs.DryPumpAlarmReset;

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
        private ICylinder BellowCyl => _devices.Cylinders.BellowCyl;
        #endregion

        #region Properties
        private EProcessMode SPDHead1ProcessMode => Parent!.Childs!.OfType<SPDHeadProcess>().First(p => p.Name == EProcess.SPDHead1.ToString()).ProcessMode;
        private EProcessMode SPDHead2ProcessMode => Parent!.Childs!.OfType<SPDHeadProcess>().First(p => p.Name == EProcess.SPDHead2.ToString()).ProcessMode;
        private EProcessMode SPDHead3ProcessMode => Parent!.Childs!.OfType<SPDHeadProcess>().First(p => p.Name == EProcess.SPDHead3.ToString()).ProcessMode;
        private EProcessMode SPDHead4ProcessMode => Parent!.Childs!.OfType<SPDHeadProcess>().First(p => p.Name == EProcess.SPDHead4.ToString()).ProcessMode;
        #endregion

        #region Constructors
        public DryPumpProcess(Devices devices, 
            RecipeSelector recipeSelector,
            ProcessIO processIO, 
            MachineStatus machineStatus,
            CarrierJigStatusList carrierJigStatusList,
            IConfiguration configuration,
            TactTimeList tactTimeList,
            Plotter plotter,
            CIMCollection cIMCollection)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            _machineStatus = machineStatus;
            _carrierJigStatusList = carrierJigStatusList;
            _configuration = configuration;
            _tactTimeList = tactTimeList;
            _plotter = plotter;
            _cIMCollection = cIMCollection;
            procInputs = processIO.DryPumpProcInput;
            procOutputs = processIO.DryPumpProcOutput;
        }
        #endregion

        #region Process Methods
        public override bool PreProcess()
        {
            if (_delayStartTick >= 0 && EnableTimerDelay)
            {
                _delayTime = (Environment.TickCount64 - _delayStartTick) / 1000.0;
            }

            if (_currentRecipe.OptionRecipe.SkipHead12 == false)
            {
                _carrierJigStatusList.CarrierJigStatusH1.DelayTime = _delayTime;
                _carrierJigStatusList.CarrierJigStatusH2.DelayTime = _delayTime;
            }
            if (_currentRecipe.OptionRecipe.SkipHead34 == false)
            {
                _carrierJigStatusList.CarrierJigStatusH3.DelayTime = _delayTime;
                _carrierJigStatusList.CarrierJigStatusH4.DelayTime = _delayTime;
            }

            if (EnablePressureHold && 
               (_machineStatus.IsDryRunMode == false || _machineStatus.DryRunUseChamberPressure))
            {
                IsCanStop = false;
                if (_devices.AnalogInputs.VacuumPressureInTorr > _currentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec
                    && AngleValve.IsOpenOutput() == false)
                {
                    AngleValve.Open();
                }

                if (_devices.AnalogInputs.VacuumPressureInTorr <= _currentRecipe.DryPumpRecipe.VacuumPressureSpec
                    && AngleValve.IsCloseOutput() == false)
                {
                    AngleValve.Close();
                }
            }

            if (EnableWritePressureLog && _currentRecipe.OptionRecipe.SavePressureLog == true)
            {
                PressureLog(_devices.AnalogInputs.VacuumPressureInTorr);
            }

            return base.PreProcess();
        }

        public override bool ProcessToWarning()
        {
            EnableWritePressureLog = false;
            EnablePressureHold = false;
            EnableTimerDelay = false;
            return base.ProcessToWarning();
        }

        public override bool ProcessToAlarm()
        {
            EnableTimerDelay = false;
            EnablePressureHold = false;
            EnableWritePressureLog = false;
            return base.ProcessToAlarm();
        }

        public override bool ProcessToStop()
        {
            EnableWritePressureLog = false;
            EnablePressureHold = false;
            EnableTimerDelay = false;
            return base.ProcessToStop();
        }

        public override bool ProcessToRun()
        {
            switch ((EDryPumpProcToRunStep)Step.ToRunStep)
            {
                case EDryPumpProcToRunStep.Start:
                    Log.Debug("ToRun start");
                    procOutputs.ClearOutputs();
                    EnableTimerDelay = false;
                    PumpRunRetryCount = 0;
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.AngleValve_Close:
                    if (AngleValve.IsClose())
                    {
                        Step.ToRunStep = (int)EDryPumpProcToRunStep.ChamberTorrCheck;
                        break;
                    }

                    Log.Debug($"Closing {AngleValve.Name}");
                    AngleValve.Close();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.AngleValveCylinderMoveDelay,
                        () => AngleValve.IsClose());
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.AngleValve_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_ANGLE_VALUE_CLOSE_FAIL);
                        break;
                    }

                    Log.Debug($"{AngleValve.Name} closed");
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.ChamberTorrCheck:
                    Log.Debug($"Chamber torr check: {_devices.AnalogInputs.VacuumPressureInTorr}");
                    if(In_ChamberPurgeOn.Value == false)
                    {
                        Log.Debug("No pressure in the chamber, skip purge");
                        Step.ToRunStep = (int)EDryPumpProcToRunStep.ChamperPurge_Off;
                        break;
                    }

                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.ChamperPurge_On:
                    Log.Debug("Purge start");
                    Out_ChamberPurgeOn.Value = true;
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.ChamperPurge_Off:
                    if (In_ChamberPurgeOn.Value)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug($"Vaccum pressure detected: {_devices.AnalogInputs.VacuumPressureInTorr}");
                    Log.Debug("Champer Purge Off");
                    Out_ChamberPurgeOn.Value = false;
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.SetFlag_DryPump_PurgeDone:
                    Log.Debug("Set flag purge done");
                    procOutputs[EDryPumpProcOutput.PurgeComplete].Value = true;
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.DryPump_Run:
                    //if (In_DryPumpRun.Value)
                    //{
                    //    Step.ToRunStep = (int)EDryPumpProcToRunStep.ClearFlag_DryPump_PurgeDone;
                    //    break;
                    //}

                    //Log.Debug($"{Out_DryPumpRun.Name} run");
                    //Out_DryPumpRun.Value = true;
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.ClearFlag_DryPump_PurgeDone:
                    if (procInputs[EDryPumpProcInput.Inject_Chamber_ReadyOut].Value == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("Clear flag purge done for inject");
                    procOutputs[EDryPumpProcOutput.PurgeComplete].Value = false;
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.End:
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
                    _cIMCollection.WriteMCC_OnlyStart(EMCCAction.PU01_INIT_PU01);
                    Step.OriginStep++;
                    break;
                case EDryPumpProcOriginStep.End:
                    Step.OriginStep++;
                    _cIMCollection.WriteMCC_OnlyEnd(EMCCAction.PU01_INIT_PU01);
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

                    _cIMCollection.WriteMCC(EMCCAction.PU01_IN_WAIT_TIME, EMCCAction.PU01_ACT_WAIT_PUMPING);

                    TactTime.CycleTimeCounter = Environment.TickCount;
                    _delayTime = 0;
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Run:
                    if (In_DryPumpRun.Value && Out_DryPumpRun.Value)
                    {
                        Step.RunStep = (int)EDryPumpProcResinInjectStep.InitQueue;
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
                    if (WaitTimeOutOccurred && _machineStatus.IsDryRunMode == false)
                    {
                        if (PumpRunRetryCount < 1)
                        {
                            Log.Error("Dry Pump Run Fail, Start Retry Run");
                            Out_DryPumpRun.Value = false;
                            Wait(100);
                            Step.RunStep = (int)EDryPumpProcResinInjectStep.DryPump_ResetAlarm;
                            break;
                        }

                        RaiseWarning(EWarning.MO_DRYPUMP_RUN_TIMEOUT);
                        break;
                    }

                    Log.Debug("Dry Pump Run Success!");
                    PumpRunRetryCount = 0;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.InitQueue;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_ResetAlarm:
                    Log.Debug("DryPump Reset Alarm");
                    Out_DryPumpResetAlarm.Value = true;
                    Task.Delay(1000).ContinueWith(t => Out_DryPumpResetAlarm.Value = false);
                    PumpRunRetryCount++;
                    Log.Debug($"Dry Pump Reset Alarm Count: {PumpRunRetryCount}");
                    Wait(5000, () => _devices.Inputs.DryPumpAlarm.Value == false);
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.DryPump_Run;
                    break;
                case EDryPumpProcResinInjectStep.InitQueue:
                    _cIMCollection.WriteMCC(EMCCAction.PU01_ACT_WAIT_PUMPING, EMCCAction.PU01_WAIT_REQUEST_DRYPUMP_VAC);

                    Log.Debug("Init queue");
                    if (_currentRecipe.OptionRecipe.OneTorrAndPurge && _currentRecipe.OptionRecipe.SkipVentTime)
                    {
                        DryPumpResinInjectStep = new Queue<EDryPumpProcResinInjectStep>(ProcessesWorkSequence.DryPumpResinInjectSequence_Use1Torr_SkipVent);
                    }
                    else if (_currentRecipe.OptionRecipe.OneTorrAndPurge)
                    {
                        DryPumpResinInjectStep = new Queue<EDryPumpProcResinInjectStep>(ProcessesWorkSequence.DryPumpResinInjectSequence_Use1Torr);
                    }
                    else if (_currentRecipe.OptionRecipe.SkipVentTime)
                    {
                        DryPumpResinInjectStep = new Queue<EDryPumpProcResinInjectStep>(ProcessesWorkSequence.DryPumpResinInjectSequence_SkipVent);
                    }
                    else
                    {
                        DryPumpResinInjectStep = new Queue<EDryPumpProcResinInjectStep>(ProcessesWorkSequence.DryPumpResinInjectSequence);
                    }

                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.StepQueue_EmptyCheck:
                    if (DryPumpResinInjectStep.Count <= 0)
                    {
                        Step.RunStep = (int)EDryPumpProcResinInjectStep.End;
                        break;
                    }

                    Step.RunStep = (int)DryPumpResinInjectStep.Dequeue();
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait:
                    if (procInputs[EDryPumpProcInput.Vacuum_WorkRequest].Value == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Info($"Input detect {EDryPumpProcInput.Vacuum_WorkRequest}");
                    TactTime.TactTimeCounter = Environment.TickCount;
                    _plotter.ClearData();
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.WriteMCC_Open_AngleValve:
                    Log.Debug("Write MCC Open Angle Valve");
                    _cIMCollection.WriteMCC(EMCCAction.PU01_WAIT_REQUEST_DRYPUMP_VAC, EMCCAction.PU01_ANGLE_VALVE_OPEN);
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_Open:
                    Log.Debug($"Opening {AngleValve.Name}");
                    if (_machineStatus.IsDryRunMode && _machineStatus.DryRunUseChamberPressure == false)
                    {
                        Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                        break;
                    }

                    PressureLog(_devices.AnalogInputs.VacuumPressureInTorr, EPumpAction.ValveOpen);
                    EnableWritePressureLog = true;
                    AngleValve.Open();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.AngleValveCylinderMoveDelay, AngleValve.IsOpen);
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_OpenWait:
                    if (WaitTimeOutOccurred && 
                       (_machineStatus.DryRunUseChamberPressure || _machineStatus.IsDryRunMode == false))
                    {
                        RaiseWarning(EWarning.CY_ANGLE_VALUE_OPEN_FAIL);
                        break;
                    }

                    Log.Debug($"{AngleValve.Name} Opened");
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.VacuumGauge_SpecIn_Wait_1torr:
                    if (_devices.AnalogInputs.VacuumPressureInTorr > 1.0 &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        Wait(50);
                        break;
                    }
                    Log.Info($"DryPump vacuum 1st in-spec 1.0 Torr");
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.WriteMCC_Close_AngleValve:
                    Log.Debug("Write MCC Close Angle Valve");
                    _cIMCollection.WriteMCC(EMCCAction.PU01_WAIT_VACCUM_GAUGE_IN_TARGET, EMCCAction.PU01_ANGLE_VALVE_CLOSE);
                    _cIMCollection.WriteMCC(EMCCAction.PU01_ANGLE_VALVE_CLOSE, EMCCAction.PU01_DELAY_BEFORE_INJECT);
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_Close:
                    Log.Debug($"Closing {AngleValve.Name}");
                    PressureLog(_devices.AnalogInputs.VacuumPressureInTorr, EPumpAction.ValveClose);
                    AngleValve.Close();
                    Wait(_currentRecipe.CylinderDelayTimeRecipe.AngleValveCylinderMoveDelay, AngleValve.IsClose);
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_ANGLE_VALUE_CLOSE_FAIL);
                        break;
                    }

                    Log.Debug("Angle valve close success");
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Purge:
                    Log.Debug("DryPump purge start");

                    _cIMCollection.WriteMCC(EMCCAction.PU01_ANGLE_VALVE_CLOSE, EMCCAction.PU01_PURGE_AND_WAIT);

                    PressureLog(_devices.AnalogInputs.VacuumPressureInTorr, EPumpAction.PurgeStart);
                    Out_ChamberPurgeOn.Value = true;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Purge_Wait:
                    if (_devices.AnalogInputs.VacuumPressureInTorr < 749 &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        Wait(10);
                        break;
                    }

                    Out_ChamberPurgeOn.Value = false;
                    Log.Debug("DryPump purge complete");
                    PressureLog(_devices.AnalogInputs.VacuumPressureInTorr, EPumpAction.PurgeEnd);
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.WriteMCC_VacuumGauge_Target_Wait:
                    Log.Debug("WriteMCC_VacuumGauge_Target_Wait");
                    _cIMCollection.WriteMCC(EMCCAction.PU01_ANGLE_VALVE_OPEN, EMCCAction.PU01_WAIT_VACCUM_GAUGE_IN_TARGET);
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.VacuumGauge_Target_Wait:
                    if (_devices.AnalogInputs.VacuumPressureInTorr > _currentRecipe.DryPumpRecipe.VacuumPressureSpec &&
                       (_machineStatus.IsDryRunMode == false || _machineStatus.DryRunUseChamberPressure))
                    {
                        Wait(50);
                        break;
                    }

                    Log.Info($"DryPump vacuum in-target {_devices.AnalogInputs.VacuumPressureInTorr} Torr");
                    Log.Debug($"Enable hold Pressure under {_currentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec}");
                    TactTime.SetTactTime();
                    EnablePressureHold = true;
                    EnableTimerDelay = true;
                    _delayStartTick = Environment.TickCount;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.Delay_BeforeInject:
                    if (((Environment.TickCount - _delayStartTick) / 1000.0) < InjectTimeRecipe.DelayTime)
                    {
                        Wait(10);
                        break;
                    }

                    _cIMCollection.WriteMCC(EMCCAction.PU01_DELAY_BEFORE_INJECT, EMCCAction.PU01_OUT_WAIT_TIME);

                    Log.Debug($"Delay before inject complete: {InjectTimeRecipe.DelayTime}s");
                    EnableTimerDelay = false;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_VacuumDone_Send:
                    procOutputs[EDryPumpProcOutput.ChamberVacuumSuccess].Value = true;
                    Log.Info($"Set output {EDryPumpProcOutput.ChamberVacuumSuccess}");
                    if (_currentRecipe.OptionRecipe.SkipVentTime == false)
                    {
                        Log.Debug("Wait Vent");
                        _cIMCollection.WriteMCC(EMCCAction.PU01_OUT_WAIT_TIME, EMCCAction.PU01_DRYPUMP_WAIT_VEN_TIME);
                    }

                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_WaitVentTime:
                    if (((_carrierJigStatusList.CarrierJigStatusH1.InjectTime < InjectTimeRecipe.VentTimeAfterInject) && !_currentRecipe.OptionRecipe.SkipHead12 && !_currentRecipe.OptionRecipe.SkipHead1 && SPDHead1ProcessMode == EProcessMode.Run) ||
                        ((_carrierJigStatusList.CarrierJigStatusH2.InjectTime < InjectTimeRecipe.VentTimeAfterInject) && !_currentRecipe.OptionRecipe.SkipHead12 && !_currentRecipe.OptionRecipe.SkipHead2 && SPDHead2ProcessMode == EProcessMode.Run) ||
                        ((_carrierJigStatusList.CarrierJigStatusH3.InjectTime < InjectTimeRecipe.VentTimeAfterInject) && !_currentRecipe.OptionRecipe.SkipHead34 && !_currentRecipe.OptionRecipe.SkipHead3 && SPDHead3ProcessMode == EProcessMode.Run) ||
                        ((_carrierJigStatusList.CarrierJigStatusH4.InjectTime < InjectTimeRecipe.VentTimeAfterInject) && !_currentRecipe.OptionRecipe.SkipHead34 && !_currentRecipe.OptionRecipe.SkipHead4 && SPDHead4ProcessMode == EProcessMode.Run))
                    {
                        Wait(10);
                        break;
                    }

                    Log.Debug($"Disable hold Pressure under {_currentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec}");
                    EnablePressureHold = false;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.SetStartVent:
                    Log.Debug("Vent start");

                    _cIMCollection.WriteMCC(EMCCAction.PU01_DRYPUMP_WAIT_VEN_TIME, EMCCAction.PU01_START_VENT);

                    _ventStartTick = Environment.TickCount;
                    PressureLog(_devices.AnalogInputs.VacuumPressureInTorr, EPumpAction.VentStart);
                    Out_ChamberPurgeOn.Value = true;

                    _cIMCollection.WriteMCC(EMCCAction.PU01_START_VENT, EMCCAction.PU01_PURGE_AND_WAIT);

                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_PurgeAndWait:
                    //if (((Environment.TickCount - _ventStartTick) / 1000.0) < InjectTimeRecipe.VentTime)
                    //{
                    //    Wait(10);
                    //    break;
                    //}
                    if (In_ChamberPurgeOn.Value)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug($"Vent Complete: {InjectTimeRecipe.VentTime}s");
                    PressureLog(_devices.AnalogInputs.VacuumPressureInTorr, EPumpAction.VentComplete);
                    procOutputs[EDryPumpProcOutput.VentComplete].Value = true;
                    Out_ChamberPurgeOn.Value = false;
                    IsCanStop = true;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.Inject_PurgeRequest_Wait:
                    if (procInputs[EDryPumpProcInput.Purge_WorkRequest].Value == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug("Detected inject request purge");
                    Log.Debug($"Disable hold Pressure under {_currentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec}");
                    EnablePressureHold = false;
                    Out_ChamberPurgeOn.Value = true;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.Wait_PurgeEnd:
                    if (In_ChamberPurgeOn.Value)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug("Purge end");
                    Out_ChamberPurgeOn.Value = false;
                    IsCanStop = true; //Skip Vent
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.SetFlag_PurgeFinish:
                    Log.Debug("Set flag purge finish for inject");
                    procOutputs[EDryPumpProcOutput.PurgeComplete].Value = true;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.WaitToClear_ProcOutput:
                    if (procInputs[EDryPumpProcInput.Vacuum_WorkRequest].Value == true)
                    {
                        Wait(20);
                        break;
                    }

                    _cIMCollection.WriteMCC(EMCCAction.PU01_PURGE_AND_WAIT, EMCCAction.PU01_IN_WAIT_TIME);

                    Log.Info($"Clear output {EDryPumpProcOutput.ChamberVacuumSuccess}");
                    procOutputs[EDryPumpProcOutput.ChamberVacuumSuccess].Value = false;
                    procOutputs[EDryPumpProcOutput.VentComplete].Value = false;
                    procOutputs[EDryPumpProcOutput.PurgeComplete].Value = false;

                    PressureLog(_devices.AnalogInputs.VacuumPressureInTorr, EPumpAction.EndCycle);
                    EnableWritePressureLog = false;
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.StepQueue_EmptyCheck;
                    break;
                case EDryPumpProcResinInjectStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    TactTime.SetCycleTime();
                    Log.Info("ResinInject end, starting new cycle");
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.Start;
                    break;

            }
        }
        #endregion

        #region Private Methods
        private enum EPumpAction
        {
            ValveOpen,
            ValveClose,
            PurgeStart,
            PurgeEnd,
            VentStart,
            VentComplete,
            EndCycle
        }

        private void PressureLog(double pressure, EPumpAction? action = null)
        {
            if ((DateTime.Now - pressureLogWatchTime).TotalMilliseconds < _recipeSelector.CurrentRecipe.DryPumpRecipe.PressureLogTimelaps * 1000 &&
                action == null)
            {
                return;
            }
            pressureLogWatchTime = DateTime.Now;

            var now = DateTime.Now;
            string message = $"{now:yyyy/MM/dd HH:mm:ss},{pressure:F3}";
            if (action != null)
                message += $",{action}";
            message += "\r\n";

            _plotter.AddChamberPressureData(pressure);
            if (action == EPumpAction.EndCycle)
            {
                _plotter.Save();
            }

            if (string.IsNullOrEmpty(pressureLogFolder))
                return;

            string path = Path.Combine(
                pressureLogFolder,
                now.ToString("yyyy-MM"),
                now.ToString("yyyy-MM-dd"),
                $"{now:yyyy-MM-dd_HH}.txt");

            lock (_fileLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.AppendAllText(path, message);
            }
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly MachineStatus _machineStatus;
        private readonly object _fileLock = new object();
        private readonly RecipeSelector _recipeSelector;
        private readonly CarrierJigStatusList _carrierJigStatusList;
        private readonly IConfiguration _configuration;
        private readonly TactTimeList _tactTimeList;
        private readonly Plotter _plotter;
        private readonly CIMCollection _cIMCollection;

        private TactTime TactTime => _tactTimeList.DryPump;
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        private InjectTimeRecipe InjectTimeRecipe => _recipeSelector.CurrentRecipe.InjectTimeRecipe;
        private string pressureLogFolder => _configuration.GetValue<string>("Folders:PressureLogFolder");
        private int PumpRunRetryCount;
        private double _ventStartTick;
        private double _delayStartTick;
        private double _delayTime;

        private bool EnablePressureHold;
        private bool EnableTimerDelay;
        private bool EnableWritePressureLog;

        private Queue<EDryPumpProcResinInjectStep> DryPumpResinInjectStep = new Queue<EDryPumpProcResinInjectStep>();

        private DateTime pressureLogWatchTime = DateTime.Now;
        #endregion
    }
}
