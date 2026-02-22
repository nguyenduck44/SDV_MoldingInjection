using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddDeviceExtension
    {
        public static IHostBuilder AddDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.AddBalanceDevices();
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
