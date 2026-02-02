using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using log4net;

namespace SDV_DotDispenser.MVVM.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly UserStore _userStore;
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private ObservableCollection<string> accesses;

        public ObservableCollection<string> Accesses
        {
            get => accesses;
            set { accesses = value; OnPropertyChanged(nameof(Accesses)); }
        }

        public string AccessSelected { get; set; }

        public LoginViewModel(UserStore userStore,
            INavigationService navigationService,
            IAuthenticationService authenticationService)
        {
            _userStore = userStore;
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _userStore.UserChanged += _userStore_UserChanged;

            Accesses = new ObservableCollection<string>(Enum.GetNames(typeof(EPermission)).ToList());
            if (Accesses != null) AccessSelected = Accesses[0];

            Log = LogManager.GetLogger("LoginVM");
        }

        private void _userStore_UserChanged()
        {
            _navigationService.NavigateTo<AutoViewModel>();
        }

        public ICommand LoginCommand
        {
            get
            {
                return new RelayCommand<string>((password) =>
                {
                    if (password == null) return;
                    if (AccessSelected == EPermission.Admin.ToString())
                    {
                        if (_authenticationService.ValidatePermission(EPermission.Admin, password) == false)
                        {
                            MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_WrongPassword"]);
                            return;
                        }

                        Log.Info("Login Admin Permission");
                    }
                    else if (AccessSelected == EPermission.Operator.ToString())
                    {
                        if (_authenticationService.ValidatePermission(EPermission.Operator, password) == false)
                        {
                            MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_WrongPassword"]);
                            return;
                        }

                        Log.Info("Login Operator Permission");
                    }
                    else if (AccessSelected == EPermission.SuperUser.ToString())
                    {
                        if (_authenticationService.ValidatePermission(EPermission.SuperUser, password) == false)
                        {
                            MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_WrongPassword"]);
                            return;
                        }

                        Log.Info("Login Super User Permission");
                    }
                });
            }
        }
    }
}
