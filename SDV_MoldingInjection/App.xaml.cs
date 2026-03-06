using EQX.Core.Common;
using EQX.InOut.InputSimulation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Extensions;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.MVVM.Views;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Windows;

namespace SDV_MoldingInjection
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }
        
        private const string AppGuid = "E9768A42-35E0-4EBB-8B76-EDF0497CC896";
        private const string InputSimGuid = "0B996414-C1D0-4E6F-920D-2FEAC2CF89D5";
        private Mutex _mutex;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .AddConfigs()
                .AddViews()
                .AddCIM()
                .AddViewModels()
                .AddMaintenanceViewModels()
                .AddNavigations()
                .AddAuthentications()
                .AddStores()
                .AddLanguageService()
                .AddMachineDescriptions()
                .AddDevices()
                .AddProcessIO()
                .AddRecipes()
                .AddProcesses()
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            bool isNewInstance = false;
            bool isInputSimInstance = false;

            if (e.Args.Length > 0 && e.Args[0] == "OpenInputSimWindow")
            {
                isInputSimInstance = true;
            }

            try
            {
                if (isInputSimInstance)
                {
                    _mutex = new Mutex(true, InputSimGuid, out isNewInstance);
                }
                else
                {
                    _mutex = new Mutex(true, AppGuid, out isNewInstance);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Mutex create error: {ex.Message}");
                Shutdown();
                return;
            }

            if (!isNewInstance)
            {
                MessageBox.Show("Ứng dụng đã được khởi chạy trước đó");

                Shutdown();
                return;
            }

            ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
            ThreadPool.SetMinThreads(40, completionPortThreads);
            ThreadPool.GetMinThreads(out int newWorker, out _);

            await AppHost!.StartAsync();

#if SIMULATION
            if (isInputSimInstance)
            {
                Window inputSimWindow = new InputSimulationView();
                var inputSimulationVM = AppHost!.Services.GetRequiredService<IInputSimulationViewModel>();
                inputSimulationVM.SetOriginInputsCommand?.Execute(null);
                inputSimWindow.DataContext = inputSimulationVM;
                inputSimWindow.Show();
                return;
            }
            else
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                System.Diagnostics.Process.Start(exePath, "OpenInputSimWindow");
            }
#endif
            Window window = AppHost.Services.GetRequiredService<MainWindowView>();
            var viewModel = AppHost.Services.GetRequiredService<MainWindowViewModel>();
            window.DataContext = viewModel;
            window.WindowState = WindowState.Maximized;
            window.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();

            base.OnExit(e);
        }
    }
}
