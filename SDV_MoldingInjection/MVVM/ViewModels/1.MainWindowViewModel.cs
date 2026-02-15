using EQX.Core.Common;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using System.Windows;
using System.Windows.Threading;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties
        public ViewModelBase HeaderVM { get; }
        public ViewModelBase FooterVM { get; }

        public ViewModelBase CurrentFrameVM
        {
            get
            {
                return _navigationStore.CurrentViewModel;
            }
        }
        public Devices Devices { get; }
        #endregion

        public MainWindowViewModel(NavigationStore navigationStore,
                                   IViewModelFactory viewModelFactory,
                                   MachineStatus machineStatus,
                                   Devices devices)
        {
            _navigationStore = navigationStore;
            _viewModelFactory = viewModelFactory;
            _machineStatus = machineStatus;
            Devices = devices;
            HeaderVM = _viewModelFactory.Create<HeaderViewModel>();
            FooterVM = _viewModelFactory.Create<FooterViewModel>();

            _navigationStore.CurrentViewModelChanged += FrameNavigationStore_CurrentViewModelChanged;
        }

        #region Privates methods

        private void FrameNavigationStore_CurrentViewModelChanged()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                OnPropertyChanged(nameof(CurrentFrameVM));
            }), DispatcherPriority.DataBind);
        }

        #endregion

        #region Private fields
        private readonly ViewModelBase _maintenanceVM;

        private readonly NavigationStore _navigationStore;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly MachineStatus _machineStatus;
        #endregion
    }
}
