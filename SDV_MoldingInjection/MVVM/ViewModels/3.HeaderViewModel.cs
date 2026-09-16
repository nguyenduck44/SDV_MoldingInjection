using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Communication.CIM;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.MVVM.ViewModels;
using SDV_MoldingInjection.Recipe;
using System.Windows.Input;
using TOPENG_Device;

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
        public MachineStatus MachineStatus { get; }

        public string IsMasterAlive => RecipeSelector.CurrentRecipe.OptionRecipe.UseCIM ? "REMOTE" : "OFFLINE";

        public string CurrentView => _navigationStore?.CurrentViewModel?.GetType().Name.TrimEnd("Model".ToCharArray())!;

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
            MachineStatus machineStatus,
            NavigationStore navigationStore,
            Devices devices,
            CIMAction cimAction)
        {
            Information = information;
            _navigationService = navigationService;
            _viewModelFactory = viewModelFactory;
            RecipeSelector = recipeSelector;
            MachineStatus = machineStatus;
            _navigationStore = navigationStore;
            Devices = devices;

            System.Timers.Timer timer = new System.Timers.Timer(500);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            _navigationStore.CurrentViewModelChanged += _navigationStore_CurrentViewModelChanged;
        }

        #region Private Methods
        private void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            OnPropertyChanged(nameof(Now));

            if (CIMCommandDetail.Create(CIMCommand.AliveBit).IsCIMBitOn())
            {
                if (isAliveBitLastOn == false)
                {
                    isAliveBitLastOn = true;
                    masterAliveWatcher = DateTime.Now;
                }
            }
            else
            {
                if (isAliveBitLastOn)
                {
                    isAliveBitLastOn = false;
                }
            }

            if ((DateTime.Now - masterAliveWatcher).TotalSeconds > _cimMasterTimeout)
            {
                MachineStatus.EquipState.IsMasterAlive = false;
            }
            else
            {
                MachineStatus.EquipState.IsMasterAlive = true;
            }
            OnPropertyChanged(nameof(IsMasterAlive));
        }

        private void _navigationStore_CurrentViewModelChanged()
        {
            OnPropertyChanged(nameof(CurrentView));
        }
        #endregion

        #region Privates
        private int _cimMasterTimeout = 20;
        private DateTime masterAliveWatcher = DateTime.Now;
        private bool isCheckingMasterAlive = false;
        private bool isAliveBitLastOn = false;
        #endregion
    }
}
