using Accessibility;
using EQX.Core.Common;
using EQX.Core.Communication;
using EQX.Core.Communication.CIM;
using EQX.Core.Communication.CIM.Custom;
using EQX.Core.Helpers;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.Process;
using EQX.UI.Controls;
using EQX.UI.Language;
using EQX.UI.MVVM;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.Defines.ErrorLog;
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
        private readonly InOutHandler _inOutHandler;
        private readonly CDAStatus _cDAStatus;
        private readonly CIMCollection _cIMCollection;
        private int raisedAlarmCode = 2;
        private int raisedWarningCode = 2;
        private readonly IAlertService _alarmService;
        private readonly IAlertService _warningService;
        private readonly object _lockAlarm = new object();

        private EMCCUnit CurrentAlarmUnit { get; set; }
        #endregion

        #region Constructor
        public RootProcess(Devices devices,
            MachineStatus machineStatus,
            NavigationStore viewModelavigationStore,
            INavigationService navigationService,
            RecipeSelector recipeSelector,
            RecipeList recipeList,
            ProcessIO processIO,
            InOutHandler inOutHandler,
            CDAStatus cDAStatus,
            CIMCollection cIMCollection,
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
            _inOutHandler = inOutHandler;
            _cDAStatus = cDAStatus;
            _cIMCollection = cIMCollection;
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
            if (_machineStatus.CurrentAlarms.Count == 0)
            {
                //EquipEventHelpers.ClearAlarm();
            }

            // 1. CHECK ALARM STATUS (Utils, Motion, Safety...)
            CheckRealTimeAlarmStatus();

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
                            _recipeSelector.CurrentRecipe.IdlePurgeRecipe.IdlePurgeAfterStopTime != 0)
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

                List<SPDHeadProcess> SPDProcesses = Childs!.OfType<SPDHeadProcess>().ToList();
                InjectProcess injectProcess = Childs!.OfType<InjectProcess>().First();
                if ((SPDProcesses.Any(spdProcess => spdProcess.Sequence == ESequence.ResinInject && spdProcess.Step.RunStep > (int)ESPDHeadProcCommonStep.WorkRequest_Wait) ||
                    injectProcess.Sequence == ESequence.DummyShot || injectProcess.Sequence == ESequence.NeedleCleaning ||
                    injectProcess.Sequence == ESequence.IdlePurge) == false)
                {
                    if (_machineStatus.OPCommand == EOperationCommand.SemiAuto)
                    {
                        command = EOperationCommand.SemiAuto;
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
                        && (_viewModelavigationStore.CurrentViewModel is AutoViewModel
                        && _machineStatus.IsDoorPasswordVerified))
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
                WriteMCC_ALARM_Start();

                foreach (var motion in _devices.Motions.All!) { motion.Stop(); }

                _machineStatus.OriginDone = false;

                _machineStatus.EquipState.IsAvailable = false;

                _machineStatus.MachineIdleTick = Environment.TickCount;

                _devices.Outputs.Lamp_Alarm(true);

                ProcessMode = EProcessMode.Alarm;
                _inOutHandler.Handler_SendError(raisedAlarmCode);
                Log.Info("ToAlarm Done, Alarm");
                AlertNotifyView.ShowDialog(_alarmService.GetById(raisedAlarmCode), true);

                WriteMCC_ALARM_End();

                if (_viewModelavigationStore.CurrentViewModel is IdlePurgeModeViewModel)
                {
                    _navigationService.NavigateTo<AutoViewModel>();
                }

                // TODO: CIM
                //CIMScenarioDispatcher.ExecuteScenario(CIMScenario.AlarmRelease, new CIMScenarioContext
                //{
                //    CIMAlarmData = new CIMAlarmData
                //    {
                //        ALCD = 2, // LIGHT ALARM
                //        ALID = raisedWarningCode,
                //        AlarmDescription = ((EAlarm)raisedWarningCode).ToString(),
                //    }
                //});

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
                WriteMCC_ALARM_Start();

                foreach (var motion in _devices.Motions.All!) { motion.Stop(); }

                _devices.Outputs.Lamp_Alarm(true);
                ProcessMode = EProcessMode.Warning;
                _inOutHandler.Handler_SendError(raisedWarningCode);
                Log.Info("ToWarning Done, Warning");
                AlertNotifyView.ShowDialog(_warningService.GetById(raisedWarningCode), true);

                WriteMCC_ALARM_End();

                if (_viewModelavigationStore.CurrentViewModel is IdlePurgeModeViewModel)
                {
                    _navigationService.NavigateTo<AutoViewModel>();
                }
                _machineStatus.EquipState.IsAvailable = false;

                _machineStatus.MachineIdleTick = Environment.TickCount;

                // TODO: CIM
                //CIMScenarioDispatcher.ExecuteScenario(CIMScenario.AlarmRelease, new CIMScenarioContext
                //{
                //    CIMAlarmData = new CIMAlarmData
                //    {
                //        ALCD = 1, // LIGHT ALARM
                //        ALID = raisedWarningCode,
                //        AlarmDescription = ((EWarning)raisedWarningCode).ToString(),
                //    }
                //});

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
                        RaiseWarning(EWarning.OP_MAIN_KEY_NOT_IN_AUTO_MODE);
                        break;
                    }

                    if (_machineStatus.IsAutoMode == false)
                    {
                        RaiseWarning(EWarning.OP_MAIN_MACHINE_NOT_IN_AUTO_MODE);
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
                case ERootProcToOriginStep.InOutMachine_DoorClose_Check:
                    if (_devices.Inputs.InOutMachineDoorClose == false)
                    {
                        RaiseWarning((int)EWarning.DO_INOUT_MACHINE_DOOR_OPEN);
                        break;
                    }

                    Log.Debug("InOut machine doors closed.");
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

                    _cIMCollection.WriteMCC_PPID(_recipeSelector.RecipeSetting.CurrentRecipe);
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.ChildsToOriginDone_Wait:
                    if (Childs!.Count(child => child.ProcessStatus != EProcessStatus.ToOriginDone) != 0)
                    {
                        Wait(10);
                        break;
                    }

                    Wait(300);
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
                        RaiseWarning(EWarning.OP_MAIN_KEY_NOT_IN_AUTO_MODE);
                        break;
                    }

                    if (_machineStatus.IsAutoMode == false)
                    {
                        RaiseWarning(EWarning.OP_MAIN_MACHINE_NOT_IN_AUTO_MODE);
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
                case ERootProcToRunStep.InOutMachine_DoorClose_Check:
                    if (_devices.Inputs.InOutMachineDoorClose == false)
                    {
                        RaiseWarning((int)EWarning.DO_INOUT_MACHINE_DOOR_OPEN);
                        break;
                    }

                    Log.Debug("InOut machine doors closed.");
                    Step.ToRunStep++;
                    break;
                case ERootProcToRunStep.ChildsToRunDone_Wait:
                    if (Childs!.Count(child => child.ProcessStatus != EProcessStatus.ToRunDone) != 0)
                    {
                        Wait(10);
                        break;
                    }

                    _cIMCollection.WriteMCC_PPID(_recipeSelector.RecipeSetting.CurrentRecipe);
                    Wait(300);
                    Step.ToRunStep++;
                    break;
                case ERootProcToRunStep.End:
                    _devices.Outputs.Lamp_Run();

                    // TODO: CIM
                    if (_recipeList.OptionRecipe.UseCIM)
                    {
                        EquipEventHelpers.ClearAlarm();
                    }
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _machineStatus.CurrentAlarms.Clear();
                    });

                    if (Sequence == ESequence.AutoRun)
                    {
                        _machineStatus.EquipState.IsAvailable = true;
                        _machineStatus.EquipState.IsInterlock = false;
                        _machineStatus.EquipState.IsRunning = true;
                    }

                    
                    _inOutHandler.Handler_SendError(0); //Clear Error
                    _machineStatus.Message = string.Empty;

                    if (Sequence == ESequence.AutoRun)
                    {
                        WriteMCC_ALARM_STOP_End();
                    }

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
            if (ProcessMode == EProcessMode.ToRun || ProcessMode == EProcessMode.Run ||
                ProcessMode == EProcessMode.ToOrigin || ProcessMode == EProcessMode.Origin)
            {
                if (_devices.Inputs.InOutMachineDoorClose == false)
                {
                    Childs!.ToList().ForEach(p => p.IsCanStop = true);
                    RaiseWarning(EWarning.DO_INOUT_MACHINE_DOOR_OPEN);
                }

                if (_devices.Inputs.DoorClose == false)
                {
                    Childs!.ToList().ForEach(p => p.IsCanStop = true);
                    RaiseWarning(EWarning.DO_MAIN_DOOR_OPEN);
                }

                if (_devices.Inputs.DoorLock == false)
                {
                    Childs!.ToList().ForEach(p => p.IsCanStop = true);
                    RaiseWarning(EWarning.DO_MAIN_DOOR_INTERLOCK_ON);
                }
            }

            if (_devices.Motions.All.Count(motion => motion.Status.HwNegLimitDetect == true || motion.Status.HwPosLimitDetect == true) > 0
                && (ProcessMode == EProcessMode.ToRun || ProcessMode == EProcessMode.Run))
            {
                _devices.Motions.All.Where(m => m.Status.HwNegLimitDetect == true).ToList().ForEach(motion =>
                {
                    //Log.Error($"{motion.Name} Limit (-) Detect");
                });
                _devices.Motions.All.Where(m => m.Status.HwPosLimitDetect == true).ToList().ForEach(motion =>
                {
                    //Log.Error($"{motion.Name} Limit (+) Detect");
                });

                RaiseAlarm((int)EAlarm.MO_MAIN_AXIS_LIMIT_DETECT);
            }

            if (_devices.Inputs.ServoOn.Value == false)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                RaiseAlarm((int)EAlarm.UT_MAIN_POWER_MC_OFF);
            }

            if (_devices.Inputs.InOutMachineEmo.Value == false)
            {
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                RaiseWarning(EWarning.EM_INOUT_MACHINE_EMS_ACTIVE);
            }

