using EQX.Core.Process;
using EQX.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_DemoEQ.Defines;
using SDV_DemoEQ.Defines.Devices;
using SDV_DemoEQ.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DemoEQ.Extensions
{
    public static class AddProcessExtension
    {
        public static IHostBuilder AddProcesses(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IProcess<ESequence>, RootProcess<ESequence, ESemiSequence>>();

                services.AddSingleton<Processes>();
            });
            return hostBuilder;
        }
    }
}
