using EQX.Core.InOut;
using EQX.InOut;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;

namespace SDV_MoldingInjection.Extensions
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
