using EQX.Core.Communication;
using EQX.Device.Balance;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines.Devices.Balance;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddBalanceDeviceExtension
    {
        public static IHostBuilder AddBalanceDevices(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddKeyedScoped<MettlerToledoWKC204C>("BalanceLeft", (ser, obj) =>
                {
                    return new MettlerToledoWKC204C(new SerialCommunicator(1, "BalanceLeft", "COM8", 38400));
                });
                services.AddKeyedScoped<MettlerToledoWKC204C>("BalanceRight", (ser, obj) =>
                {
                    return new MettlerToledoWKC204C(new SerialCommunicator(2, "BalanceRight", "COM7", 38400));
                });

                services.AddSingleton<Balances>();
            });
            return hostBuilder;
        }
    }
}
