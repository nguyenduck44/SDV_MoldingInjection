using EQX.Core.Common;
using EQX.InOut.InputSimulation;
using EQX.UI.MVVM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.MVVM.Views;
using SDV_MoldingInjection.Recipe;

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

                services.AddViewModel<HeaderViewModel>();
                services.AddViewModel<FooterViewModel>();
                services.AddViewModel<RightPanelViewModel>();
                services.AddViewModel<NavigateMenuViewModel>();

                services.AddViewModel<InitDeinitViewModel>();
                services.AddViewModel<OriginViewModel>();
                services.AddViewModel<AutoViewModel>();
                services.AddViewModel<ManualViewModel>();

                services.AddViewModel<TeachViewModel>();

                services.AddViewModel<DataViewModel>();
                services.AddViewModel<RecipeViewModel>();
                services.AddViewModel<MotionsConfigViewModel>();
                services.AddViewModel<AdditionalMoldingViewModel>();
                services.AddViewModel<IdlePurgeViewModel>();
                services.AddViewModel<OptionViewModel>();
                services.AddViewModel<InjectTimeViewModel>();
                services.AddViewModel<MaterialPortsViewModel>();
                services.AddViewModel<SecurityControlViewModel>();

                services.AddViewModel<MonitorViewModel>();
                services.AddViewModel<MonitorIOViewModel>();

                services.AddViewModel<ErrorViewModel>();
                services.AddViewModel<AlarmViewModel>();
                services.AddViewModel<LogViewModel>();
                services.AddViewModel<LoginViewModel>();
                services.AddViewModel<DevViewModel>();

                services.AddViewModel<MonitorIOViewModel>();
                services.AddViewModel<MonitorMotionViewModel>();
                services.AddViewModel<ProductionInforViewModel>();
                services.AddViewModel<TactTimeViewModel>();
                services.AddViewModel<OPCallMessageViewModel>();
                services.AddViewModel<InterlockMessageViewModel>();
                services.AddViewModel<TerminalMessageViewModel>();
                services.AddSingleton<MotionsStatusViewModel>((s) =>
                {
                    return new MotionsStatusViewModel()
                    {
                        AllMotions = s.GetRequiredService<Motions>().All
                    };
                });

                services.AddViewModel<CIMTestViewModel>();

                services.AddSingleton<IViewModelFactory, ViewModelFactory>();

                services.AddSingleton<IInputSimulationViewModel>(new MMFInputSimulationViewModel<EMachineInput>(
                    new List<string>
                    {
                    },
                    new List<string>
                    {
                    }
                ));
            });

            return hostBuilder;
        }

        public static IHostBuilder AddMaintenanceViewModels(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<MaintenanceViewModel<ESemiSequence, RecipeList>, InjectMaintenanceViewModel>();
                services.AddSingleton<MaintenanceViewModel<ESemiSequence, RecipeList>, DryPumpMaintenanceViewModel>();
                services.AddSingleton<MaintenanceViewModel<ESemiSequence, RecipeList>, SPDHeadMaintenanceViewModel>();
                services.AddSingleton<MaintenanceViewModel<ESemiSequence, RecipeList>, SPDHeadMaintenanceViewModel>();
                services.AddSingleton<MaintenanceViewModel<ESemiSequence, RecipeList>, SPDHeadMaintenanceViewModel>();
                services.AddSingleton<MaintenanceViewModel<ESemiSequence, RecipeList>, SPDHeadMaintenanceViewModel>();
                services.AddSingleton<SPDHeadAllMaintenanceViewModel>();
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
