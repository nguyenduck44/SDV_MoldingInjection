using EQX.Core.Common;
using System.Windows;
using SDV_BIM_Line_DotDispenser.MVVM.ViewModels;
using SDV_BIM_Line_DotDispenser.Process;

namespace SDV_BIM_Line_DotDispenser.MVVM.Views
{
    public partial class MainWindowView : Window
    {
        private readonly INavigationService _navigationService;
        private readonly ViewModelProvider _viewModelProvider;
        private readonly MachineStatus _machineStatus;

        public MainWindowView(INavigationService navigationService,
            ViewModelProvider viewModelProvider,
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
            _viewModelProvider.GetViewModel<InitDeinitViewModel>().Initialization();

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Environment.ExitCode != 100)
            {
                e.Cancel = true;
                _viewModelProvider.GetViewModel<HeaderViewModel>().ApplicationCloseCommand.Execute(null);
            }
        }

    }
}