using EQX.Core.Common;
using System.Windows;

namespace SDV_DotDispenser.MVVM.ViewModels
{
    public class FooterViewModel : ViewModelBase
    {
        public ViewModelBase NavigateVM { get; set; }

        /// <summary>
        /// Is InitDeinitView currently be displayed?
        /// </summary>
        public bool IsNavigationMenuHide
        {
            get
            {
                return _viewModelNavigationStore?.CurrentViewModel?.GetType() != typeof(InitDeinitViewModel);
            }
        }

        public FooterViewModel(NavigationStore viewModelNavigationStore,
            IViewModelFactory viewModelFactory)
        {
            _viewModelNavigationStore = viewModelNavigationStore;
            _viewModelFactory = viewModelFactory;

            _viewModelNavigationStore.CurrentViewModelChanged += _viewModelNavigationStore_CurrentViewModelChanged;

            NavigateVM = _viewModelFactory.Create<NavigateMenuViewModel>();
        }

        private void _viewModelNavigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(IsNavigationMenuHide));
        }

        private readonly NavigationStore _viewModelNavigationStore;
        private readonly IViewModelFactory _viewModelFactory;
    }
}
