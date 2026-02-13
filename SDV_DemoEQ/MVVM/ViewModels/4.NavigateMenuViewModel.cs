using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Language;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SDV_DemoEQ.MVVM.ViewModels
{
    public class NavigateMenuViewModel : ViewModelBase
    {
        #region Command(s)
        public ObservableCollection<NavigationButton> LeftNavigationButtons => new ObservableCollection<NavigationButton>(_navigationButtonRepository.GetNavigationButtons("Left"));
        public ObservableCollection<NavigationButton> RightNavigationButtons => new ObservableCollection<NavigationButton>(_navigationButtonRepository.GetNavigationButtons("Right"));

        public IRelayCommand NavigationButtonClickCommand
        {
            get
            {
                return new RelayCommand<object>((button) =>
                {
                    if (button is not NavigationButton navButton)
                        return;

                    _navigationService.NavigateTo(navButton.ViewModelType);
                });
            }
        }

        public string CurrentUserLabel
        {
            get => _currentUserLabel;
            private set => SetProperty(ref _currentUserLabel, value);
        }
        #endregion

        public NavigateMenuViewModel(INavigationService navigationService,
            INavigationButtonRepository navigationButtonRepository,
            IUserStore userStore,
            IViewModelFactory viewModelFactory)
        {
            _navigationService = navigationService;
            _navigationButtonRepository = navigationButtonRepository;
            _userStore = userStore;
            _viewModelFactory = viewModelFactory;

            // TODO: Remove this

            _userStore.UserChanged += _userStore_UserChanged;
            UpdateNavigationButtons();
            UpdateCurrentUserLabel();
        }

        private void UpdateNavigationButtons()
        {
            OnPropertyChanged(nameof(LeftNavigationButtons));
            OnPropertyChanged(nameof(RightNavigationButtons));
        }

        private void UpdateCurrentUserLabel()
        {
            CurrentUserLabel = $"{_userStore.Permission}";
        }

        private void _userStore_UserChanged()
        {
            UpdateNavigationButtons();
            UpdateCurrentUserLabel();
        }

        public override void Dispose()
        {
            _userStore.UserChanged -= _userStore_UserChanged;
            base.Dispose();
        }

        #region Privates
        private readonly INavigationService _navigationService;
        private readonly INavigationButtonRepository _navigationButtonRepository;
        private readonly IUserStore _userStore;
        private readonly IViewModelFactory _viewModelFactory;
        private string _currentUserLabel = string.Empty;
        #endregion
    }
}
