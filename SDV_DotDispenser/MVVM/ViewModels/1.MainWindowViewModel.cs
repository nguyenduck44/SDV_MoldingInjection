using EQX.Core.Common;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Defines.Devices;
using SDV_DotDispenser.Process;
using System.Windows;
using System.Windows.Threading;

namespace SDV_DotDispenser.MVVM.ViewModels
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
                                   ViewModelFactory viewModelProvider,
                                   MachineStatus machineStatus,
                                   Devices devices)
        {
            _navigationStore = navigationStore;
            _viewModelProvider = viewModelProvider;
            _machineStatus = machineStatus;
            Devices = devices;
            HeaderVM = _viewModelProvider.Create<HeaderViewModel>();
            FooterVM = _viewModelProvider.Create<FooterViewModel>();

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
        private readonly ViewModelFactory _viewModelProvider;
        private readonly MachineStatus _machineStatus;
        #endregion
    }
}
