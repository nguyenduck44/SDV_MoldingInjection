using EQX.Core.Vision.Algorithms;
using EQX.Vision.Algorithms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DotDispenser.Extensions
{
    public static class AddVisionExtension
    {
        public static IHostBuilder AddVision(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IVisionToolRepository>((ser) =>
                {
                    var configuration = ser.GetRequiredService<IConfiguration>();

                    return new VisionToolRepository(configuration["Files:VisionToolListConfigFile"]);
                });

                services.AddSingleton<IVisionFlowRepository, VisionFlowRepository>();
            });

            return hostBuilder;
        }
    }
}
