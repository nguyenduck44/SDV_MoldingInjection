using EQX.Core.Common;
using EQX.Core.Sequence;
using EQX.Process;
using EQX.UI.Controls;
using EQX.UI.Language;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.MVVM.ViewModels;
using System.Windows;

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
        private int raisedAlarmCode = -1;
        private int raisedWarningCode = -1;
        private readonly IAlertService _alarmService;
        private readonly IAlertService _warningService;
        private readonly object _lockAlarm = new object();

        private bool DoorClose
        {
            get
            {
                return _devices.Inputs.DoorClose;
            }
        }

        private bool IsMainAirSupplied => _devices.Inputs.MainCDACheck.Value;
        private bool IsServoOn => _devices.Inputs.ServoOn.Value;
        #endregion

        #region Constructor
        public RootProcess(Devices devices,
            MachineStatus machineStatus,
            NavigationStore viewModelavigationStore,
            [FromKeyedServices("AlarmService")] IAlertService alarmService,
            [FromKeyedServices("WarningService")] IAlertService warningService)
        {
            _devices = devices;
            _machineStatus = machineStatus;
            _viewModelavigationStore = viewModelavigationStore;
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
                        command = EOperationCommand.Ready;
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
                else if ((_machineStatus.OPCommand == EOperationCommand.Ready
                    || _machineStatus.OPCommand == EOperationCommand.Start
                    || _devices.Inputs.OPButtonStart.Value == true
                    || _devices.Inputs.OPButtonReset.Value == true)
                    && _viewModelavigationStore.CurrentViewModel is AutoViewModel)
                {
                    MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_ResetAlarmBeforeRun"], (string)Application.Current.Resources["str_Confirm"]);
                    _machineStatus.OPCommand = EOperationCommand.None;
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

                _devices.Outputs.Lamp_Alarm(true);

                ProcessMode = EProcessMode.Alarm;
                Log.Info("ToAlarm Done, Alarm");
                AlertNotifyView.ShowDialog(_alarmService.GetById(raisedAlarmCode), true);
                raisedAlarmCode = -1;
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
                raisedWarningCode = -1;
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
                    _devices.Outputs.Buzzer1On.Value = false;
                    Step.OriginStep++;
                    break;
                case ERootProcToOriginStep.DoorSensorCheck:
                    if (_devices.Inputs.DoorClose == false)
                    {
                        //WARNING
                        RaiseWarning((int)EWarning.DoorOpen);
                        break;
                    }

                    Log.Debug("Doors closed.");
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
                ProcessMode = EProcessMode.Stop;

                _devices.Motions.All.ForEach(r => r.Stop());
                _devices.Outputs.Lamp_Stop();
                _devices.Outputs.EQPStop.Value = true;
                Log.Info("ToStop Done, Stop");
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
                    _devices.Outputs.Buzzer1On.Value = false;
                    break;
                case ERootProcToRunStep.DoorSensorCheck:
                    //if (_machineStatus.IsByPassMode)
                    //{
                    //    Log.Warn("ByPass mode active - skipping door sensor check during run.");
                    //    Step.ToRunStep++;
                    //    break;
                    //}

                    //if (DoorSensor == false)
                    //{
                    //    RaiseWarning((int)EWarning.DoorOpen);
                    //    break;
                    //}
                    Log.Debug("Doors closed.");
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
            //if (IsPowerMCOn == false)
            //{
            //    RaiseAlarm((int)EAlarm.PowerMCOff);
            //    Childs!.ToList().ForEach(p => p.IsAlarm = true);
            //    Childs!.ToList().ForEach(p => p.IsCanStop = true);
            //    return;
            //}

            if (_devices.Inputs.Emergency.Value == true)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);
                Childs!.ToList().ForEach(p => p.IsCanStop = true);
                RaiseAlarm((int)EAlarm.EmergencyStopActivated);
                return;
            }

            //if (IsMainAirSupplied == false)
            //{
            //    Childs!.ToList().ForEach(p => p.IsAlarm = true);
            //    Childs!.ToList().ForEach(p => p.IsCanStop = true);
            //    RaiseAlarm((int)EAlarm.MainAirNotSupplied);
            //    return;
            //}

            if (_devices.Motions.All.Count(motion => motion.Status.IsMotionOn != true) > 0 &&
                (ProcessMode == EProcessMode.ToRun || ProcessMode == EProcessMode.Run))
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);

                _devices.Motions.All.Where(m => m.Status.IsMotionOn == false).ToList().ForEach(motion =>
                {
                    Log.Error($"{motion.Name} Motion Off");
                });
                RaiseAlarm((int)EAlarm.Motion_Driver_Off);
            }
            if (_devices.Motions.All.Count(motion => motion.Status.HwNegLimitDetect == true || motion.Status.HwPosLimitDetect == true) > 0
                && ProcessMode != EProcessMode.Origin && ProcessMode != EProcessMode.ToOrigin
                && ProcessMode != EProcessMode.ToWarning && ProcessMode != EProcessMode.Warning
                && ProcessMode != EProcessMode.ToAlarm && ProcessMode != EProcessMode.Alarm
                && ProcessMode != EProcessMode.None)
            {
                _devices.Motions.All.Where(m => m.Status.HwNegLimitDetect == true).ToList().ForEach(motion =>
                {
                    Log.Error($"{motion.Name} Limit (-) Detect");
                });
                _devices.Motions.All.Where(m => m.Status.HwPosLimitDetect == true).ToList().ForEach(motion =>
                {
                    Log.Error($"{motion.Name} Limit (+) Detect");
                });

                RaiseAlarm((int)EAlarm.Motion_Limit_Detected);
            }
            if (_devices.Motions.All.Count(motion => motion.Status.IsAlarm == true) > 0)
            {
                Childs!.ToList().ForEach(p => p.IsAlarm = true);

                _devices.Motions.All.Where(m => m.Status.IsAlarm == true).ToList().ForEach(motion =>
                {
                    Log.Error($"{motion.Name} Motion Alarm");
                });
                RaiseAlarm((int)EAlarm.Motion_Alarm_Detected);
            }
        }

        //private EOperationCommand GetUserCommand()
        //{
        //    if (_systemState.CommandState.HMICommand == EOperationCommand.Stop ||
        //        _devices.Inputs.StopSW.Value == true)
        //    {
        //        return EOperationCommand.Stop;
        //    }
        //    if (_systemState.CommandState.HMICommand == EOperationCommand.Stop ||
        //        _devices.Inputs.StopSW.Value == true)
        //    {
        //        return EOperationCommand.Stop;
        //    }

        //    return EOperationCommand.None;
        //}

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
                        return;
                    }

                    if (_machineStatus.OriginDone == false)
                    {
                        MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_MachineNeedToBeOriginBeforeRun"]);
                        _machineStatus.OPCommand = EOperationCommand.None;
                        return;
                    }

                    Sequence = (ESequence)Enum.Parse(typeof(ESequence), _machineStatus.SemiAutoSequence.ToString());

                    foreach (var process in Childs!)
                    {
                        process.ProcessStatus = EProcessStatus.None;
                        process.Sequence = Sequence;
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
                if (this.IsInAlarmMode()) return;

                Log.Error($"{alarmSource} raising alarm [#{(int)(EAlarm)alarmId}] {(EAlarm)alarmId}");
                raisedAlarmCode = alarmId;
                ProcessMode = EProcessMode.ToAlarm;
            }
        }

        private void RootProcess_WarningRaised(int warningId, string warningSource)
        {
            lock (_lockAlarm)
            {
                if (this.IsInWarningMode() || this.IsInAlarmMode()) return;

                Log.Warn($"{warningSource} raising warning [#{(int)(EWarning)warningId}] {(EWarning)warningId}");

                raisedWarningCode = warningId;
                ProcessMode = EProcessMode.ToWarning;
            }
        }
        #endregion
    }
}
