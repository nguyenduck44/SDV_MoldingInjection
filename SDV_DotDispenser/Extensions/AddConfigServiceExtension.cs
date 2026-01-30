using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace SDV_DotDispenser.Extensions
{
    public static class AddConfigServiceExtension
    {
        public static IHostBuilder AddConfigs(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                services.AddSingleton<IConfiguration>(configuration);

                #region Configue Log
                FileInfo fileInfo = new FileInfo(configuration.GetValue<string>("Files:LogConfigFile"));
                XmlConfigurator.ConfigureAndWatch(fileInfo);
                #endregion
            });
            return hostBuilder;
        }
    }
}
