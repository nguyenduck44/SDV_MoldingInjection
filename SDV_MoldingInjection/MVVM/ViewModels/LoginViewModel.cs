using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Communication.CIM.Custom;
using EQX.UI.Controls;
using log4net;
using SDV_MoldingInjection.Services.Security;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel responsible only for presentation concerns of the login view.
    /// Authentication and dialog responsibilities are delegated to injected services.
    /// </summary>
    public class LoginViewModel : ViewModelBase, IDisposable
    {
        #region Properties
        public ObservableCollection<string> PasswordModes { get; }
        public ObservableCollection<string> PermissionLevels { get; }

        public string SelectedPasswordMode
        {
            get => _selectedPasswordMode;
            set
            {
                _selectedPasswordMode = value;

                _securityControlStore.Settings.PasswordMode = ParsePasswordMode(value);
                _securityControlStore.Save();
                OnPropertyChanged(nameof(SelectedPasswordMode));
                OnPropertyChanged(nameof(IsEQPPasswordMode));
                OnPropertyChanged(nameof(IsRMSPasswordMode));
            }
        }

        public string SelectedPermissionLevel
        {
            get => _selectedPermissionLevel;
            set
            {
                _selectedPermissionLevel = value;
                OnPropertyChanged(nameof(SelectedPermissionLevel));
            }
        }

        public ObservableCollection<string> Permissions
        {
            get => _permissions;
            set { _permissions = value; OnPropertyChanged(nameof(Permissions)); }
        }

        public string SelectedPermission
        {
            get => _selectedPermission;
            set { _selectedPermission = value; OnPropertyChanged(nameof(SelectedPermission)); }
        }

        public string OperatorID
        {
            get { return operatorID; }
            set
            {
                operatorID = value;
                OnPropertyChanged(OperatorID);
            }
        }

        public bool IsLoggedIn
        {
            get => isLoggedIn;
            set
            {
                isLoggedIn = value;
                OnPropertyChanged(nameof(IsLoggedIn));
            }
        }

        public bool IsEQPPasswordMode => _securityControlStore.Settings.PasswordMode == EPasswordMode.EQP;
        public bool IsRMSPasswordMode => _securityControlStore.Settings.PasswordMode == EPasswordMode.RMS;
        #endregion

        #region Commands
        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand<string>((password) =>
                {
                    if (IsEQPPasswordMode)
                    {
                        if (!Enum.TryParse<EPermission>(SelectedPermission, out var permission))
                        {
                            _log.Warn($"Unknown access selected: {SelectedPermission}");
                            MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_WrongPassword"]);
                            return;
                        }

                        // Delegate authentication logic to the authentication service (Single Responsibility).
                        var isValid = _authenticationService.ValidatePermission(permission, password);

                        if (!isValid)
                        {
                            _log.Info($"Failed login attempt for permission {permission}");
                            MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_WrongPassword"]);
                            return;
                        }
                        _log.Info($"Login {permission} Permission");
                    }

                    else
                    {
                        if(EquipEventHelpers.OperatorLogin(operatorID, password) == false)
                        {
                            _log.Info($"Login Fail");
                            MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_WrongPassword"]);
                            return;
                        }

                        _log.Info($"Login {operatorID} Permission");
                        IsLoggedIn = true;
                    }

                    _navigationService.NavigateTo<AutoViewModel>();
                });
            }
        }

        public ICommand LogoutCommand
        {
            get
            {
                return new RelayCommand<string>((password) =>
                {
                    EquipEventHelpers.OperatorLogout(operatorID, password);
                    IsLoggedIn = false;
                    OperatorID = string.Empty;
                    _log.Info($"User {operatorID} Logout Success!");
                });
            }
        }

        #endregion

        public LoginViewModel(
            INavigationService navigationService,
            IAuthenticationService authenticationService,
            ISecurityControlStore securityControlStore,
            IUserStore userStore)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _securityControlStore = securityControlStore;
            _userStore = userStore;
            _log = LogManager.GetLogger("LoginVM");

            _permissions = new ObservableCollection<string>(Enum.GetNames(typeof(EPermission)).ToList());
            _selectedPermission = _permissions.FirstOrDefault()!;

            PasswordModes = new ObservableCollection<string>(Enum.GetNames(typeof(EPasswordMode)));
            PermissionLevels = new ObservableCollection<string>(Enum.GetNames(typeof(EPermission)));

            SelectedPasswordMode = _securityControlStore.Settings.PasswordMode.ToString();
            SelectedPermissionLevel = PermissionLevels.FirstOrDefault() ?? nameof(EPermission.Operator);
        }

        #region Private Methods
        private static EPasswordMode ParsePasswordMode(string mode)
        {
            return Enum.TryParse<EPasswordMode>(mode, out var parsedMode) ? parsedMode : EPasswordMode.RMS;
        }
        #endregion

        #region Private Fields
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISecurityControlStore _securityControlStore;
        private readonly IUserStore _userStore;
        private readonly ILog _log;
        private ObservableCollection<string> _permissions;
        private string _selectedPermission;
        private string operatorID;
        private bool isLoggedIn;
        private string _selectedPasswordMode = nameof(EPasswordMode.RMS);
        private string _selectedPermissionLevel = nameof(EPermission.Operator);
        #endregion
    }
}
