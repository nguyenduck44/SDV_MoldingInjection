using Microsoft.Extensions.Hosting;
using EQX.Core.Device.SpeedController;
using EQX.InOut.InOut;
using EQX.Core.TorqueController;
using EQX.InOut.InOut.Analog;

namespace SDV_BIM_Line_DotDispenser.Extensions
{
    public static class AddDeviceExtension
    {
        public static IHostBuilder AddDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.AddMotionDevices();
            hostBuilder.AddIODevices();
            hostBuilder.AddCylinderDevices();

            return hostBuilder;
        }

    }
}
