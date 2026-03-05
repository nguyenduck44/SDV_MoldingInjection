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

        public MachineStatus MachineStatus { get; }
        public Devices Devices { get; }

        public string MachineRunModeDisplay => MachineStatus.MachineRunMode.ToString();
        #endregion

        #region Constructor
        public RightPanelViewModel(MachineStatus machineStatus,
            INavigationService navigationService,
            Devices devices,
            NavigationStore navigationStore,
            ILanguageService languageService)
        {
            MachineStatus = machineStatus;
            _navigationService = navigationService;
            Devices = devices;
            _navigationStore = navigationStore;
            _languageService = languageService;
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
        }

        private void MachineStatusOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MachineStatus.MachineRunMode))
            {
                OnPropertyChanged(nameof(MachineRunModeDisplay));
            }
        }

        #region Commands
        public ICommand MaintenanceCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ESemiSequence seq = ESemiSequence.Unloading;
                    MachineStatus.OPCommand = EOperationCommand.SemiAuto;
                    MachineStatus.SemiAutoSequence = seq;
                });
            }
        }

        public ICommand NeedleCleanCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    ESemiSequence seq = ESemiSequence.NeedleCleaning;
                    MachineStatus.OPCommand = EOperationCommand.SemiAuto;
                    MachineStatus.SemiAutoSequence = seq;
                });
            }
        }
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
        #endregion

        #region Privates
        private readonly INavigationService _navigationService;
        private readonly NavigationStore _navigationStore;
        private readonly ILanguageService _languageService;
        System.Timers.Timer statusUpdateTimer;
        #endregion
    }
}
