using EQX.Core.Process;
using EQX.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_BIM_Line_DotDispenser.Defines;
using SDV_BIM_Line_DotDispenser.Defines.Devices;
using SDV_BIM_Line_DotDispenser.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_BIM_Line_DotDispenser.Extensions
{
    public static class AddProcessExtension
    {
        public static IHostBuilder AddProcesses(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddKeyedScoped<IProcess<ESequence>, RootProcess<ESequence, ESemiSequence>>(EProcess.Root.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, TransferProcess>(EProcess.Transfer.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, ShuttleProcess>(EProcess.Shuttle.ToString());

                services.AddSingleton((ser) =>
                {
                    List<IProcess<ESequence>> processList = new List<IProcess<ESequence>>();

                    foreach (EProcess process in Enum.GetValues(typeof(EProcess)))
                    {
                        var proc = ser.GetKeyedService<IProcess<ESequence>>(process.ToString());
                        if (proc != null)
                        {
                            ((ProcessBase<ESequence>)proc).Name = process.ToString();
                            processList.Add(proc);
                        }
                    }
                    return new Processes(processList);
                });

            });
            return hostBuilder;
        }
    }
}
