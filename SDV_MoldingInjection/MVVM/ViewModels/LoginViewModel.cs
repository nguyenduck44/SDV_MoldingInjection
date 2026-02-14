using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using log4net;

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
        #endregion

        #region Commands
        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand<string>((password) =>
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

                    _navigationService.NavigateTo<AutoViewModel>();
                    _log.Info($"Login {permission} Permission");
                });
            }
        }
        #endregion

        public LoginViewModel(
            INavigationService navigationService,
            IAuthenticationService authenticationService)
        {
            _navigationService = navigationService;
            _authenticationService = authenticationService;

            _log = LogManager.GetLogger("LoginVM");

            _permissions = new ObservableCollection<string>(Enum.GetNames(typeof(EPermission)).ToList());
            _selectedPermission = _permissions.FirstOrDefault()!;
        }

        #region Private Methods
        #endregion

        #region Private Fields
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILog _log;
        private ObservableCollection<string> _permissions;
        private string _selectedPermission;
        #endregion
    }
}
