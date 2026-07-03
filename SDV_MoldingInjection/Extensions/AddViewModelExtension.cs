using EQX.Core.Common;
using EQX.InOut.InputSimulation;
using EQX.UI.Controls;
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
                services.AddViewModel<MachineStatusAutoViewModel>();
                services.AddViewModel<ManualViewModel>();
                services.AddViewModel<TeachViewModel>();
                services.AddViewModel<IdlePurgeModeViewModel>();

                #region Datas
                services.AddViewModel<DataViewModel>();
                services.AddViewModel<RecipeViewModel>();
                services.AddViewModel<MotionsConfigViewModel>();
                services.AddViewModel<AdditionalMoldingViewModel>();
                services.AddViewModel<IdlePurgeViewModel>();
                services.AddViewModel<OptionViewModel>();
                services.AddViewModel<InjectTimeViewModel>();
                services.AddViewModel<CylinderDelayTimeViewModel>();
                services.AddViewModel<CDASettingViewModel>();
                services.AddViewModel<MaterialPortsViewModel>();
                services.AddViewModel<SecurityControlViewModel>();
                #endregion

                #region Monitors
                services.AddViewModel<MonitorViewModel>();
                services.AddViewModel<MonitorIOViewModel>();
                services.AddViewModel<ProcessesMonitoringViewModel>();
                services.AddViewModel<MonitorIOViewModel>();
                services.AddViewModel<InterfaceInOutViewModel>();
                services.AddViewModel<AnalogStatusViewModel>();
                services.AddViewModel<ProductionInforViewModel>();
                services.AddViewModel<TactTimeViewModel>();
                services.AddSingleton<TactTimeListViewModel>();
                services.AddViewModel<OPCallMessageViewModel>();
                services.AddViewModel<InterlockMessageViewModel>();
                services.AddViewModel<TerminalMessageViewModel>();
                services.AddViewModel<HistoryDataViewModel>();
                services.AddViewModel<EBoxMainPowerMonitoringViewModel>();
                services.AddSingleton<MotionsStatusViewModel>((s) =>
                {
                    return new MotionsStatusViewModel()
                    {
                        AllMotions = s.GetRequiredService<Motions>().All
                    };
                });
                #endregion

                #region Alarms
                services.AddViewModel<CurrentAlarmViewModel>();
                services.AddViewModel<ErrorViewModel>();
                services.AddViewModel<AlarmViewModel>();
                services.AddViewModel<LogViewModel>();
                services.AddViewModel<LoginViewModel>();
                #endregion

                #region DEV
                services.AddViewModel<DevViewModel>();
                services.AddViewModel<CIMTestViewModel>();
                #endregion

                #region Others
                services.AddViewModel<AutoTeachModeViewModel>();
                services.AddSingleton<ILeakTestResultViewModelFactory, LeakTestResultViewModelFactory>();
                services.AddSingleton<ILeakTestResultDialogService, LeakTestResultDialogService>();
                services.AddSingleton<IViewModelFactory, ViewModelFactory>();
                services.AddSingleton<IInputSimulationViewModel>(new MMFInputSimulationViewModel<EMachineInput>(
                    new List<string>
                    {
                        EMachineInput.OP_KEY_SW_AUTO.ToString(),
                    },
                    new List<string>
                    {
                        EMachineInput.OP_KEY_SW_AUTO.ToString(),
                    }
                ));
                #endregion
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
