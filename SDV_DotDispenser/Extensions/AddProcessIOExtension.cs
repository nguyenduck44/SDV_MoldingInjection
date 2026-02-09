using EQX.Core.InOut;
using EQX.InOut.Virtual;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_DotDispenser.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Extensions
{
    public static class AddProcessIOExtension
    {
        public static IHostBuilder AddProcessIO(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IDInputDevice>(new MappableInputDevice<EStageProcInput> { Name = "LeftStageProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<EStageProcOutput> { Name = "LeftStageProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<EStageProcInput> { Name = "RightStageProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<EStageProcOutput> { Name = "RightStageProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<EPlasmaProcInput> { Name = "PlasmaProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<EPlasmaProcOutput> { Name = "PlasmaProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<ECleanProcInput> { Name = "CleanProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<ECleanProcOutput> { Name = "CleanProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<EDispenserProcInput> { Name = "DispenserProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<EDispenserProcOutput> { Name = "DispenserProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<EFinalInspectProcInput> { Name = "FinalInspectProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<EFinalInspectProcOutput> { Name = "FinalInspectProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<EUVProcInput> { Name = "UVProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<EUVProcOutput> { Name = "UVProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<ETransferProcInput> { Name = "TransferProcessInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<ETransferProcOutput> { Name = "TransferProcessOutput" });

                services.AddSingleton<ProcessIO>();
            });

            return hostBuilder;
        }
    }
}
