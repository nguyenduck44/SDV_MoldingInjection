using EQX.Core.Common;
using EQX.Core.Robot;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using EQX.TemplateProject.Defines.Devices;
using EQX.TemplateProject.Process;

namespace EQX.TemplateProject.MVVM.ViewModels
{
    public class DevViewModel : ViewModelBase
    {
        private readonly Devices _devices;

        #region Properties
        #endregion

        public MachineStatus MachineStatus { get; }
        public string Name { get; private set; }

        public DevViewModel(
            [FromKeyedServices("RobotLoad")] IRobot robotLoad,
            [FromKeyedServices("RobotUnload")] IRobot robotUnload,
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
