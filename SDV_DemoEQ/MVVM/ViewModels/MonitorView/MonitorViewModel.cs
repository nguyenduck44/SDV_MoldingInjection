using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SDV_DemoEQ.MVVM.ViewModels
{
    public class MonitorViewModel : ViewModelBase
    {
        #region Commands
        public ICommand ManualIONavigate => new RelayCommand(navigationService.NavigateTo<MonitorIOViewModel>);
        public ICommand ManualMotionNavigate => new RelayCommand(navigationService.NavigateTo<MonitorMotionViewModel>);
        #endregion

        public MonitorViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        #region Privates
        private readonly INavigationService navigationService;
        #endregion
    }
}
