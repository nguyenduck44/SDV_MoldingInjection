using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EQX.TemplateProject.Defines;
using EQX.TemplateProject.Process;
using EQX.TemplateProject.Services.Interlock;

namespace EQX.TemplateProject.Extensions
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
