using EQX.Core.Common;
using EQX.Core.Robot;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using SDV_DemoEQ.Defines.Devices;
using SDV_DemoEQ.Process;

namespace SDV_DemoEQ.MVVM.ViewModels
{
    public class DevViewModel : ViewModelBase
    {
        private readonly Devices _devices;

        #region Properties
        #endregion

        public MachineStatus MachineStatus { get; }
        public string Name { get; private set; }

        public DevViewModel(
            Devices devices,
            MachineStatus machineStatus)
        {
            _devices = devices;
            MachineStatus = machineStatus;
            Log = LogManager.GetLogger("DevVM");
        }

        #region Privates
        #endregion
    }
}
