using EQX.Core.Communication.Modbus;
using EQX.Device.Indicator;
using EQX.Device.Measurement;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using System.IO;

namespace SDV_MoldingInjection.Extensions.AddDevices
{
    public static class AddAccura2550CMZ3PDevicesExtension
    {
        public static IHostBuilder AddAccura2550CMZ3PDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddKeyedScoped<IModbusCommunication>("Accura2550Communication", (serviceProvider, key) =>
                {
                    return new ModbusTcpCommunication("10.10.10.100", 502);
                });

                services.AddKeyedSingleton<Accura2550CMZ3PModule>("Accura2550", (serviceProvider, key) =>
                {
                    return new Accura2550CMZ3PModule(0, "Accura2550", serviceProvider.GetRequiredKeyedService<IModbusCommunication>("Accura2550Communication"));
                });
            });

            return hostBuilder;
        }
    }
}
