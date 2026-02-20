using EQX.Core.Device.SpeedController;
using EQX.Core.InOut;
using EQX.Core.TorqueController;
using EQX.InOut.InOut;
using EQX.InOut.InOut.Analog;
using EQX.InOut.Virtual;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using System.IO;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddDeviceExtension
    {
        public static IHostBuilder AddDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.AddMotionDevices();
            hostBuilder.AddIODevices();
            hostBuilder.AddCylinderDevices();

            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<InterlockService>();
            });

            return hostBuilder;
        }

    }
}
