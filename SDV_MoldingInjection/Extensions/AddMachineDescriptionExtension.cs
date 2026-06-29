using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.MVVM.Models;
using System.IO;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddMachineDescriptionExtension
    {
        public static IHostBuilder AddMachineDescriptions(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                bool eBoxMainPowerConnect = false;
                if (File.Exists(configuration["Files:EBoxMainPowerConfigFile"]))
                {
                    try
                    {
                        var EBoxMainPowerConnect = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile(configuration["Files:EBoxMainPowerConfigFile"]!)
                            .Build();

                        eBoxMainPowerConnect = EBoxMainPowerConnect.GetValue<bool>("EBoxMainPowerConnect")!;
                    }
                    catch (Exception ex)
                    {

                    }
                }

                services.AddSingleton<Information>();
                services.AddSingleton<Plotter>();
                services.AddSingleton<MachineStatus>();
                services.AddSingleton<Config>((ser) =>
                {
                    return new Config(eBoxMainPowerConnect);
                });
            });

            return hostBuilder;
        }
    }
}
