using EQX.Core.Process;
using EQX.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;
using SDV_DotDispenser.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Extensions
{
    public static class AddProcessExtension
    {
        public static IHostBuilder AddProcesses(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IProcess<ESequence>, RootProcess<ESequence, ESemiSequence>>();
                services.AddSingleton<IProcess<ESequence>, StageProcess>();
                services.AddSingleton<IProcess<ESequence>, StageProcess>();
                services.AddSingleton<IProcess<ESequence>, PlasmaProcess>();
                services.AddSingleton<IProcess<ESequence>, CleanProcess>();
                services.AddSingleton<IProcess<ESequence>, DispenserProcess>();
                services.AddSingleton<IProcess<ESequence>, FinalInspectProcess>();
                services.AddSingleton<IProcess<ESequence>, UVCureProcess>();
                services.AddSingleton<IProcess<ESequence>, TransferProcess>();

                services.AddSingleton<Processes>();
            });
            return hostBuilder;
        }
    }
}
