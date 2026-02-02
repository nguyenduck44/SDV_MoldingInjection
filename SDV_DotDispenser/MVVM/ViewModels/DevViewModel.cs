using EQX.Core.Common;
using EQX.Core.Robot;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using SDV_DotDispenser.Defines.Devices;
using SDV_DotDispenser.Process;

namespace SDV_DotDispenser.MVVM.ViewModels
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
