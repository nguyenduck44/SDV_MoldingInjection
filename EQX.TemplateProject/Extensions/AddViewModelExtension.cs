using EQX.Core.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EQX.TemplateProject.Defines;
using EQX.TemplateProject.MVVM.ViewModels;
using EQX.TemplateProject.MVVM.Views;

namespace EQX.TemplateProject.Extensions
{
    public static class AddViewViewModelExtension
    {
        public static void AddViewModel<TViewModel>(this IServiceCollection services) where TViewModel : ViewModelBase
        {
            services.AddSingleton<TViewModel>();
        }

        public static IHostBuilder AddViewModels(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddViewModel<MainWindowViewModel>();

                services.AddViewModel<NavigateMenuViewModel>();
                services.AddViewModel<HeaderViewModel>();
                services.AddViewModel<FooterViewModel>();

                services.AddViewModel<InitDeinitViewModel>();
                services.AddViewModel<OriginViewModel>();
                services.AddViewModel<AutoViewModel>();
                services.AddViewModel<ManualViewModel>();
                services.AddViewModel<DataViewModel>();
                services.AddViewModel<TeachViewModel>();
                services.AddViewModel<VisionViewModel>();
                services.AddViewModel<IOMonitoringViewModel>();

                services.AddViewModel<LogViewModel>();
                services.AddViewModel<LoginViewModel>();
                services.AddViewModel<DevViewModel>();

                services.AddSingleton<ViewModelNavigationStore>();
                services.AddTransient<ViewModelProvider>();
                services.AddTransient<INavigationService, NavigationService>();
            });

            return hostBuilder;
        }

        public static IHostBuilder AddViews(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<MainWindowView>();
            });

            return hostBuilder;
        }
    }
}
