using EQX.Core.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.MVVM.ViewModels;

namespace SDV_MoldingInjection.Extensions
{
    public static class AddNavigationExtension
    {
        public static IHostBuilder AddNavigations(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<NavigationButton>((s) => {
                    return new NavigationButton()
                    {
                        Label = "Auto",
                        GroupName = "Left",
                        ViewModelType = typeof(AutoViewModel),
                        RequiredRole = EPermission.Operator,
                        ImageKey = "image_auto_selected",
                        DisabledImageKey = "image_auto_normal_dark"
                    };
                });
                services.AddSingleton<NavigationButton>((s) => {
                    return new NavigationButton()
                    {
                        Label = "Manual",
                        GroupName = "Left",
                        ViewModelType = typeof(ManualViewModel),
                        RequiredRole = EPermission.Operator,
                        ImageKey = "image_manual_selected",
                        DisabledImageKey = "image_manual_normal_dark"
                    };
                });
                services.AddSingleton<NavigationButton>((s) => {
                    return new NavigationButton()
                    {
                        Label = "Teach",
                        GroupName = "Left",
                        ViewModelType = typeof(TeachViewModel),
                        RequiredRole = EPermission.Admin,
                        ImageKey = "image_setting_selected",
                        DisabledImageKey = "image_setting_normal_light"
                    };
                });
                services.AddSingleton<NavigationButton>((s) => {
                    return new NavigationButton()
                    {
                        Label = "Data",
                        GroupName = "Left",
                        ViewModelType = typeof(DataViewModel),
                        RequiredRole = EPermission.Admin,
                        ImageKey = "image_data_selected",
                        DisabledImageKey = "image_data_normal"
                    };
                });
                services.AddSingleton<NavigationButton>((s) => {
                    return new NavigationButton()
                    {
                        Label = "Monitor",
                        GroupName = "Left",
                        ViewModelType = typeof(MonitorViewModel),
                        RequiredRole = EPermission.Operator,
                        ImageKey = "image_equipstatus_selected",
                        DisabledImageKey = "image_equipstatus_normal_light"
                    };
                });
                services.AddSingleton<NavigationButton>((s) => {
                    return new NavigationButton()
                    {
                        Label = "Dev",
                        GroupName = "Left",
                        ViewModelType = typeof(DevViewModel),
                        RequiredRole = EPermission.SuperUser,
                        ImageKey = "square_auto_seleted",
                        DisabledImageKey = "square_auto_normal"
                    };
                });

                services.AddSingleton<NavigationButton>((s) => {
                    return new NavigationButton()
                    {
                        Label = "User",
                        GroupName = "Right",
                        ViewModelType = typeof(LoginViewModel),
                        RequiredRole = EPermission.Operator,
                        ImageKey = "image_user_change_selected",
                        DisabledImageKey = "image_user_change_normal_dark"
                    };
                });
                services.AddSingleton<NavigationButton>((s) => {
                    return new NavigationButton()
                    {
                        Label = "Log",
                        GroupName = "Right",
                        ViewModelType = typeof(LogViewModel),
                        RequiredRole = EPermission.Operator,
                        ImageKey = "image_log_selected",
                        DisabledImageKey = "image_log_normal"
                    };
                });

                services.AddSingleton<NavigationStore>();
                services.AddSingleton<INavigationButtonRepository, NavigationButtonRepository>();
                services.AddSingleton<INavigationService, NavigationService>();
            });
            return hostBuilder;
        }
    }
}
