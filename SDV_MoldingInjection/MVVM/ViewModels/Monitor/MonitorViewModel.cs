using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.MVVM;
using SDV_MoldingInjection.Defines;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MonitorViewModel : ViewModelBase
    {
        public bool IsSuperUserPermission => _userStore.Permission == EPermission.SuperUser;
        public bool IsConnectEBoxMainPower => _config.IsConnectEBoxMainPower;

        #region Commands
        public ICommand ProductionInforNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<ProductionInforViewModel>();
                });
            }
        }

        public ICommand TactTimeNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<TactTimeViewModel>();
                });
            }
        }

        public ICommand MachineIONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = _viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = _outputs.Machine;
                    monitorIOViewModel.CurrentInputList = _inputs.Machine;
                    _navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }
        public ICommand Head1IONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = _viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = _outputs.Head1;
                    monitorIOViewModel.CurrentInputList = _inputs.Head1;
                    monitorIOViewModel.SelectedInputDeviceIndex = 0;
                    monitorIOViewModel.SelectedOutputDeviceIndex = 0;
                    _navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }
        public ICommand Head2IONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = _viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = _outputs.Head2;
                    monitorIOViewModel.CurrentInputList = _inputs.Head2;
                    monitorIOViewModel.SelectedInputDeviceIndex = 0;
                    monitorIOViewModel.SelectedOutputDeviceIndex = 0;
                    _navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }
        public ICommand Head3IONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = _viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = _outputs.Head3;
                    monitorIOViewModel.CurrentInputList = _inputs.Head3;
                    monitorIOViewModel.SelectedInputDeviceIndex = 0;
                    monitorIOViewModel.SelectedOutputDeviceIndex = 0;
                    _navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }
        public ICommand Head4IONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = _viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = _outputs.Head4;
                    monitorIOViewModel.CurrentInputList = _inputs.Head4;
                    monitorIOViewModel.SelectedInputDeviceIndex = 0;
                    monitorIOViewModel.SelectedOutputDeviceIndex = 0;
                    _navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }

        public ICommand OPCallMsgNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<OPCallMessageViewModel>();
                });
            }
        }

        public ICommand InterlockMsgNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<InterlockMessageViewModel>();
                });
            }
        }

        public ICommand HistoryDataNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<HistoryDataViewModel>();
                });
            }
        }

        public ICommand TerminalMessageNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<TerminalMessageViewModel>();
                });
            }
        }

        public ICommand MotionsStatusNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<MotionsStatusViewModel>();
                });
            }
        }

        public ICommand CDAStatusNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<AnalogStatusViewModel>();
                });
            }
        }

        public ICommand ProcessesStatusNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<ProcessesMonitoringViewModel>();
                });
            }
        }

        public ICommand Accura2550DNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<EBoxMainPowerMonitoringViewModel>();
                });
            }
        }

        public ICommand InterfaceInOutNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<InterfaceInOutViewModel>();
                });
            }
        }
        #endregion

        public MonitorViewModel(INavigationService navigationService,
            IViewModelFactory viewModelFactory,
            Inputs inputs,
            Outputs outputs,
            IUserStore userStore,
            MachineStatus machineStatus,
            Config config)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
            _inputs = inputs;
            _outputs = outputs;
            _userStore = userStore;
            _machineStatus = machineStatus;
            _config = config;
        }

        #region Privates
        private readonly INavigationService _navigationService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly Inputs _inputs;
        private readonly Outputs _outputs;
        private readonly IUserStore _userStore;
        private readonly MachineStatus _machineStatus;
        private readonly Config _config;
        #endregion
    }
}
