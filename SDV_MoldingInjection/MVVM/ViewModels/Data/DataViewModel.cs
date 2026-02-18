using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class DataViewModel : ViewModelBase
    {
        #region Commands
        public ICommand RecipeDataNavigateCommand => new RelayCommand(() =>
        {
            _navigationService.NavigateTo<RecipeViewModel>();
        });
        #endregion

        #region Constructor(s)
        public DataViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
        }
        #endregion

        #region Privates
        private readonly INavigationService _navigationService;
        #endregion
    }
}
