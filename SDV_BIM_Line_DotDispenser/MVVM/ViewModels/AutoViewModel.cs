using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Sequence;
using EQX.Device.Indicator;
using EQX.UI.Controls;
using EQX.UI.Language;
using log4net;
using SDV_BIM_Line_DotDispenser.Defines.Devices;
using SDV_BIM_Line_DotDispenser.Process;
using SDV_BIM_Line_DotDispenser.Recipe;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SDV_BIM_Line_DotDispenser.MVVM.ViewModels
{
    public class AutoViewModel : ViewModelBase
    {
        #region Constructor
        public AutoViewModel(MachineStatus machineStatus,
            INavigationService navigationService,
            Devices devices,
            RecipeSelector recipeSelector,
            UserStore userStore,
            ViewModelNavigationStore navigationStore,
            ILanguageService languageService)
        {
            MachineStatus = machineStatus;
            _navigationService = navigationService;
            Devices = devices;
            RecipeSelector = recipeSelector;
            UserStore = userStore;
            _navigationStore = navigationStore;
            _languageService = languageService;
            MachineStatus.PropertyChanged += MachineStatusOnPropertyChanged;

            Log = LogManager.GetLogger("AutoVM");

            statusUpdateTimer = new System.Timers.Timer(100);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }
        #endregion

        public void EnableTimer()
        {
            statusUpdateTimer.Enabled = true;
        }

        public void DisableTimer()
        {
            statusUpdateTimer.Enabled = false;
        }
        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel != this) return;

            Devices.Inputs.DoorSensor.RaiseValueUpdated();
            Devices.Inputs.Emergency.RaiseValueUpdated();
        }

        #region Properties
        public ObservableCollection<ILanguageDefinition> Cultures
        {
            get
            {
                ObservableCollection<ILanguageDefinition> availableLanguages = new ObservableCollection<ILanguageDefinition>();
                foreach (var item in _languageService.AvailableLanguages)
                {
                    availableLanguages.Add(item);
                }
                return availableLanguages;
            }
        }

        public MachineStatus MachineStatus { get; }
        public Devices Devices { get; }
        public RecipeSelector RecipeSelector { get; }
        public UserStore UserStore { get; }

        public string MachineRunModeDisplay => MachineStatus.MachineRunModeDisplay;
        public double Temperature => _nEOSHSDIndicator.Temperature;
        public double Humidity => _nEOSHSDIndicator.Humidity;
        #endregion

        private void MachineStatusOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MachineStatus.MachineRunMode))
            {
                OnPropertyChanged(nameof(MachineRunModeDisplay));
            }
        }

        #region Commands
        public ICommand SelectRunModeCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var selected = RunModeDialog.ShowRunModeDialog(Enum.GetValues<EMachineRunMode>());
                    if (selected.HasValue)
                    {
                        MachineStatus.MachineRunMode = selected.Value;
                    }
                });
            }
        }

        public ICommand IOMonitoringNavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<IOMonitoringViewModel>();
                });
            }
        }

        public ICommand InitializeNavigate
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_AreYouSureYouWantToSetMachineReady"], (string)Application.Current.Resources["str_Confirm"]) == false)
                    {
                        return;
                    }
                    Log.Debug("Ready Button Click");
                    MachineStatus.OPCommand = EOperationCommand.Ready;
                });
            }
        }

        public ICommand InputStopCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MachineStatus.IsInputStop == false)
                    {
                        MachineStatus.IsInputStop = true;
                        Log.Debug("ENABLE STOP INTPUT!");
                    }
                    else
                    {
                        MachineStatus.IsInputStop = false;
                        Log.Debug("DISABLE STOP INTPUT!");
                    }
                });
            }
        }

        public ICommand OutputStopCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MachineStatus.IsOutputStop == false)
                    {
                        MachineStatus.IsOutputStop = true;
                        Log.Debug("ENABLE STOP OUTPUT!");
                    }
                    else
                    {
                        MachineStatus.IsOutputStop = false;
                        Log.Debug("DISABLE STOP OUTPUT!");
                    }
                });
            }
        }

        public ICommand OriginCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<OriginViewModel>();
                });
            }
        }

        public ICommand StartCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var messageKey = "str_AreYouSureYouWantToStartMachine";

                    if (MachineStatus.IsDryRunMode)
                    {
                        messageKey = "str_AreYouSureYouWantToStartMachineDryRun";
                    }
                    //else if (MachineStatus.IsByPassMode)
                    //{
                    //    messageKey = "str_AreYouSureYouWantToStartMachineByPass";
                    //}

                    if (MessageBoxEx.ShowDialog((string)Application.Current.Resources[messageKey], (string)Application.Current.Resources["str_Confirm"]) == false)
                    {
                        return;
                    }
                    Log.Debug("Start Button Click");
                    MachineStatus.OPCommand = EOperationCommand.Start;
                });
            }
        }

        public ICommand StopCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Log.Debug("Stop Button Click");
                    MachineStatus.OPCommand = EOperationCommand.Stop;
                });
            }
        }


        public ICommand DoorOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    bool currentValue = Devices.Outputs.DoorOpen.Value;
                    Devices.Outputs.DoorOpen.Value = !currentValue;
                });
            }
        }

        public ICommand BuzzerOffCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Outputs.TowerBuzzer.Value = false;
                });
            }
        }
        #endregion

        #region Privates
        private readonly INavigationService _navigationService;
        private readonly NEOSHSDIndicator _nEOSHSDIndicator;
        private readonly ViewModelNavigationStore _navigationStore;
        private readonly ILanguageService _languageService;
        System.Timers.Timer statusUpdateTimer;
        #endregion
    }
}
