using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.MVVM;
using SDV_MoldingInjection.Defines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MonitorViewModel : ViewModelBase
    {
        #region Commands
        public ICommand MachineIONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = outputs.Machine;
                    monitorIOViewModel.CurrentInputList = inputs.Machine;
                    navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }
        public ICommand Head1IONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = outputs.Head1;
                    monitorIOViewModel.CurrentInputList = inputs.Head1;
                    navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }
        public ICommand Head2IONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = outputs.Head2;
                    monitorIOViewModel.CurrentInputList = inputs.Head2;
                    navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }
        public ICommand Head3IONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = outputs.Head3;
                    monitorIOViewModel.CurrentInputList = inputs.Head3;
                    navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }
        public ICommand Head4IONavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var monitorIOViewModel = viewModelFactory.Create<MonitorIOViewModel>();
                    monitorIOViewModel.CurrentOutputList = outputs.Head4;
                    monitorIOViewModel.CurrentInputList = inputs.Head4;
                    navigationService.NavigateTo<MonitorIOViewModel>();
                });
            }
        }

        public ICommand OPCallMsgNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    navigationService.NavigateTo<OPCallMessageViewModel>();
                });
            }
        }

        public ICommand InterlockMsgNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    navigationService.NavigateTo<InterlockMessageViewModel>();
                });
            }
        }

        public ICommand TerminalMessageNavigateCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    navigationService.NavigateTo<TerminalMessageViewModel>();
                });
            }
        }
        #endregion

        public MonitorViewModel(INavigationService navigationService,
            IViewModelFactory viewModelFactory,
            Inputs inputs,
            Outputs outputs)
        {
            this.navigationService = navigationService;
            this.viewModelFactory = viewModelFactory;
            this.inputs = inputs;
            this.outputs = outputs;
        }

        #region Privates
        private readonly INavigationService navigationService;
        private readonly IViewModelFactory viewModelFactory;
        private readonly Inputs inputs;
        private readonly Outputs outputs;
        #endregion
    }
}
