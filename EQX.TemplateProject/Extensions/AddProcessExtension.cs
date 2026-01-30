using EQX.Core.Process;
using EQX.Process;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EQX.TemplateProject.Defines;
using EQX.TemplateProject.Defines.Devices;
using EQX.TemplateProject.Process;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQX.TemplateProject.Extensions
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
