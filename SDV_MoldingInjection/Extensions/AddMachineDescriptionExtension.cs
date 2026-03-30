using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.MVVM.Models;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddMachineDescriptionExtension
    {
        public static IHostBuilder AddMachineDescriptions(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<Information>();
                services.AddSingleton<Plotter>();
                services.AddSingleton<MachineStatus>();
            });

            return hostBuilder;
        }
    }
}
