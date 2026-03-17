using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class HeaderViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly NavigationStore _navigationStore;

        public Information Information { get; }
        public RecipeSelector RecipeSelector { get; }
        public Devices Devices { get; }

        public string CurrentView => _navigationStore?.CurrentViewModel?.GetType().Name.TrimEnd("Model".ToCharArray());

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
                    _viewModelFactory.Create<InitDeinitViewModel>().Deinitialization();
                });
            }
        }

        public ICommand BuzzerOffCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Outputs.Buzzer1On.Value = false;
                    Devices.Outputs.Buzzer2On.Value = false;
                    Devices.Outputs.Buzzer3On.Value = false;
                    Devices.Outputs.Buzzer4On.Value = false;
                });
            }
        }

        public HeaderViewModel(Information information,
            INavigationService navigationService,
            IViewModelFactory viewModelFactory,
            RecipeSelector recipeSelector,
            Devices devices,
            NavigationStore navigationStore)
        {
            Information = information;
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
            RecipeSelector = recipeSelector;
            Devices = devices;
            _navigationStore = navigationStore;

            _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;

            System.Timers.Timer timer = new System.Timers.Timer(500);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void _navigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentView));
        }

        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            OnPropertyChanged(nameof(Now));
        }
    }
}
