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
    public static class AddVirtualIOExtension 
    {
        public static IHostBuilder AddProcessIO(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddKeyedSingleton<IDInputDevice, MappableInputDevice<ETransferProcessInput>>("TransferProcessInput");
                services.AddKeyedSingleton<IDOutputDevice, MappableOutputDevice<ETransferProcessOutput>>("TransferProcessOutput");

                services.AddSingleton<VirtualIO>();

            });

            return hostBuilder;
        }
    }
}
