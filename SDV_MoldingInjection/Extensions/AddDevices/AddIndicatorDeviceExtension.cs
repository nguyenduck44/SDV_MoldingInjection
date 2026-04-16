using EQX.Core.Communication.Modbus;
using EQX.Device.Indicator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddIndicatorDeviceExtension
    {
        public static IHostBuilder AddIndicatorDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                string indicatorCOMPort = "COM2";
                try
                {
                    var COMPortconfigProcess = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(configuration["Files:COMPortConfigFile"]!)
                        .Build();

                    indicatorCOMPort = COMPortconfigProcess.GetValue<string>("IndicatorCOMPort")!;
                }
                catch (Exception ex)
                {

                }

                services.AddKeyedScoped<IModbusCommunication>("IndicatorModbusCommunication", (serviceProvider, key) =>
                {
                    return new ModbusRTUCommunication(indicatorCOMPort, 115200);
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
