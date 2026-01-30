using EQX.Core.Common;
using EQX.UI.Converters;
using EQX.UI.Language;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EQX.TemplateProject.Defines;
using EQX.TemplateProject.Services.Factories;

namespace EQX.TemplateProject.Extensions
{
    public static class AddStoreExtension
    {
        public static IHostBuilder AddStores(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<UserStore>();
                services.AddSingleton<CellStatusToColorConverter>();

                services.AddSingleton<TeachingViewModelFactory>();
                services.AddSingleton<ManualViewModelFactory>();

                services.AddKeyedScoped<IAlertService, AlarmService<EAlarm>>("AlarmService");
                services.AddKeyedScoped<IAlertService, WarningService<EWarning>>("WarningService");

                services.AddKeyedScoped("BlinkTimer", (s, o) => { return new ActionAssignableTimer(500); });
            });

            return hostBuilder;
        }
    }
}
