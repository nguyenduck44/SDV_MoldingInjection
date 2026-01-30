using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Process;
using SDV_DotDispenser.Services.Interlock;

namespace SDV_DotDispenser.Extensions
{
    public static class AddMachineDescriptionExtension
    {
        public static IHostBuilder AddMachineDescriptions(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<Information>();
                services.AddSingleton<MachineStatus>();
                services.AddSingleton<InterlockService>();
            });

            return hostBuilder;
        }
    }
}
