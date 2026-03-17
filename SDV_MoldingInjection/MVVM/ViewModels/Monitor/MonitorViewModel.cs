using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.MVVM;
using SDV_MoldingInjection.Defines;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MonitorViewModel : ViewModelBase
    {
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
        #endregion

        public MonitorViewModel(INavigationService navigationService,
            IViewModelFactory viewModelFactory,
            Inputs inputs,
            Outputs outputs)
        {
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
            _inputs = inputs;
            _outputs = outputs;
        }

        #region Privates
        private readonly INavigationService _navigationService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly Inputs _inputs;
        private readonly Outputs _outputs;
        #endregion
    }
}
