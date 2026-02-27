using EQX.Core.Recipe;
using EQX.Core.Sequence;
using EQX.Process;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Process
{
    public class MonitoringProcess : ProcessBase<ESequence>
    {
        public MonitoringProcess(MachineStatus machineStatus,
            RecipeSelector recipeSelector)
        {
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;
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
                    Log.Info($"Move To {_machineStatus.MultiPointPosition.Name} Start");
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
                        pp.Motion.MoveAbs(pp.Value);
                    }

                    Wait(_recipeSelector.CurrentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                    {
                        return currentPoints.All(pp => pp.Motion.IsOnPosition(pp.Value));
                    });

                    Step.RunStep++;
                    break;
                case EMonitoringProcessMoveMultiPointStep.PointMove_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        Log.Error($"Move To {_machineStatus.MultiPointPosition.Name} Fail");
                        RaiseWarning(EWarning.MoveTargetPosition_Fail);
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

        private Queue<IGrouping<uint, PositionPoint>> MoveMultiPointQueueSteps = new Queue<IGrouping<uint, PositionPoint>>();
        private List<PositionPoint> currentPoints = new List<PositionPoint>();
        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
    }
}
