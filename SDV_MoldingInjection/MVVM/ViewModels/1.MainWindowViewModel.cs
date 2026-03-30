using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Recipe;
using SDV_MoldingInjection.Services.Security;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties
        public ViewModelBase HeaderVM { get; }
        public ViewModelBase FooterVM { get; }

        public ViewModelBase RightPanelVM { get; }
        public bool ShowRightPanelVM
        {
            get
            {
                return _navigationStore.CurrentViewModel is AutoViewModel ||
                    _navigationStore.CurrentViewModel is ManualViewModel ||
                    (_navigationStore.CurrentViewModel is MaintenanceViewModel<ESemiSequence, RecipeList> maintenanceViewModel && maintenanceViewModel.MaintenanceView == EMaintenanceView.Manual);
            }
        }

        public ViewModelBase CurrentFrameVM
        {
            get
            {
                return _navigationStore.CurrentViewModel;
            }
        }
        public Devices Devices { get; }

        public bool IsDoorSafetyOverlayVisible
        {
            get => _isDoorSafetyOverlayVisible;
            private set
            {
                if (_isDoorSafetyOverlayVisible == value) return;
                _isDoorSafetyOverlayVisible = value;
                OnPropertyChanged();
            }
        }

        public string DoorSafetyOverlayMessage
        {
            get => _doorSafetyOverlayMessage;
            private set
            {
                if (_doorSafetyOverlayMessage == value) return;
                _doorSafetyOverlayMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand ConfirmDoorSafetyCommand => new RelayCommand(() =>
        {
            if (TryValidateDoorSafetyPassword() == false)
            {
                MessageBoxEx.ShowDialog("Wrong password.", false, "WARNING");
                return;
            }

            _machineStatus.IsDoorPasswordVerified = true;
            RefreshDoorSafetyOverlay();
        });
        #endregion

        public MainWindowViewModel(NavigationStore navigationStore,
                                   IViewModelFactory viewModelFactory,
                                   MachineStatus machineStatus,
                                   Devices devices,
                                   ISecurityControlStore securityControlStore)
        {
            _navigationStore = navigationStore;
            _viewModelFactory = viewModelFactory;
            _securityControlStore = securityControlStore;
            _machineStatus = machineStatus;
            Devices = devices;
            HeaderVM = _viewModelFactory.Create<HeaderViewModel>();
            FooterVM = _viewModelFactory.Create<FooterViewModel>();
            RightPanelVM = _viewModelFactory.Create<RightPanelViewModel>();

            _lastDoorClose = Devices.Inputs.DoorClose;
            _machineStatus.IsDoorPasswordVerified = _lastDoorClose;
            RefreshDoorSafetyOverlay();

            _navigationStore.CurrentViewModelChanged += FrameNavigationStore_CurrentViewModelChanged;

            _doorSafetyTimer = new NonOverlappingTimer(150);
            _doorSafetyTimer.Elapsed += DoorSafetyTimer_Elapsed;
            _doorSafetyTimer.Start();
        }

        #region Privates methods

        private void FrameNavigationStore_CurrentViewModelChanged()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                OnPropertyChanged(nameof(CurrentFrameVM));
                OnPropertyChanged(nameof(ShowRightPanelVM));
            }), DispatcherPriority.DataBind);
        }

        private void DoorSafetyTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var isDoorClose = Devices.Inputs.DoorClose;
            // Re-authenticate only when door transitions from CLOSE -> OPEN.
            if (_lastDoorClose && isDoorClose == false)
            {
                _machineStatus.IsDoorPasswordVerified = false;
            }

            _lastDoorClose = isDoorClose;

            if (Application.Current == null) return;

            Application.Current.Dispatcher.BeginInvoke(new Action(RefreshDoorSafetyOverlay), DispatcherPriority.DataBind);
        }

        private void RefreshDoorSafetyOverlay()
        {
            var isDoorClose = Devices.Inputs.DoorClose;
            IsDoorSafetyOverlayVisible = !_machineStatus.IsDoorPasswordVerified;
            DoorSafetyOverlayMessage = isDoorClose
                ? "DON'T TOUCH\nPlease enter password to resume operation."
                : "DON'T TOUCH\nDoor is open.";
        }

        private bool TryValidateDoorSafetyPassword()
        {
            VirtualKeyboard virtualKeyboard = new VirtualKeyboard()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = Application.Current.MainWindow.Width,
                Height = Application.Current.MainWindow.Height * 0.6
            };

            var result = virtualKeyboard.ShowDialog();
            if (result != true) return false;

            var password = virtualKeyboard.InputText ?? string.Empty;
            if (string.IsNullOrWhiteSpace(password)) return false;

            // Accept Admin/SuperUser for safety override.
            return _securityControlStore.ValidatePassword(EPermission.Engineer, password) ||
                   _securityControlStore.ValidatePassword(EPermission.Admin, password) ||
                   _securityControlStore.ValidatePassword(EPermission.SuperUser, password);
        }
        #endregion

        #region Private fields
        private readonly ViewModelBase _maintenanceVM;

        private readonly NavigationStore _navigationStore;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly MachineStatus _machineStatus;
        private readonly ISecurityControlStore _securityControlStore;
        private readonly NonOverlappingTimer _doorSafetyTimer;
        private bool _lastDoorClose;
        private bool _isDoorSafetyOverlayVisible;
        private string _doorSafetyOverlayMessage = string.Empty;
        #endregion
    }
}
