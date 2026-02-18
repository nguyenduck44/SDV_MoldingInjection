using EQX.Core.InOut;
using EQX.InOut.Virtual;
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
    public static class AddProcessIOExtension
    {
        public static IHostBuilder AddProcessIO(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IDInputDevice>(new MappableInputDevice<EInjectProcInput> { Name = "MoldProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<EInjectProcOutput> { Name = "MoldProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<EDryPumpProcInput> { Name = "DryPumpProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<EDryPumpProcOutput> { Name = "DryPumpProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<ESPDHeadProcInput> { Name = "SPDHead1_ProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<ESPDHeadProcOutput> { Name = "SPDHead1_ProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<ESPDHeadProcInput> { Name = "SPDHead2_ProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<ESPDHeadProcOutput> { Name = "SPDHead2_ProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<ESPDHeadProcInput> { Name = "SPDHead3_ProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<ESPDHeadProcOutput> { Name = "SPDHead3_ProcOutput" });

                services.AddSingleton<IDInputDevice>(new MappableInputDevice<ESPDHeadProcInput> { Name = "SPDHead4_ProcInput" });
                services.AddSingleton<IDOutputDevice>(new MappableOutputDevice<ESPDHeadProcOutput> { Name = "SPDHead4_ProcOutput" });

                services.AddSingleton<ProcessIO>();
            });

            return hostBuilder;
        }
    }
}
