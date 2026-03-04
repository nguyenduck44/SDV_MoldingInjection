using EQX.Core.Communication.Modbus;
using EQX.Device.Indicator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddIndicatorDeviceExtension
    {
        public static IHostBuilder AddIndicatorDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddKeyedScoped<IModbusCommunication>("IndicatorModbusCommunication", (serviceProvider, key) =>
                {
                    return new ModbusRTUCommunication("COM2", 115200);
                });

                services.AddKeyedSingleton<NEOSHSDIndicator>("PanelIndicator", (serviceProvider, key) =>
                {
                    return new NEOSHSDIndicator(1,"PanelIndicator", serviceProvider.GetRequiredKeyedService<IModbusCommunication>("IndicatorModbusCommunication"));
                });

            });

            return hostBuilder;
        }
    }
}
