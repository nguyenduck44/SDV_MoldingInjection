using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using SDV_DotDispenser.Defines;
using SDV_DotDispenser.Recipe;

namespace SDV_DotDispenser.MVVM.ViewModels
{
    public class HeaderViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ViewModelFactory _viewModelProvider;

        public Information Information { get; }
        public RecipeSelector RecipeSelector { get; }

        public DateTime Now => DateTime.Now;

        public ICommand ApplicationCloseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog((string)System.Windows.Application.Current.Resources["str_AreYouSureYouWantToCloseApplication"]) == false)
                    {
                        return;
                    }

                    _navigationService.NavigateTo<InitDeinitViewModel>();
                    _viewModelProvider.Create<InitDeinitViewModel>().Deinitialization();
                });
            }
        }

        public HeaderViewModel(Information information,
            INavigationService navigationService,
            ViewModelFactory viewModelProvider,
            RecipeSelector recipeSelector)
        {
            Information = information;
            _navigationService = navigationService;
            _viewModelProvider = viewModelProvider;
            RecipeSelector = recipeSelector;
          
            System.Timers.Timer timer = new System.Timers.Timer(500);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            OnPropertyChanged(nameof(Now));
        }
    }
}
