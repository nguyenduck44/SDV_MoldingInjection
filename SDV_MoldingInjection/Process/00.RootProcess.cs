using Accessibility;
using EQX.Core.Common;
using EQX.Core.Communication;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.Process;
using EQX.UI.Controls;
using EQX.UI.Language;
using EQX.UI.MVVM;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.MVVM.Views;
using SDV_MoldingInjection.Recipe;
using System.Windows;
using TOPENG_Device;

namespace SDV_MoldingInjection.Process
{
    public class RootProcess<TESequence, TESemiSequence> : ProcessBase<ESequence>
        where TESequence : Enum
        where TESemiSequence : Enum
    {
        #region Privates
        private readonly Devices _devices;
        private readonly MachineStatus _machineStatus;
        private readonly NavigationStore _viewModelavigationStore;
        private readonly INavigationService _navigationService;
        private readonly RecipeSelector _recipeSelector;
        private readonly RecipeList _recipeList;
        private readonly ProcessIO _processIO;
        private int raisedAlarmCode = 2;
        private int raisedWarningCode = 2;
        private readonly IAlertService _alarmService;
        private readonly IAlertService _warningService;
        private readonly object _lockAlarm = new object();

        private bool IsMainAirSupplied => _devices.Inputs.MainCDACheck.Value;
        private bool IsServoOn => _devices.Inputs.ServoOn.Value;
        #endregion

        #region Constructor
        public RootProcess(Devices devices,
            MachineStatus machineStatus,
            NavigationStore viewModelavigationStore,
            INavigationService navigationService,
            RecipeSelector recipeSelector,
            RecipeList recipeList,
            ProcessIO processIO,
            [FromKeyedServices("AlarmService")] IAlertService alarmService,
            [FromKeyedServices("WarningService")] IAlertService warningService)
        {
            _devices = devices;
            _machineStatus = machineStatus;
            _viewModelavigationStore = viewModelavigationStore;
            _navigationService = navigationService;
            _recipeSelector = recipeSelector;
            _recipeList = recipeList;
            _processIO = processIO;
            _alarmService = alarmService;
            _warningService = warningService;

            this.ProcessModeUpdated += ProcessModeUpdatedHandler;

            this.AlarmRaised += (alarmId, alarmSource) =>
            {
                RootProcess_AlarmRaised(alarmId, alarmSource);
            };
            this.WarningRaised += (warningId, warningSource) =>
            {
                RootProcess_WarningRaised(warningId, warningSource);
            };

            _machineStatus.EquipState = new EquipState
            {
                IsRunning = false,
            };
        }

        #endregion

        #region Override Methods
        public override bool PreProcess()
        {
            // 1. CHECK ALARM STATUS (Utils, Motion, Safety...)
            if (ProcessMode != EProcessMode.ToAlarm && ProcessMode != EProcessMode.Alarm)
            {
                CheckRealTimeAlarmStatus();
            }

            //2.CHECK USER OPERATION COMMAND(Origin / Ready / Start / Stop / Semiauto...)
            //if (_machineStatus.OPCommand == EOperationCommand.None) return base.PreProcess();

            EOperationCommand command = EOperationCommand.None;
            if (_machineStatus.IsRunningProcessMode)
            {
                // Machine is doing something : Run, Origin, To-Action
                // Can Stop
                if (_machineStatus.OPCommand == EOperationCommand.Stop
                    || _devices.Inputs.OPButtonStop.Value == true
                    /*|| _machineStatus.InitDeinitStatus == EInitDeinitStatus.DeinitStarted*/)
                {
                    command = EOperationCommand.Stop;
                }

                if (command == EOperationCommand.None &&
                    _recipeList.IdlePurgeRecipe.EnableIdlePurge &&
                    Sequence == ESequence.AutoRun &&
                    ProcessMode == EProcessMode.Run)
                {
                    if (IsAutoRunWaitingNoPanel())
                    {
                        if ((Environment.TickCount - _machineStatus.MachineIdleTick) >
                            _recipeList.IdlePurgeRecipe.IdlePurgeAfterStopTime * 60000 &&
                            _recipeSelector.CurrentRecipe.IdlePurgeRecipe.IdlePurgeAfterStopTime != 0 &&
                            _recipeSelector.CurrentRecipe.IdlePurgeRecipe.IdlePurgeCycleTime != 0)
                        {
                            command = EOperationCommand.SemiAuto;
                            _machineStatus.SemiAutoSequence = ESemiSequence.IdlePurge;
                        }
                    }
                    else
                    {
                        _machineStatus.MachineIdleTick = Environment.TickCount;
                    }
                }

                // Block run OPCommand actived while machine is Runiing
                _machineStatus.OPCommand = EOperationCommand.None;
            }
            else
            {
                // Machine is in standby mode (doing nothing)
                // Can Origin / Ready / Start
                if (_machineStatus.OPCommand == EOperationCommand.Origin)
                {
                    command = EOperationCommand.Origin;
                }

                if (ProcessMode != EProcessMode.ToAlarm &&
                    ProcessMode != EProcessMode.Alarm && ProcessMode != EProcessMode.None)
                {
                    if ((_machineStatus.OPCommand == EOperationCommand.Ready
                        || _devices.Inputs.OPButtonReset.Value == true)
                        && (_viewModelavigationStore.CurrentViewModel is AutoViewModel))
                    {
                        //command = EOperationCommand.Ready;
                    }
                    else if ((_machineStatus.OPCommand == EOperationCommand.Start
                        || _devices.Inputs.OPButtonStart.Value == true)
                        && (_viewModelavigationStore.CurrentViewModel is AutoViewModel))
                    {
                        command = EOperationCommand.Start;
                    }
                    else if (_machineStatus.OPCommand == EOperationCommand.SemiAuto)
                    {
                        command = EOperationCommand.SemiAuto;
                    }
                }
                else if (_machineStatus.OPCommand == EOperationCommand.Ready
                    || _machineStatus.OPCommand == EOperationCommand.Start
                    || _machineStatus.OPCommand == EOperationCommand.SemiAuto
                    || ((_devices.Inputs.OPButtonStart.Value == true)
                    && _viewModelavigationStore.CurrentViewModel is AutoViewModel))
                {
                    MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_ResetAlarmBeforeRun"], (string)Application.Current.Resources["str_Confirm"]);
                    _machineStatus.OPCommand = EOperationCommand.None;
                }
                else if (_machineStatus.OPCommand == EOperationCommand.SemiAuto &&
                    _machineStatus.SemiAutoSequence == ESemiSequence.MoveMultiPoint)
                {
                    command = EOperationCommand.SemiAuto;
                }

                if (_recipeSelector.CurrentRecipe.IdlePurgeRecipe.EnableIdlePurge &&
                    _recipeSelector.CurrentRecipe.IdlePurgeRecipe.IdlePurgeAfterStopTime != 0 &&
                    _recipeSelector.CurrentRecipe.IdlePurgeRecipe.IdlePurgeCycleTime != 0 &&
                   (Environment.TickCount - _machineStatus.MachineIdleTick) > _recipeSelector.CurrentRecipe.IdlePurgeRecipe.IdlePurgeAfterStopTime * 60000)
                {
                    command = EOperationCommand.SemiAuto;
                    _machineStatus.SemiAutoSequence = ESemiSequence.IdlePurge;
                }
            }

            // 3. HANDLE USER OPERATION COMMAND
            HandleOPCommand(command);
            return base.PreProcess();
        }

        public override bool ProcessToAlarm()
        {
            if (Childs.All(child => child.ProcessStatus == EProcessStatus.ToAlarmDone))
            {
                foreach (var motion in _devices.Motions.All!) { motion.Stop(); }

                _machineStatus.OriginDone = false;

                _machineStatus.EquipState.IsAvailable = false;

                _machineStatus.MachineIdleTick = Environment.TickCount;

                _devices.Outputs.Lamp_Alarm(true);

                ProcessMode = EProcessMode.Alarm;
                Log.Info("ToAlarm Done, Alarm");
                AlertNotifyView.ShowDialog(_alarmService.GetById(raisedAlarmCode), true);

                // TODO: CIM
                CIMScenarioDispatcher.ExecuteScenario(CIMScenario.AlarmRelease, new CIMScenarioContext
                {
                    CIMAlarmData = new CIMAlarmData
                    {
                        ALCD = 2, // HEAVY ALARM
                        ALID = raisedWarningCode,
                        AlarmDescription = ((EAlarm)raisedWarningCode).ToString(),
                    }
                });

                raisedAlarmCode = 2;
            }
            else
            {
                Thread.Sleep(10);
            }

            return true;
        }

        public override bool ProcessToWarning()
        {
            if (Childs!.Count(child => child.ProcessStatus != EProcessStatus.ToWarningDone) == 0)
            {
                foreach (var motion in _devices.Motions.All!) { motion.Stop(); }

                _devices.Outputs.Lamp_Alarm(true);
                ProcessMode = EProcessMode.Warning;
                Log.Info("ToWarning Done, Warning");
                AlertNotifyView.ShowDialog(_warningService.GetById(raisedWarningCode), true);

                _machineStatus.EquipState.IsAvailable = false;

                _machineStatus.MachineIdleTick = Environment.TickCount;

                // TODO: CIM
                CIMScenarioDispatcher.ExecuteScenario(CIMScenario.AlarmRelease, new CIMScenarioContext
                {
                    CIMAlarmData = new CIMAlarmData
                    {
                        ALCD = 2, // HEAVY ALARM
                        ALID = raisedWarningCode,
                        AlarmDescription = ((EWarning)raisedWarningCode).ToString(),
                    }
                });

                raisedWarningCode = 2;
            }
            else
            {
                Thread.Sleep(10);
            }

            return true;
        }

        public override bool ProcessOrigin()
        {
            if (Childs!.Count(child => child.ProcessStatus != EProcessStatus.OriginDone) == 0)
            {
                _machineStatus.OriginDone = true;
                _machineStatus.MachineReadyDone = false;

                _devices.Outputs.Lamp_Stop();
                //foreach (var motion in _devices.Motions.All!) { motion.ClearPosition(); }

                MessageBoxEx.Show(Application.Current.Resources["str_OriginSuccess"].ToString(), false);

                ProcessMode = EProcessMode.Stop;
                Log.Info("Origin done, Stop");
            }
            else
            {
                Thread.Sleep(10);
            }

            return true;
        }

        public override bool ProcessToOrigin()
        {
            switch ((ERootProcToOriginStep)Step.OriginStep)
            {
                case ERootProcToOriginStep.Start:
                    Log.Info("To Origin started");
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.AutoModeSwitchCheck:
#if !SIMULATION
                    if (_devices.Inputs.AutoSW.Value == false)
                    {
                        RaiseWarning(EWarning.OP_MAIN_CP_KEY_AUTO_ERROR);
                        break;
                    }

                    if (_machineStatus.IsAutoMode == false)
                    {
                        RaiseWarning(EWarning.OP_MAIN_STG_MODE_AUTO_ERROR);
                        break;
                    }
#endif
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.DoorClose:
                    _devices.Outputs.EQPStop.Value = false;
                    Wait(5000, () => _devices.Inputs.DoorClose);
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.DoorSensorCheck:
                    if (_devices.Inputs.DoorClose == false)
                    {
                        RaiseWarning((int)EWarning.DO_MAIN_DOOR_OPEN);
                        break;
                    }

                    Log.Debug("Doors closed.");
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.DoorLockCheck:
                    if (_devices.Inputs.DoorLock == false)
                    {
                        RaiseWarning((int)EWarning.DO_MAIN_DOOR_INTERLOCK_ON);
                        break;
                    }
                    Log.Debug("Doors safety locked.");
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.Motion_AlarmReset:
                    if (_devices.Motions.All.All(m => m.Status.IsAlarm == false))
                    {
                        Step.OriginStep = (int)ERootProcToOriginStep.ChildsToOriginDone_Wait;
                        break;
                    }

                    _devices.Motions.All.Where(m => m.Status.IsAlarm == true)
                        .ToList().ForEach(m => m.AlarmReset());

                    Wait(2000);
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.Motion_AlarmReset_Wait:
                    if (_devices.Motions.All.Any(m => m.Status.IsAlarm))
                    {
                        RaiseAlarm(EAlarm.MO_MAIN_SERVO_RESET_FAIL);
                        break;
                    }

                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.ChildsToOriginDone_Wait:
                    if (Childs!.Count(child => child.ProcessStatus != EProcessStatus.ToOriginDone) != 0)
                    {
                        Wait(10);
                        break;
                    }
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.End:
                    Log.Info("To Origin done");
                    Step.OriginStep = 0;
                    ProcessMode = EProcessMode.Origin;
                    break;
            }
            return true;
        }

        public override bool ProcessToStop()
        {
            if (Childs!.Count(child => child.ProcessStatus != EProcessStatus.ToStopDone) == 0)
            {
                _devices.Motions.All.ForEach(r => r.Stop());

                _machineStatus.MachineIdleTick = Environment.TickCount;

                _machineStatus.EquipState.IsRunning = false;

                ProcessMode = EProcessMode.Stop;
                _devices.Outputs.Lamp_Stop();

                Log.Info("ToStop Done, Stop");

                // TODO: CIM
                if (_machineStatus.EquipState.IsInterlock)
                {
                    EquipEventHelpers.EquipStateReport(_machineStatus.EquipState);
                }
                else
                {
                    // Stop after AutoRun -> TPMLoss Report
                    if (Sequence == ESequence.AutoRun)
                    {
                        // TPMLoss Report
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            EquipEventDetail.Create(EquipEvent.TPMLossReady).SetPLCBitOn();

                            TPMLossWindow tpmLossWindow = new TPMLossWindow();
                            tpmLossWindow.ShowDialog();

                            if (tpmLossWindow.SelectedTPMMode != null)
                            {
                                EquipEventHelpers.TPMLostReport((ETPMLossDesciption)tpmLossWindow.SelectedTPMMode);
                            }
                        });
                    }
                }
            }
            else
            {
                Thread.Sleep(10);
            }

