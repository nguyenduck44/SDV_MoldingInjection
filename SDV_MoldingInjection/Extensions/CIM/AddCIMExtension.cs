using EQX.Core.Communication.CIM;
using EQX.UI.MVVM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines.CIM;
using System.Windows;

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

                services.AddSingleton<CIMFunctionViewModel>((ser) =>
                {
                    var configuration = ser.GetRequiredService<IConfiguration>();
                    return new CIMFunctionViewModel(configuration["Files:CIMFunctionFile"])
                    {
                        IsUseAPC = false
                    };
                });
            });
            return hostBuilder;
        }
    }
}
