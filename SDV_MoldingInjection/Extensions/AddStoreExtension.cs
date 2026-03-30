using EQX.Core.Common;
using EQX.UI.Converters;
using EQX.UI.Language;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Productions;
using SDV_MoldingInjection.Services;
using SDV_MoldingInjection.Services.Security;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddAuthenticationExtension
    {
        public static IHostBuilder AddAuthentications(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IUserStore, UserStore>();
                services.AddSingleton<IAuthenticationService, SDVAuthenticationService>();
            });
            return hostBuilder;
        }
    }

    public static class AddStoreExtension
    {
        public static IHostBuilder AddStores(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ISecurityControlStore, SecurityControlStore>();
                services.AddSingleton<CellStatusToColorConverter>();
                services.AddSingleton<SyringAmountStatusList>();
                services.AddSingleton<CarrierJigStatusList>();
                services.AddSingleton<ProductionService>();

                services.AddKeyedScoped<IAlertService, AlarmService<EAlarm>>("AlarmService");
                services.AddKeyedScoped<IAlertService, WarningService<EWarning>>("WarningService");

                services.AddKeyedScoped("BlinkTimer", (s, o) => { return new ActionAssignableTimer(500); });
            });

            return hostBuilder;
        }
    }
}
