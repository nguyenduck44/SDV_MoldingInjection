using EQX.Core.Communication.CIM;
using EQX.UI.MVVM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines.CIM;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddCIMExtension
    {
        public static IHostBuilder AddCIM(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ICIMMapHelper, SDVCIMMapHeler>();
                services.AddSingleton<CIMAction>();

                services.AddSingleton<MaterialPort>();
                services.AddSingleton<MaterialPort>();
                services.AddSingleton<MaterialPort>();
                services.AddSingleton<MaterialPort>();

                services.AddSingleton<CIMCollection>();
            });
            return hostBuilder;
        }
    }
}
