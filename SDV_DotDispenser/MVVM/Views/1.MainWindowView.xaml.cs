using EQX.Core.Common;
using System.Windows;
using SDV_DotDispenser.MVVM.ViewModels;
using SDV_DotDispenser.Process;

namespace SDV_DotDispenser.MVVM.Views
{
    public partial class MainWindowView : Window
    {
        private readonly INavigationService _navigationService;
        private readonly ViewModelFactory _viewModelProvider;
        private readonly MachineStatus _machineStatus;

        public MainWindowView(INavigationService navigationService,
            ViewModelFactory viewModelProvider,
            MachineStatus machineStatus)
        {
            _navigationService = navigationService;
            _viewModelProvider = viewModelProvider;
            _machineStatus = machineStatus;

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (typeof(MainWindowViewModel) != this.DataContext.GetType())
            {
                return;
            }
            _navigationService.NavigateTo<InitDeinitViewModel>();
            _viewModelProvider.Create<InitDeinitViewModel>().Initialization();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Environment.ExitCode != 100)
            {
                e.Cancel = true;
                _viewModelProvider.Create<HeaderViewModel>().ApplicationCloseCommand.Execute(null);
            }
        }

    }
}