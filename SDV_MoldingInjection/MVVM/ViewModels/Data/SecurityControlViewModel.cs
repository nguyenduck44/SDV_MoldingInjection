using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using SDV_MoldingInjection.Services.Security;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class SecurityControlViewModel : ViewModelBase
    {
        public SecurityControlViewModel(ISecurityControlStore securityControlStore, IUserStore userStore)
        {
            _securityControlStore = securityControlStore;
            _userStore = userStore;

            PasswordModes = new ObservableCollection<string>(Enum.GetNames(typeof(EPasswordMode)));
            PermissionLevels = new ObservableCollection<string>(Enum.GetNames(typeof(EPermission)));

            SelectedPasswordMode = _securityControlStore.Settings.PasswordMode.ToString();
            ScreenSaverSec = _securityControlStore.Settings.ScreenSaverSec;
            SelectedPermissionLevel = PermissionLevels.FirstOrDefault() ?? nameof(EPermission.Operator);
        }

        #region Properties
        public ObservableCollection<string> PasswordModes { get; }

        public ObservableCollection<string> PermissionLevels { get; }

        public string SelectedPasswordMode
        {
            get => _selectedPasswordMode;
            set
            {
                _selectedPasswordMode = value;
                OnPropertyChanged(nameof(SelectedPasswordMode));
                OnPropertyChanged(nameof(IsOperatorPasswordEditable));
            }
        }

        public int ScreenSaverSec
        {
            get => _screenSaverSec;
            set
            {
                _screenSaverSec = value;
                OnPropertyChanged(nameof(ScreenSaverSec));
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

        public string CurrentPassword
        {
            get => _currentPassword;
            set
            {
                _currentPassword = value;
                OnPropertyChanged(nameof(CurrentPassword));
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                OnPropertyChanged(nameof(NewPassword));
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged(nameof(ConfirmPassword));
            }
        }

        public bool IsOperatorPasswordEditable => ParsePasswordMode(SelectedPasswordMode) == EPasswordMode.EQP;
        #endregion

        #region Commands
        public ICommand ApplySecuritySettingCommand => new RelayCommand(() =>
        {
            if (MessageBoxEx.ShowDialog("Do you want to change Security Settings?") == false)
            {
                SelectedPasswordMode = _securityControlStore.Settings.PasswordMode.ToString();
                return;
            }

            if (_userStore.Permission < EPermission.Admin)
            {
                MessageBoxEx.ShowDialog("Only Admin or higher can update security settings.");
                return;
            }

            if (ScreenSaverSec < 10)
            {
                MessageBoxEx.ShowDialog("Screen Saver Timer must be at least 10 seconds.");
                return;
            }

            _securityControlStore.Settings.PasswordMode = ParsePasswordMode(SelectedPasswordMode);
            _securityControlStore.Settings.ScreenSaverSec = ScreenSaverSec;
            _securityControlStore.Save();
            MessageBoxEx.ShowDialog("Security settings saved.");
        });

        public ICommand ChangePasswordCommand => new RelayCommand(() =>
        {
            if (_userStore.Permission < EPermission.Admin)
            {
                MessageBoxEx.ShowDialog("Password change is allowed only in Admin mode.");
                return;
            }

            if (Enum.TryParse<EPermission>(SelectedPermissionLevel, out var level) == false)
            {
                MessageBoxEx.ShowDialog("Invalid permission level.");
                return;
            }

            if (level == EPermission.Operator && IsOperatorPasswordEditable == false)
            {
                MessageBoxEx.ShowDialog("Operator password is managed by RMS mode.");
                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                MessageBoxEx.ShowDialog("New password is required.");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                MessageBoxEx.ShowDialog("New password and confirm password do not match.");
                return;
            }

            if (_securityControlStore.ChangePassword(level, CurrentPassword, NewPassword) == false)
            {
                MessageBoxEx.ShowDialog("Current password is incorrect.");
                return;
            }

            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
            MessageBoxEx.ShowDialog("Password changed successfully.");
        });
        #endregion

        #region Privates
        private static EPasswordMode ParsePasswordMode(string mode)
        {
            return Enum.TryParse<EPasswordMode>(mode, out var parsedMode) ? parsedMode : EPasswordMode.RMS;
        }

        private readonly ISecurityControlStore _securityControlStore;
        private readonly IUserStore _userStore;

        private string _selectedPasswordMode = nameof(EPasswordMode.RMS);
        private string _selectedPermissionLevel = nameof(EPermission.Operator);
        private int _screenSaverSec = 1000;
        private bool _enableDynamicSuperUserCode = true;
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        #endregion
    }
}
