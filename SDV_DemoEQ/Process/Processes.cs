using EQX.Core.Process;
using EQX.Process;
using SDV_DemoEQ.Defines;
using System.Diagnostics;

namespace SDV_DemoEQ.Process
{
    public class Processes
    {
        public IEnumerable<IProcess<ESequence>> All { get; }

        #region Properties
        public IProcess<ESequence> RootProcess { get; }
        #endregion

        public Processes(IEnumerable<IProcess<ESequence>> processes)
        {
            All = processes;
            RootProcess = All.First(p => p.GetType() == typeof(RootProcess<ESequence, ESemiSequence>));
        }

        public void Initialize()
        {
            // Initialize the processes
            for (int i = 0; i < All.Count(); i++)
            {
                var process = All.ToList()[i];

                ((ProcessBase<ESequence>)process).Name = Enum.GetName(typeof(EProcess), i)!;

                if (process == RootProcess) continue;

                RootProcess.AddChild(process);

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
    }
}
