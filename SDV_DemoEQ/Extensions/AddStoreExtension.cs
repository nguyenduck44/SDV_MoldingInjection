using EQX.Core.Common;
using EQX.UI.Converters;
using EQX.UI.Language;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_DemoEQ.Defines;
using SDV_DemoEQ.Services;

namespace SDV_DemoEQ.Extensions
{
    public static class AddAuthenticationExtension
    {
        public static IHostBuilder AddAuthentications(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IUserStore, UserStore>();
                services.AddSingleton<IAuthenticationService, DotDispenserAuthenticationService>();
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
                services.AddSingleton<CellStatusToColorConverter>();

                services.AddKeyedScoped<IAlertService, AlarmService<EAlarm>>("AlarmService");
                services.AddKeyedScoped<IAlertService, WarningService<EWarning>>("WarningService");

                services.AddKeyedScoped("BlinkTimer", (s, o) => { return new ActionAssignableTimer(500); });
            });

            return hostBuilder;
        }
    }
}
