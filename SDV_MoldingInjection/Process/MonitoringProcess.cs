using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.Device.Measurement;
using EQX.InOut;
using EQX.Process;
using EQX.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.Recipe;
using System.Windows;

namespace SDV_MoldingInjection.Process
{
    public class MonitoringProcess : ProcessBase<ESequence>
    {
        #region Cylinders
        private ICylinder ChamberOpenClose => _devices.Cylinders.ChamberOpenClose;
        #endregion

        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
        private readonly Devices _devices;
        private readonly Config _config;
        private readonly NavigationStore _navigationStore;
        private readonly Accura2550CMZ3PModule _accura2550;
        private Queue<IGrouping<uint, PositionPoint>> MoveMultiPointQueueSteps = new Queue<IGrouping<uint, PositionPoint>>();
        private List<PositionPoint> currentPoints = new List<PositionPoint>();
        private Task FDCReportTask;
        public MonitoringProcess(MachineStatus machineStatus,
            RecipeSelector recipeSelector,
            Devices devices, 
            Config config,
            NavigationStore navigationStore,
            [FromKeyedServices("Accura2550")] Accura2550CMZ3PModule accura2550)
        {
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;
            _devices = devices;
            _config = config;
            _navigationStore = navigationStore;
            _accura2550 = accura2550;
            CancellationToken token = new CancellationToken();
            FDCReportTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await WriteFDC();
                    await Task.Delay(10);
                }
            }, token);
        }

        #region Properties
        public int EnergyReceived => _accura2550.EnergyReceived;
        public double MaxActivePowerTotal => _accura2550.MaxActivePowerTotal;
        public double ABVoltage => _accura2550.ABVoltage;
        public double BCVoltage => _accura2550.BCVoltage;
        public double ACurrent => _accura2550.ACurrent;
        public double BCurrent => _accura2550.BCurrent;
        #endregion

        #region Process Methods
        public override bool PreProcess()
        {
            if (_navigationStore.CurrentViewModel is not SPDHeadAllMaintenanceViewModel)
            {
                if (_machineStatus.CurrentProcessMode != EProcessMode.ToRun && _machineStatus.CurrentProcessMode != EProcessMode.Run)
                {
                    _machineStatus.IsSkipHead1 = true;
                    _machineStatus.IsSkipHead2 = true;
                    _machineStatus.IsSkipHead3 = true;
                    _machineStatus.IsSkipHead4 = true;
                    _machineStatus.ActionCount = 1;
                }
            }

            if (_devices.Inputs.OPButtonReset.Value)
            {
                if (_machineStatus.CurrentProcessMode == EProcessMode.ToWarning ||
                    _machineStatus.CurrentProcessMode == EProcessMode.Warning ||
                    _machineStatus.CurrentProcessMode == EProcessMode.ToAlarm ||
                    _machineStatus.CurrentProcessMode == EProcessMode.Alarm)
                {
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        bool isShown = Application.Current.Windows.OfType<AlertNotifyView>().Any();
                        if (isShown) Application.Current.Windows.OfType<AlertNotifyView>().ToList().ForEach(anv => anv.Close());

                        _devices.Outputs.Buzzer1On.Value = false;
                        _devices.Outputs.Buzzer2On.Value = false;
                        _devices.Outputs.Buzzer3On.Value = false;
                        _devices.Outputs.Buzzer4On.Value = false;
                        _devices.Outputs.ResetSW_Clear();
                    });
                }
            }
            return base.PreProcess();
        }
        public override bool ProcessRun()
        {
            switch (Sequence)
            {
                case ESequence.MoveMultiPoint:
                    Sequence_MoveMultiPoint();
                    break;
                default:
                    Sequence = ESequence.Stop;
                    break;
            }

            return true;
        }
        #endregion

        #region Sequence Methods
        private void Sequence_MoveMultiPoint()
        {
            switch ((EMonitoringProcessMoveMultiPointStep)Step.RunStep)
            {
                case EMonitoringProcessMoveMultiPointStep.Start:
                    if (_devices.Inputs.AutoSW.Value == false)
                    {
                        RaiseWarning(EWarning.OP_MAIN_KEY_NOT_IN_AUTO_MODE);
                        break;
                    }
                    Log.Debug("Move Target Position Start");
                    Step.RunStep++;
                    break;
                case EMonitoringProcessMoveMultiPointStep.Close_Chamber:
                    if (ChamberOpenClose.IsClose())
                    {
                        Log.Debug("Chamber Is Closed");
                        Step.RunStep = (int)EMonitoringProcessMoveMultiPointStep.Init_QueuePosition;
                        break;
                    }

                    Log.Debug($"{ChamberOpenClose} moving close");
                    ChamberOpenClose.Close();
                    Wait(_recipeSelector.CurrentRecipe.CylinderDelayTimeRecipe.ChamberOpenCloseCylinderMoveDelay,
                        () => ChamberOpenClose.IsClose());
                    Step.RunStep++;
                    break;
                case EMonitoringProcessMoveMultiPointStep.Close_Chamber_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.CY_MOLD_CHAMBER_CLOSE_FAIL);
                        break;
                    }

                    Log.Debug($"Move {ChamberOpenClose} close done");
                    Step.RunStep++;
                    break;
                case EMonitoringProcessMoveMultiPointStep.Init_QueuePosition:
                    var positionPoints = _machineStatus.MultiPointPosition.Points
                       .GroupBy(p => p.MovingOrder)
                       .OrderBy(g => g.Key)
                       .ToList();

                    MoveMultiPointQueueSteps = new Queue<IGrouping<uint, PositionPoint>>(positionPoints);

                    Step.RunStep++;
                    break;
                case EMonitoringProcessMoveMultiPointStep.QueueEmptyCheck:
                    if (MoveMultiPointQueueSteps.Count <= 0)
                    {
                        Step.RunStep = (int)EMonitoringProcessMoveMultiPointStep.End;
                        break;
                    }

                    currentPoints = MoveMultiPointQueueSteps.Dequeue().ToList();
                    Step.RunStep++;
                    break;
                case EMonitoringProcessMoveMultiPointStep.PointMove:
                    foreach (var pp in currentPoints)
                    {
                        pp.Motion.MoveAbs(pp.TargetPos);
                    }

                    Wait(_recipeSelector.CurrentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                    {
                        return currentPoints.All(pp => pp.Motion.IsOnPosition(pp.TargetPos));
                    });

                    Step.RunStep++;
                    break;
                case EMonitoringProcessMoveMultiPointStep.PointMove_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        Log.Error($"Move To {_machineStatus.MultiPointPosition.Name} Fail");
                        RaiseWarning(EWarning.MO_MOVE_TARGET_POS_FAIL);
                        break;
                    }

                    Step.RunStep = (int)EMonitoringProcessMoveMultiPointStep.QueueEmptyCheck;
                    break;
                case EMonitoringProcessMoveMultiPointStep.End:
                    Log.Info($"Move To {_machineStatus.MultiPointPosition.Name} Done");
                    _machineStatus.MultiPointPosition = new MultiPointPosition();
                    Parent!.ProcessMode = EProcessMode.ToStop;
                    break;
            }
        }
        #endregion

        #region Private Methods
        private async Task WriteFDC()
        {
            if (Parent?.ProcessMode == EProcessMode.Run && _machineStatus.EquipState.IsMasterAlive)
            {
                FDCHelper.WriteFDCValue(EFDCValue.FUNCTION_STEP_ID, 2);
            }
            else if (Parent?.ProcessMode != EProcessMode.Run && _machineStatus.EquipState.IsMasterAlive)
            {
                FDCHelper.WriteFDCValue(EFDCValue.FUNCTION_STEP_ID, 1);
            }
            else
            {
                FDCHelper.WriteFDCValue(EFDCValue.FUNCTION_STEP_ID, 0);
            }
#if !SIMULATION
            if (_config.IsConnectEBoxMainPower)
            {
                Application.Current?.Dispatcher.Invoke(() =>
                {
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_T_PW, EnergyReceived * 1000);
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_I_PW, (int)(MaxActivePowerTotal * 1000));
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_R_VOL, (int)(ABVoltage * 10));
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_T_VOL, (int)(BCVoltage * 10));
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_R_CUR, (int)(ACurrent * 1000));
                    FDCHelper.WriteFDCValue(EFDCValue.N02_U_T_CUR, (int)(BCurrent * 1000));

                    if (_accura2550.IsConnected == false)
                    {
                        Log.Warn("Accura Main Power Reconnect");
                        _accura2550.Connect();
                    }
                });
            }
#endif
        }
        #endregion
    }
}
