using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Sequence;
using EQX.Device.Indicator;
using EQX.InOut;
using Microsoft.Extensions.DependencyInjection;
using EQX.UI.Controls;
using EQX.UI.Language;
using log4net;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class RightPanelViewModel : ViewModelBase
    {
        #region Properties
        public double Pressure => Devices.AnalogInputs.VacuumPressureInTorr;
        public double PanelTemperature => _panelIndicator.Temperature;
        public double PanelHumidity => _panelIndicator.Humidity;
        public double PumpTemperature => _pumpIndicator.Temperature;
        public double PumpHumidity => _pumpIndicator.Humidity;

        public MachineStatus MachineStatus { get; }
        public Devices Devices { get; }

        public string MachineRunModeDisplay => MachineStatus.MachineRunMode.ToString();
        #endregion

        #region Constructor
        public RightPanelViewModel(MachineStatus machineStatus,
            INavigationService navigationService,
            Devices devices,
            NavigationStore navigationStore,
            ILanguageService languageService,
            [FromKeyedServices("PanelIndicator")] NEOSHSDIndicator panelIndicator,
            [FromKeyedServices("PumpIndicator")] NEOSHSDIndicator pumpIndicator)
        {
            MachineStatus = machineStatus;
            _navigationService = navigationService;
            Devices = devices;
            _navigationStore = navigationStore;
            _languageService = languageService;
            _panelIndicator = panelIndicator;
            _pumpIndicator = pumpIndicator;
            MachineStatus.PropertyChanged += MachineStatusOnPropertyChanged;

            Log = LogManager.GetLogger("AutoVM");

            statusUpdateTimer = new System.Timers.Timer(100);
            statusUpdateTimer.Elapsed += StatusUpdateTimerHandler;
            statusUpdateTimer.Start();
        }
        #endregion

        private void StatusUpdateTimerHandler(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel is not AutoViewModel &&
                _navigationStore.CurrentViewModel is not ManualViewModel &&
                _navigationStore.CurrentViewModel is not MaintenanceViewModel<ESemiSequence, RecipeList>)
                return;

            OnPropertyChanged(nameof(Pressure));
            OnPropertyChanged(nameof(PanelTemperature));
            OnPropertyChanged(nameof(PanelHumidity));
            OnPropertyChanged(nameof(PumpTemperature));
            OnPropertyChanged(nameof(PumpHumidity));
        }

        private void MachineStatusOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MachineStatus.MachineRunMode))
            {
                OnPropertyChanged(nameof(MachineRunModeDisplay));
            }
        }

        #region Commands
        public ICommand ChamberCloseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Cylinders.ChamberOpenClose.Close();
                });
            }
        }

        public ICommand ChamberOpenCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Cylinders.ChamberOpenClose.Open();
                });
            }
        }

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
                    _navigationService.NavigateTo<MonitorIOViewModel>();
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
                });
            }
        }

        public ICommand BuzzerOffCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }
        #endregion

        #region Privates
        private readonly INavigationService _navigationService;
        private readonly NEOSHSDIndicator _panelIndicator;
        private readonly NEOSHSDIndicator _pumpIndicator;
        private readonly NavigationStore _navigationStore;
        private readonly ILanguageService _languageService;
        System.Timers.Timer statusUpdateTimer;
        #endregion
    }
}