#endif
            if (_devices.Inputs.Emergency.Value == true)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                //Log.Error("Emergency Stop Activated. MC OFF");
                RaiseAlarm((int)EAlarm.EM_EMERGENCY_STOP_EMS_ACTIVED);
            }

            if (_devices.Inputs.SmokeDetectAlarm.Value == true)
            {
                //Log.Error("Panel Smoke Detected");
                RaiseAlarm((int)EAlarm.UT_MAIN_SMOKE_DETECT);
            }

            if (_devices.Inputs.TempHighWarning.Value == true)
            {
                //Log.Error("OverTemperature Detected (>=35)");
                RaiseWarning((int)EWarning.TE_MAIN_SENSOR_TEMP_OVER);
            }

            if (_devices.Inputs.TempHighAlarm.Value == true)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                //Log.Error("OverTemperature Detected (>=40)");
                RaiseAlarm((int)EAlarm.TE_MAIN_SENSOR_TEMP_OVER);
            }
#if !SIMULATION
            if (_cDAStatus.IsMainAir == false)
            {
                //Log.Error("Main Air Not Supplied");
                RaiseWarning((int)EWarning.VC_MAIN_AIR_NOT_SUPPLIED);
            }
#endif
            if (_devices.Motions.All.Count(motion => motion.Status.IsMotionOn != true) > 0 &&
                (ProcessMode == EProcessMode.ToRun || ProcessMode == EProcessMode.Run))
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);

                _devices.Motions.All.Where(m => m.Status.IsMotionOn == false).ToList().ForEach(motion =>
                {
                    //Log.Error($"{motion.Name} Motion Off");
                });
                RaiseAlarm((int)EAlarm.MO_MAIN_SERVO_PWR_OFF);
            }

            if (_devices.Motions.All.Count(motion => motion.Status.IsAlarm == true) > 0)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);

                _devices.Motions.All.Where(m => m.Status.IsAlarm == true).ToList().ForEach(motion =>
                {
                    //Log.Error($"{motion.Name} Motion Alarm");
                });
                RaiseAlarm((int)EAlarm.MO_MAIN_SERVO_ALARM_DETECT);
            }

            //if (_devices.Inputs.InOutMachineMCOn.Value == false)
            //{
            //    Childs!.ToList().ForEach(p => p.IsAlarm = true);
            //    Childs!.ToList().ForEach(p => p.IsCanStop = true);
            //    RaiseWarning((int)EWarning.UT_INOUT_MACHINE_MC_OFF);
            //}

            //if (_devices.Inputs.InOutMachineDoorOpen.Value == true)
            //{
            //    Childs!.ToList().ForEach(p => p.IsAlarm = true);
            //    Childs!.ToList().ForEach(p => p.IsCanStop = true);
            //    RaiseWarning((int)EWarning.DO_INOUT_MACHINE_DOOR_OPEN);
            //}
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
                    EquipEventDetail.Create(EquipEvent.TPMLossReady).SetPLCBitOn();

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
                    }

                    ProcessMode = EProcessMode.ToRun;

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
                UpdateCurrentAlarm(alarmId, true);

                CurrentAlarmUnit = GetMCCUnitFromName(alarmSource);

                if (this.IsInAlarmMode()) return;

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
                UpdateCurrentAlarm(warningId, false);

                CurrentAlarmUnit = GetMCCUnitFromName(warningSource);

                if (this.IsInWarningMode() || this.IsInAlarmMode()) return;

                Log.Warn($"[{(int)(EWarning)warningId}] {(EWarning)warningId}");
                _machineStatus.Message = $"[{(int)(EWarning)warningId}] {(EWarning)warningId}";

                raisedWarningCode = warningId;
                ProcessMode = EProcessMode.ToWarning;
            }
        }
        private void UpdateCurrentAlarm(int id, bool isAlarm)
        {
            ErrorLogEntry errorLogEntry = new ErrorLogEntry();
            errorLogEntry.ErrorCode = id;
            errorLogEntry.Message = isAlarm ? ((EAlarm)id).ToString() : ((EWarning)id).ToString();
            errorLogEntry.IOName = isAlarm ? ((EAlarm)id).GetDescription(true) : ((EWarning)id).GetDescription(true);
            errorLogEntry.Type = isAlarm ? "ALARM" : "WARNING";

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (_machineStatus.CurrentAlarms.Any(e => e.ErrorCode == errorLogEntry.ErrorCode) == false)
                {
                    _machineStatus.CurrentAlarms.Add(errorLogEntry);
                    if (isAlarm)
                    {
                        AddAlarm(id);
                        EquipEventHelpers.RaiseAlarm(MachineStatus.AlarmTotalWords);

                        Log.Error($"[{(int)(EAlarm)id}] {(EAlarm)id}");
                        _machineStatus.Message = $"[{(int)(EAlarm)id}] {(EAlarm)id}";
                        raisedAlarmCode = id;
                        ProcessMode = EProcessMode.ToAlarm;
                    }
                    else
                    {
                        AddAlarm(id);
                        EquipEventHelpers.RaiseAlarm(MachineStatus.AlarmTotalWords);

                        Log.Warn($"[{(int)(EWarning)id}] {(EWarning)id}");
                        _machineStatus.Message = $"[{(int)(EWarning)id}] {(EWarning)id}";
                        raisedWarningCode = id;
                        ProcessMode = EProcessMode.ToWarning;
                    }
                }
            });
        }

        private void AddAlarm(int alarmId)
        {
            if (alarmId < 0) return;

            int wordIndex = alarmId / 16;
            int bitIndex = alarmId % 16;

            MachineStatus.AlarmTotalWords[wordIndex] |= (short)(1 << bitIndex);
        }

        private EMCCUnit GetMCCUnitFromName(string processsName)
        {
            if (processsName == EProcess.Inject.ToString()) return EMCCUnit.IJ01;
            if (processsName == EProcess.DryPump.ToString()) return EMCCUnit.PU01;
            if (processsName == EProcess.SPDHead1.ToString()) return EMCCUnit.SP01;
            if (processsName == EProcess.SPDHead2.ToString()) return EMCCUnit.SP02;
            if (processsName == EProcess.SPDHead3.ToString()) return EMCCUnit.SP03;
            return EMCCUnit.SP04;
        }

        private void WriteMCC_ALARM_Start()
        {
            if (CurrentAlarmUnit == EMCCUnit.IJ01) _cIMCollection.WriteMCC_OnlyStart(EMCCAction.IJ01_ALARM);
            if (CurrentAlarmUnit == EMCCUnit.PU01) _cIMCollection.WriteMCC_OnlyStart(EMCCAction.PU01_ALARM);
            if (CurrentAlarmUnit == EMCCUnit.SP01) _cIMCollection.WriteMCC_OnlyStart(EMCCAction.SP01_ALARM);
            if (CurrentAlarmUnit == EMCCUnit.SP02) _cIMCollection.WriteMCC_OnlyStart(EMCCAction.SP02_ALARM);
            if (CurrentAlarmUnit == EMCCUnit.SP03) _cIMCollection.WriteMCC_OnlyStart(EMCCAction.SP03_ALARM);
            if (CurrentAlarmUnit == EMCCUnit.SP04) _cIMCollection.WriteMCC_OnlyStart(EMCCAction.SP04_ALARM);
        }

        private void WriteMCC_ALARM_End()
        {
            if (CurrentAlarmUnit == EMCCUnit.IJ01) _cIMCollection.WriteMCC(EMCCAction.IJ01_ALARM, EMCCAction.IJ01_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.PU01) _cIMCollection.WriteMCC(EMCCAction.PU01_ALARM, EMCCAction.PU01_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.SP01) _cIMCollection.WriteMCC(EMCCAction.SP01_ALARM, EMCCAction.SP01_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.SP02) _cIMCollection.WriteMCC(EMCCAction.SP02_ALARM, EMCCAction.SP02_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.SP03) _cIMCollection.WriteMCC(EMCCAction.SP03_ALARM, EMCCAction.SP03_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.SP04) _cIMCollection.WriteMCC(EMCCAction.SP04_ALARM, EMCCAction.SP04_ALARM_STOP);
        }

        private void WriteMCC_ALARM_STOP_End()
        {
            if (CurrentAlarmUnit == EMCCUnit.IJ01) _cIMCollection.WriteMCC_OnlyEnd(EMCCAction.IJ01_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.PU01) _cIMCollection.WriteMCC_OnlyEnd(EMCCAction.PU01_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.SP01) _cIMCollection.WriteMCC_OnlyEnd(EMCCAction.SP01_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.SP02) _cIMCollection.WriteMCC_OnlyEnd(EMCCAction.SP02_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.SP03) _cIMCollection.WriteMCC_OnlyEnd(EMCCAction.SP03_ALARM_STOP);
            if (CurrentAlarmUnit == EMCCUnit.SP04) _cIMCollection.WriteMCC_OnlyEnd(EMCCAction.SP04_ALARM_STOP);
        }

        private bool IsAutoRunWaitingNoPanel()
        {
            return _processIO.InjectProcOutput[EInjectProcOutput.SPDHeadWorkRequest].Value == false &&
                   JigId1SendOn == false &&
                   JigId2SendOn == false;
            
        }

        private Queue<IGrouping<uint, PositionPoint>> MoveMultiPointQueueSteps = new Queue<IGrouping<uint, PositionPoint>>();
        private List<PositionPoint> currentPoints = new List<PositionPoint>();
        private bool JigId1SendOn => CIMAddressMap.ReadCIMBit("B2107") == 1;
        private bool JigId2SendOn => CIMAddressMap.ReadCIMBit("B2108") == 1;
        #endregion
    }
}
