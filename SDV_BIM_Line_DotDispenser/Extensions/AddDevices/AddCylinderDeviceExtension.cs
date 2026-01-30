using EQX.Core.InOut;
using EQX.InOut;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_BIM_Line_DotDispenser.Defines.Devices.Cylinder;

namespace SDV_BIM_Line_DotDispenser.Extensions
{
    public static class AddCylinderDeviceExtension
    {
        public static IHostBuilder AddCylinderDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
#if SIMULATION
                services.AddSingleton<ICylinderFactory, SimulationCylinderFactory>();
#else
                services.AddSingleton<ICylinderFactory, CylinderFactory>();
#endif
                services.AddSingleton<Cylinders>();
            });

            return hostBuilder;
        }
    }
}
