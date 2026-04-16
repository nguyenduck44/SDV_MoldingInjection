using EQX.Core.Communication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddInOutHandlerExtension
    {
        public static IHostBuilder AddInOutHandler(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

                string inOutHandlerCOMPort = "COM3";
                try
                {
                    var COMPortconfigProcess = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(configuration["Files:COMPortConfigFile"]!)
                        .Build();

                    inOutHandlerCOMPort = COMPortconfigProcess.GetValue<string>("InOutHandlerCOMPort")!;
                }
                catch (Exception ex)
                {

                }
                services.AddKeyedScoped<SerialCommunicator>("InOutHandlerSerialCommunication", (ser, obj) =>
                {
                    return new SerialCommunicator(3, "InOutHandlerSerialCommunication", inOutHandlerCOMPort, 115200);
                });

                services.AddSingleton<InOutHandler>();
            });
            return hostBuilder;
        }
    }
}