            return true;
        }

        public override bool ProcessToRun()
        {
            switch ((ERootProcToRunStep)Step.ToRunStep)
            {
                case ERootProcToRunStep.Start:
                    Log.Debug("ToRun Start");
                    Step.ToRunStep++;
                    break;
                case ERootProcToRunStep.AutoModeSwitchCheck:
#if !SIMULATION
                    if (_devices.Inputs.AutoSW.Value == false)
                    {
                        RaiseWarning(EWarning.OP_MAIN_CP_KEY_AUTO_ERROR);
                        break;
                    }

                    if (_machineStatus.IsAutoMode == false)
                    {
                        RaiseWarning(EWarning.OP_MAIN_STG_MODE_AUTO_ERROR);
                        break;
                    }
#endif
                    Step.ToRunStep++;
                    break;
                case ERootProcToRunStep.DoorClose:
                    _devices.Outputs.EQPStop.Value = false;
                    Wait(5000, () => _devices.Inputs.DoorClose);
                    Step.ToRunStep++;
                    break;
                case ERootProcToRunStep.DoorSensorCheck:
                    if (_devices.Inputs.DoorClose == false)
                    {
                        RaiseWarning((int)EWarning.DO_MAIN_DOOR_OPEN);
                        break;
                    }

                    Log.Debug("Doors closed.");
                    Step.ToRunStep++;
                    break;
                case ERootProcToRunStep.DoorLock_Check:
                    if (_devices.Inputs.DoorLock == false)
                    {
                        RaiseWarning((int)EWarning.DO_MAIN_DOOR_INTERLOCK_ON);
                        break;
                    }

                    Log.Debug("Doors safety locked.");
                    Step.ToRunStep++;
                    break;
                case ERootProcToRunStep.ChildsToRunDone_Wait:
                    if (Childs!.Count(child => child.ProcessStatus != EProcessStatus.ToRunDone) != 0)
                    {
                        Wait(10);
                        break;
                    }
                    Step.ToRunStep++;
                    break;
                case ERootProcToRunStep.End:
                    _devices.Outputs.Lamp_Run();

                    // TODO: CIM
                    if (Sequence == ESequence.AutoRun)
                    {
                        _machineStatus.EquipState.IsAvailable = true;
                        _machineStatus.EquipState.IsInterlock = false;
                        _machineStatus.EquipState.IsRunning = true;
                    }

                    _machineStatus.Message = string.Empty;
                    ProcessMode = EProcessMode.Run;
                    Log.Info("ToRun Done, Running");
                    break;
                default:
                    Thread.Sleep(10);
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
                case ESequence.AutoRun:
                    break;
                case ESequence.Ready:
                    if (Childs!.Count(child => child.Sequence != ESequence.Stop) == 0)
                    {
                        _machineStatus.MachineReadyDone = true;
                        ProcessMode = EProcessMode.Stop;
                        _devices.Outputs.Lamp_Stop();
                        Log.Info("Initialize done");
                    }
                    break;
                default: // User defined SemiAuto sequence
                    if (Childs!.Count(child => child.Sequence != ESequence.Stop) == 0)
                    {
                        ProcessMode = EProcessMode.Stop;
                        _devices.Outputs.Lamp_Stop();
                        Log.Info("SemiAuto sequence done");
                    }
                    break;
            }

            return true;
        }
        #endregion

        #region Private Methods
        private void ProcessModeUpdatedHandler(object? sender, EventArgs e)
        {
            _machineStatus.CurrentProcessMode = this.ProcessMode;

            if (this.ProcessMode == EProcessMode.ToWarning ||
                this.ProcessMode == EProcessMode.Warning ||
                this.ProcessMode == EProcessMode.ToAlarm ||
                this.ProcessMode == EProcessMode.Alarm)
            {
                _machineStatus.MachineReadyDone = false;
            }
        }

        private void CheckRealTimeAlarmStatus()
        {
#if !SIMULATION
            if (_devices.Inputs.ServoOn.Value == false)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                Log.Error("POWER OFF");
                RaiseAlarm((int)EAlarm.UT_MAIN_MCR_COIL_SERVO_OFF);
                return;
            }
