using EQX.Core.Communication;
using EQX.Device.Balance;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using System.IO;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddBalanceDeviceExtension
    {
        public static IHostBuilder AddBalanceDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                string balance1COMPort = "COM7";
                string balance2COMPort = "COM8";
                try
                {
                    var COMPortconfigProcess = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(configuration["Files:COMPortConfigFile"]!)
                        .Build();

                    balance1COMPort = COMPortconfigProcess.GetValue<string>("Balance1_COMPort")!;
                    balance2COMPort = COMPortconfigProcess.GetValue<string>("Balance2_COMPort")!;
                }
                catch (Exception ex)
                {

                }

                services.AddKeyedScoped<MettlerToledoWKC204C>("BalanceLeft", (ser, obj) =>
                {
                    return new MettlerToledoWKC204C(new SerialCommunicator(1, "BalanceLeft", balance1COMPort, 38400));
                });
                services.AddKeyedScoped<MettlerToledoWKC204C>("BalanceRight", (ser, obj) =>
                {
                    return new MettlerToledoWKC204C(new SerialCommunicator(2, "BalanceRight", balance2COMPort, 38400));
                });

                services.AddSingleton<Balances>();
            });
            return hostBuilder;
        }
    }
}
