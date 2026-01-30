using EQX.Core.Process;
using SDV_DotDispenser.Defines;

namespace SDV_DotDispenser.Process
{
    public class Processes
    {
        private readonly List<IProcess<ESequence>> _processes;

        #region Properties
        public IProcess<ESequence> RootProcess => _processes.First(p => p.Name == EProcess.Root.ToString());
        public IProcess<ESequence> StageLeftProcess => _processes.First(p => p.Name == EProcess.StageLeft.ToString());
        public IProcess<ESequence> StageRightProcess => _processes.First(p => p.Name == EProcess.StageRight.ToString());
        public IProcess<ESequence> NozzleCleanProcess => _processes.First(p => p.Name == EProcess.NozzleClean.ToString());
        public IProcess<ESequence> DispenserProcess => _processes.First(p => p.Name == EProcess.Dispenser.ToString());
        public IProcess<ESequence> VisionInspectionProcess => _processes.First(p => p.Name == EProcess.VisionInspection.ToString());
        public IProcess<ESequence> UVProcess => _processes.First(p => p.Name == EProcess.UV.ToString());
        public IProcess<ESequence> TransferProcess => _processes.First(p => p.Name == EProcess.Transfer.ToString());
        #endregion

        public Processes(List<IProcess<ESequence>> processes)
        {
            _processes = processes;
        }

        public void Initialize()
        {
            // Initialize the processes
            RootProcess.AddChild(StageLeftProcess);
            RootProcess.AddChild(StageRightProcess);
            RootProcess.AddChild(NozzleCleanProcess);
            RootProcess.AddChild(DispenserProcess);
            RootProcess.AddChild(VisionInspectionProcess);
            RootProcess.AddChild(UVProcess);
            RootProcess.AddChild(TransferProcess);

            // Set the process hierarchy
            foreach (var process in RootProcess.Childs)
            {
                process.AlarmRaised += ((alarmId, alarmSource) =>
                {
                    RootProcess.RaiseAlarm(alarmId, alarmSource);
                });
                process.WarningRaised += ((wawrningId, wawrningSource) =>
                {
                    RootProcess.RaiseWarning(wawrningId, wawrningSource);
                });
            }

            ProcessesStart();
        }

        private void ProcessesStart()
        {
            RootProcess.Start();
            RootProcess.Childs?.All(p => p.Start());
        }

        public void ProcessesStop()
        {
            RootProcess.Stop();
            RootProcess.Childs?.All(p => p.Stop());
        }
        #region Privates
        private readonly MachineStatus _machineStatus;
        #endregion
    }
}
