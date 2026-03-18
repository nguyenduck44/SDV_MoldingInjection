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
                        EquipEventHelpers.OperatorLogin(operatorID, password);
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
            ISecurityControlStore securityControlStore)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _securityControlStore = securityControlStore;

            _log = LogManager.GetLogger("LoginVM");

            _permissions = new ObservableCollection<string>(Enum.GetNames(typeof(EPermission)).ToList());
            _selectedPermission = _permissions.FirstOrDefault()!;
        }

        #region Private Methods
        #endregion

        #region Private Fields
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISecurityControlStore _securityControlStore;
        private readonly ILog _log;
        private ObservableCollection<string> _permissions;
        private string _selectedPermission;
        private string operatorID;
        private bool isLoggedIn;
        #endregion
    }
}
