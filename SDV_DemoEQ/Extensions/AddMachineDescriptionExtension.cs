using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_DemoEQ.Defines;
using SDV_DemoEQ.Process;
using SDV_DemoEQ.Services.Interlock;

namespace SDV_DemoEQ.Extensions
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
