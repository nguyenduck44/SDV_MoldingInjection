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
                services.AddKeyedScoped<IProcess<ESequence>, RootProcess<ESequence, ESemiSequence>>(EProcess.Root.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, StageProcess>(EProcess.StageLeft.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, StageProcess>(EProcess.StageRight.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, NozzleCleanProcess>(EProcess.NozzleClean.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, DispenserProcess>(EProcess.Dispenser.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, VisionInspectionProcess>(EProcess.VisionInspection.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, UVProcess>(EProcess.UV.ToString());
                services.AddKeyedScoped<IProcess<ESequence>, TransferProcess>(EProcess.Transfer.ToString());

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
