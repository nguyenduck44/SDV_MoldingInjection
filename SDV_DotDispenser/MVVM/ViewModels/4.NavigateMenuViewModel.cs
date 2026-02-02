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

namespace SDV_DotDispenser.MVVM.ViewModels
{
    public class NavigateMenuViewModel : ViewModelBase
    {
        #region Command(s)
        public ObservableCollection<NavigationButton> LeftNavigationButtons { get; set; }
        public ObservableCollection<NavigationButton> RightNavigationButtons { get; set; }

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

        public ICommand ApplicationCloseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _viewModelProvider.Create<HeaderViewModel>().ApplicationCloseCommand.Execute(null);
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
            IUserStore userStore,
            IViewModelFactory viewModelProvider,
            INavigationButtonRepository navigationButtonRepository)
        {
            _navigationService = navigationService;
            _userStore = userStore;
            _viewModelProvider = viewModelProvider;
            _navigationButtonRepository = navigationButtonRepository;

            // TODO: Remove this

            _userStore.UserChanged += _userStore_UserChanged;
            UpdateNavigationButtons();
            UpdateCurrentUserLabel();
        }

        private void UpdateNavigationButtons()
        {
            LeftNavigationButtons = new ObservableCollection<NavigationButton>(_navigationButtonRepository.GetNavigationButtons("Left"));
            RightNavigationButtons = new ObservableCollection<NavigationButton>(_navigationButtonRepository.GetNavigationButtons("Right"));
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
        private readonly IUserStore _userStore;
        private readonly IViewModelFactory _viewModelProvider;
        private readonly INavigationButtonRepository _navigationButtonRepository;
        private string _currentUserLabel = string.Empty;
        #endregion
    }
}
