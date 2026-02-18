using EQX.Core.Common;
using EQX.InOut.InputSimulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.MVVM.Views;

namespace SDV_MoldingInjection.Extensions
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
                services.AddViewModel<AppManualUnitViewModel>();
                services.AddViewModel<TeachViewModel>();

                services.AddViewModel<DataViewModel>();
                services.AddViewModel<RecipeViewModel>();

                services.AddViewModel<MonitorViewModel>();
                services.AddViewModel<MonitorIOViewModel>();

                services.AddViewModel<LogViewModel>();
                services.AddViewModel<LoginViewModel>();
                services.AddViewModel<DevViewModel>();

                services.AddViewModel<MonitorIOViewModel>();
                services.AddViewModel<MonitorMotionViewModel>();

                services.AddSingleton<IViewModelFactory, ViewModelFactory>();

                services.AddSingleton<IInputSimulationViewModel>(new MMFInputSimulationViewModel<EMachineInput>(
                    new List<string>
                    {
                        EMachineInput.DOOR_CLOSE_LEFT.ToString(),
                        EMachineInput.DOOR_CLOSE_RIGHT.ToString(),
                        EMachineInput.DOOR_LOCK_LEFT.ToString(),
                        EMachineInput.DOOR_LOCK_RIGHT.ToString(),
                    },
                    new List<string>
                    {
                        EMachineInput.DOOR_CLOSE_LEFT.ToString(),
                        EMachineInput.DOOR_CLOSE_RIGHT.ToString(),
                        EMachineInput.DOOR_LOCK_LEFT.ToString(),
                        EMachineInput.DOOR_LOCK_RIGHT.ToString(),
                    }
                ));
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
