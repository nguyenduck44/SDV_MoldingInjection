using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.InOut;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class DryPumpMaintenanceViewModel : MaintenanceViewModel<ESemiSequence>
    {
        #region Properties
        public double PressureSpec => _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureSpec;
        public double CurrentPressure => _devices.AnalogInputs.VacuumPressureInTorr;
        public double TimeInSecond { get; set; }
        #endregion

        #region Commands
        public ICommand LeakTestStartCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _devices.Cylinders.AngleValve.Open();

                    logFileName = $"D:\\MoldInjection\\Log\\PressureLog\\{DateTime.Now:yyyymmdd_hhmmss}.txt";
                    PressureLog(CurrentPressure, "Valve Open");

                    tickCount = Environment.TickCount;
                    enableExternalTimerAction = true;
                });
            }
        }

        public ICommand LeakTestStopCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _devices.Cylinders.AngleValve.Close();

                    PressureLog(CurrentPressure, "Valve Close");
                    // Disable saving log
                    logFileName = string.Empty;

                    enableExternalTimerAction = false;
                });
            }
        }

        public ICommand ShowResultCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                });
            }
        }
        #endregion

        public DryPumpMaintenanceViewModel(NavigationStore navigationStore,
            Devices devices, RecipeSelector recipeSelector, MachineStatus machineStatus)
            : base(navigationStore, machineStatus)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
        }

        public override void Init()
        {
            RelatedViewModel = new List<string>
            {
                nameof(EProcess.Inject),
            };
            Cylinders = new ObservableCollection<ICylinder>
            {
                _devices.Cylinders.ChamberOpenClose,
                _devices.Cylinders.BellowCyl,
                _devices.Cylinders.AngleValve,
            };
            Sequences = new ObservableCollection<ESemiSequence>
            {
                ESemiSequence.Ready,
                ESemiSequence.ResinInject,
            };
            Inputs = new ObservableCollection<IDInput>
            {
                _devices.Inputs.DryPumpRun,
                _devices.Inputs.DryPumpAlarm,
                _devices.Inputs.PumpCDACheck,
                _devices.Inputs.PumpVentCDACheck,
                _devices.Inputs.PumpFanRun1,
                _devices.Inputs.PumpFanRun2,
            };
            Outputs = new ObservableCollection<IDOutput>
            {
                _devices.Outputs.DryPumpRun,
                _devices.Outputs.ChamberPurgeOn,
                 _devices.Outputs.VacChamberOpen,
                _devices.Outputs.VacChamberClose,
                _devices.Outputs.DryPumpAlarmReset,
            };
        }

        protected override void ExternalTimerElapsedAction()
        {
            OnPropertyChanged(nameof(CurrentPressure));

            PressureLog(CurrentPressure);

            if (enableExternalTimerAction == false) return;

            if (CurrentPressure < PressureSpec)
                LeakTestStopCommand.Execute(null);

            TimeInSecond = 1.0 * (Environment.TickCount - tickCount) / 1000.0;

            OnPropertyChanged(nameof(TimeInSecond));
        }

        private void PressureLog(double pressure, string action = "")
        {
            if (string.IsNullOrEmpty(logFileName)) return;

            string message = $"{DateTime.Now:yyyy/mm/dd hh:mm:ss:ff},{pressure}";
            if (action != string.Empty)
                message += $",{action}";
            message += "\r\n";

            lock (_fileLock)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logFileName));
                File.AppendAllText(logFileName, message);
            }
        }

        #region Privates
        private readonly object _fileLock = new object();
        private string logFileName = "";
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;

        private int tickCount;
        private bool enableExternalTimerAction;
        #endregion
    }
}