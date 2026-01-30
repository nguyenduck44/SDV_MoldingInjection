using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_BIM_Line_DotDispenser.Defines;
using SDV_BIM_Line_DotDispenser.Process;
using SDV_BIM_Line_DotDispenser.Services.Interlock;

namespace SDV_BIM_Line_DotDispenser.Extensions
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
