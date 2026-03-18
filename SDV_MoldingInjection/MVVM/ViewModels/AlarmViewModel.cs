using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AlarmViewModel : ViewModelBase
    {
        private readonly IViewModelFactory _viewModelFactory;

        public AlarmViewModel(IViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
            NavigateErrorViewCommand.Execute(null);
        }
        public ViewModelBase CurrentVM { get; set; }

        public ICommand NavigateErrorViewCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CurrentVM = _viewModelFactory.Create<ErrorViewModel>();
                    OnPropertyChanged(nameof(CurrentVM));
                    OnPropertyChanged(nameof(IsErrorView));
                    OnPropertyChanged(nameof(IsLogView));
                });
            }
        }

        public ICommand NavigateLogViewCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    CurrentVM = _viewModelFactory.Create<LogViewModel>();
                    OnPropertyChanged(nameof(CurrentVM));
                    OnPropertyChanged(nameof(IsErrorView));
                    OnPropertyChanged(nameof(IsLogView));
                });
            }
        }

        public bool IsErrorView => CurrentVM is ErrorViewModel;
        public bool IsLogView => CurrentVM is LogViewModel;
    }
}
