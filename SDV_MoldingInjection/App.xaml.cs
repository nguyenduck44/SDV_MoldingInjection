using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SDV_MoldingInjection.Extensions;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.MVVM.Views;
using System.Windows;

namespace SDV_MoldingInjection
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        private const string AppGuid = "2341E081-7BFC-4738-85A4-CF782120EDCC";
        private Mutex _mutex;

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .AddConfigs()
                .AddViews()
                .AddViewModels()
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

            try
            {
                _mutex = new Mutex(true, AppGuid, out isNewInstance);
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
