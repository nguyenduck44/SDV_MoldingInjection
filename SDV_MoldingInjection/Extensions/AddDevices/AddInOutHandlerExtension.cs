using EQX.Core.Communication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using System;
using System.Collections.Generic;
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
                services.AddKeyedScoped<SerialCommunicator>("InOutHandlerSerialCommunication", (ser, obj) =>
                {
                    return new SerialCommunicator(3, "InOutHandlerSerialCommunication", "COM3", 115200);
                });

                services.AddSingleton<InOutHandler>();
            });
            return hostBuilder;
        }
    }
}
