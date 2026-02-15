using EQX.Core.Common;
using EQX.Core.Robot;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.MVVM.ViewModels
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
