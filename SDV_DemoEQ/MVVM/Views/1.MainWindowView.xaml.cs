using EQX.Core.Common;
using System.Windows;
using SDV_DemoEQ.MVVM.ViewModels;
using SDV_DemoEQ.Process;

namespace SDV_DemoEQ.MVVM.Views
{
    public partial class MainWindowView : Window
    {
        private readonly INavigationService _navigationService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly MachineStatus _machineStatus;

        public MainWindowView(INavigationService navigationService,
            IViewModelFactory viewModelFactory,
            MachineStatus machineStatus)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
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
            _viewModelFactory.Create<InitDeinitViewModel>().Initialization();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Environment.ExitCode != 100)
            {
                e.Cancel = true;
                _viewModelFactory.Create<HeaderViewModel>().ApplicationCloseCommand.Execute(null);
            }
        }

    }
}