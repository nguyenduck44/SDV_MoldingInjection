using EQX.Core.Process;
using EQX.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddProcessExtension
    {
        public static IHostBuilder AddProcesses(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IProcess<ESequence>, RootProcess<ESequence, ESemiSequence>>();
                services.AddSingleton<IProcess<ESequence>, InjectProcess>();
                services.AddSingleton<IProcess<ESequence>, DryPumpProcess>();
                services.AddSingleton<IProcess<ESequence>, SPDHeadProcess>();
                services.AddSingleton<IProcess<ESequence>, SPDHeadProcess>();
                services.AddSingleton<IProcess<ESequence>, SPDHeadProcess>();
                services.AddSingleton<IProcess<ESequence>, SPDHeadProcess>();
                services.AddSingleton<IProcess<ESequence>, MonitoringProcess>();

                services.AddSingleton<Processes>();

                services.AddSingleton<CIMAction>();
            });
            return hostBuilder;
        }
    }
}