#endif
            if (_devices.Inputs.Emergency.Value == true)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                Log.Error("Emergency Stop Activated. MC OFF");
                RaiseAlarm((int)EAlarm.EM_MAIN_CP_EMS_SERVO_OFF);
                return;
            }
            if (ProcessMode == EProcessMode.ToRun || ProcessMode == EProcessMode.Run ||
                ProcessMode == EProcessMode.ToOrigin || ProcessMode == EProcessMode.Origin)
            {
                if (_devices.Inputs.DoorClose == false)
                {
                    Log.Error("Door Open");
                    RaiseAlarm(EAlarm.DO_MAIN_DOOR_OPEN);
                    return;
                }
                if ((ProcessMode == EProcessMode.Run || ProcessMode == EProcessMode.Origin) && _devices.Inputs.DoorLock == false)
                {
                    Log.Error("Door Not Safety Lock");
                    RaiseAlarm(EAlarm.DO_MAIN_DOOR_INTERLOCK_ON);
                    return;
                }
            }

            if (_devices.Inputs.SmokeDetectAlarm.Value == true)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                Log.Error("Panel Smoke Detected");
                RaiseAlarm((int)EAlarm.UT_MAIN_SMOKE_DETECT);
                return;
            }

            if (_devices.Inputs.TempHighWarning.Value == true &&
                ProcessMode != EProcessMode.Warning &&
                ProcessMode != EProcessMode.ToWarning)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                Log.Error("OverTemperature Detected (>=35)");
                RaiseWarning((int)EWarning.TE_MAIN_SENSOR_TEMP_OVER);
                return;
            }

            if (_devices.Inputs.TempHighAlarm.Value == true)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                Log.Error("OverTemperature Detected (>=40)");
                RaiseAlarm((int)EAlarm.TE_MAIN_SENSOR_TEMP_OVER);
                return;
            }
