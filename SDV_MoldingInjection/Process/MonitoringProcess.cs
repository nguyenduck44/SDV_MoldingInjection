using EQX.Core.Common;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.Process;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.Recipe;
using System.Windows;

namespace SDV_MoldingInjection.Process
{
    public class MonitoringProcess : ProcessBase<ESequence>
    {
        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
        private readonly Devices _devices;
        private readonly NavigationStore _navigationStore;
        private Queue<IGrouping<uint, PositionPoint>> MoveMultiPointQueueSteps = new Queue<IGrouping<uint, PositionPoint>>();
        private List<PositionPoint> currentPoints = new List<PositionPoint>();

        public MonitoringProcess(MachineStatus machineStatus,
            RecipeSelector recipeSelector,
            Devices devices,
            NavigationStore navigationStore)
        {
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;
            _devices = devices;
            _navigationStore = navigationStore;
        }

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
                    Application.Current.Dispatcher.Invoke(() =>
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

    }
}
