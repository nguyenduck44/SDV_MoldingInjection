using EQX.Core.Process;
using EQX.TemplateProject.Defines;

namespace EQX.TemplateProject.Process
{
    public class Processes
    {
        private readonly List<IProcess<ESequence>> _processes;

        #region Properties
        public IProcess<ESequence> RootProcess => _processes.First(p => p.Name == EProcess.Root.ToString());
        public IProcess<ESequence> TransferProcess => _processes.First(p => p.Name == EProcess.Transfer.ToString());
        public IProcess<ESequence> ShuttleProcess => _processes.First(p => p.Name == EProcess.Shuttle.ToString());
        #endregion

        public Processes(List<IProcess<ESequence>> processes)
        {
            _processes = processes;
        }

        public void Initialize()
        {
            // Initialize the processes
            RootProcess.AddChild(TransferProcess);
            RootProcess.AddChild(ShuttleProcess);

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