#if !SIMULATION
            if (_devices.Inputs.MainCDACheck.Value == false)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                Log.Error("Main Air Not Supplied");
                RaiseAlarm((int)EAlarm.VC_MAIN_STG_AIR_LOW_OPEN);
                return;
            }
#endif
            if (_devices.Motions.All.Count(motion => motion.Status.IsMotionOn != true) > 0 &&
                (ProcessMode == EProcessMode.ToRun || ProcessMode == EProcessMode.Run))
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);

                _devices.Motions.All.Where(m => m.Status.IsMotionOn == false).ToList().ForEach(motion =>
                {
                    Log.Error($"{motion.Name} Motion Off");
                });
                RaiseAlarm((int)EAlarm.MO_MAIN_SERVO_PWR_OFF);
            }
            if (_devices.Motions.All.Count(motion => motion.Status.HwNegLimitDetect == true || motion.Status.HwPosLimitDetect == true) > 0
                && (ProcessMode == EProcessMode.ToRun || ProcessMode == EProcessMode.Run))
            {
                _devices.Motions.All.Where(m => m.Status.HwNegLimitDetect == true).ToList().ForEach(motion =>
                {
                    Log.Error($"{motion.Name} Limit (-) Detect");
                });
                _devices.Motions.All.Where(m => m.Status.HwPosLimitDetect == true).ToList().ForEach(motion =>
                {
                    Log.Error($"{motion.Name} Limit (+) Detect");
                });

                RaiseAlarm((int)EAlarm.MO_MAIN_AXIS_LIMIT_DETECT);
            }
            if (_devices.Motions.All.Count(motion => motion.Status.IsAlarm == true) > 0)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);

                _devices.Motions.All.Where(m => m.Status.IsAlarm == true).ToList().ForEach(motion =>
                {
                    Log.Error($"{motion.Name} Motion Alarm");
                });
                RaiseAlarm((int)EAlarm.MO_MAIN_SERVO_ALARM_DETECT);
            }
        }

        private void HandleOPCommand(EOperationCommand command)
        {
            switch (command)
            {
                case EOperationCommand.None:
                    Thread.Sleep(20);
                    break;
                case EOperationCommand.Origin:
                    foreach (var motion in _devices.Motions.All!)
                    {
                        if (motion.Status.IsAlarm)
                        {
                            motion.AlarmReset();
                            Thread.Sleep(200);
                        }

                        motion.MotionOn();
                    }

                    Thread.Sleep(1000);

                    if (_devices.Motions.All.Count(motion => motion.Status.IsMotionOn != true) > 0)
                    {
                        MessageBoxEx.ShowDialog("Motions turn on failed");
                        _machineStatus.OPCommand = EOperationCommand.None;
                        return;
                    }

                    Step.OriginStep = 0;
                    Childs?.ToList().ForEach(p => p.Step.OriginStep = 0);
                    ProcessMode = EProcessMode.ToOrigin;
                    _machineStatus.OPCommand = EOperationCommand.None;
                    break;
                case EOperationCommand.Ready:
                    if (_devices.Motions.All.Count(motion => motion.Status.IsMotionOn != true) > 0)
                    {
                        MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_TurnOnAllMotionsFirst"], (string)Application.Current.Resources["str_Confirm"]);
                        _machineStatus.OPCommand = EOperationCommand.None;
                        return;
                    }

                    if (_machineStatus.OriginDone == false)
                    {
                        MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_MachineNeedToBeOriginBeforeRun"]);
                        _machineStatus.OPCommand = EOperationCommand.None;
                        return;
                    }

                    Sequence = ESequence.Ready;
                    foreach (var process in Childs!)
                    {
                        process.ProcessStatus = EProcessStatus.None;
                        process.Sequence = Sequence;
                    }

                    ProcessMode = EProcessMode.ToRun;
                    _machineStatus.OPCommand = EOperationCommand.None;
                    break;
                case EOperationCommand.Start:
                    if (_devices.Motions.All.Count(motion => motion.Status.IsMotionOn != true) > 0)
                    {
                        MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_TurnOnAllMotionsFirst"], (string)Application.Current.Resources["str_Confirm"]);
                        _machineStatus.OPCommand = EOperationCommand.None;
                        return;
                    }

                    //if (_machineStatus.MachineReadyDone == false)
                    //{
                    //    MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_MachineNeedToBeReadyBeforeRun"]);
                    //    _machineStatus.OPCommand = EOperationCommand.None;
                    //    return;
                    //}

                    //if (Childs!.Any(p => p.IsAlarm))
                    //{
                    //    MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_MachineNeedToBeOriginBeforeRun"]);
                    //    _machineStatus.OPCommand = EOperationCommand.None;
                    //    return;
                    //}

                    Sequence = ESequence.AutoRun;

                    foreach (var process in Childs!)
                    {
                        process.ProcessStatus = EProcessStatus.None;
                        process.Sequence = Sequence;
                    }

                    ProcessMode = EProcessMode.ToRun;

                    _machineStatus.OPCommand = EOperationCommand.None;
                    _machineStatus.SemiAutoSequence = ESemiSequence.None;
                    break;
                case EOperationCommand.Stop:
                    _machineStatus.MachineReadyDone = false;

                    ProcessMode = EProcessMode.ToStop;
                    _machineStatus.OPCommand = EOperationCommand.None;
                    break;
                case EOperationCommand.SemiAuto:
                    if (_devices.Motions.All.Count(motion => motion.Status.IsMotionOn != true) > 0)
                    {
                        MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_TurnOnAllMotionsFirst"], (string)Application.Current.Resources["str_Confirm"]);
                        _machineStatus.OPCommand = EOperationCommand.None;
                        _machineStatus.MachineIdleTick = Environment.TickCount;
                        return;
                    }

                    if (_machineStatus.OriginDone == false)
                    {
                        MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_MachineNeedToBeOriginBeforeRun"]);
                        _machineStatus.OPCommand = EOperationCommand.None;
                        _machineStatus.MachineIdleTick = Environment.TickCount;
                        return;
                    }

                    Sequence = (ESequence)Enum.Parse(typeof(ESequence), _machineStatus.SemiAutoSequence.ToString());

                    foreach (var process in Childs!)
                    {
                        process.ProcessStatus = EProcessStatus.None;
                        process.Sequence = Sequence;
                    }

                    if (Sequence == ESequence.IdlePurge)
                    {
                        _navigationService.NavigateTo<IdlePurgeModeViewModel>();
                    }

                    if (Sequence == ESequence.MoveMultiPoint)
                    {
                        _devices.Outputs.EQPStop.Value = false;
                        Thread.Sleep(50);
                        ProcessMode = EProcessMode.Run;
                    }
                    else
                    {
                        ProcessMode = EProcessMode.ToRun;
                    }

                    _machineStatus.OPCommand = EOperationCommand.None;
                    _machineStatus.SemiAutoSequence = ESemiSequence.None;
                    break;
                default:
                    break;
            }
        }

        private void RootProcess_AlarmRaised(int alarmId, string alarmSource)
        {
            lock (_lockAlarm)
            {
                if (this.IsInAlarmMode()) return;

                // TODO: CIM
                CIMScenarioDispatcher.ExecuteScenario(CIMScenario.AlarmOccur, new CIMScenarioContext
                {
                    CIMAlarmData = new CIMAlarmData
                    {
                        ALCD = 2, // LIGHT ALARM
                        ALID = alarmId,
                        AlarmDescription = ((EAlarm)alarmId).ToString(),
                    }
                });

                Log.Error($"[{(int)(EAlarm)alarmId}] {(EAlarm)alarmId}");
                _machineStatus.Message = $"[{(int)(EAlarm)alarmId}] {(EAlarm)alarmId}";

                raisedAlarmCode = alarmId;
                ProcessMode = EProcessMode.ToAlarm;
            }
        }

        private void RootProcess_WarningRaised(int warningId, string warningSource)
        {
            lock (_lockAlarm)
            {
                if (this.IsInWarningMode() || this.IsInAlarmMode()) return;

                // TODO: CIM
                CIMScenarioDispatcher.ExecuteScenario(CIMScenario.AlarmOccur, new CIMScenarioContext
                {
                    CIMAlarmData = new CIMAlarmData
                    {
                        ALCD = 2, // HEAVY ALARM
                        ALID = warningId,
                        AlarmDescription = ((EWarning)warningId).ToString(),
                    }
                });

                Log.Warn($"[{(int)(EWarning)warningId}] {(EWarning)warningId}");
                _machineStatus.Message = $"[{(int)(EWarning)warningId}] {(EWarning)warningId}";

                raisedWarningCode = warningId;
                ProcessMode = EProcessMode.ToWarning;
            }
        }

        private bool IsAutoRunWaitingNoPanel()
        {
            return _processIO.InjectProcOutput[EInjectProcOutput.SPDHeadWorkRequest].Value == false;
        }

        private Queue<IGrouping<uint, PositionPoint>> MoveMultiPointQueueSteps = new Queue<IGrouping<uint, PositionPoint>>();
        private List<PositionPoint> currentPoints = new List<PositionPoint>();
#endregion
    }
}
