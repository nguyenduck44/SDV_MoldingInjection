using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Recipe;
using EQX.InOut;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class DryPumpMaintenanceViewModel : AppMaintenanceViewModel
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
                    if (enableExternalTimerAction) return;

                    _devices.Cylinders.AngleValve.Open();

                    logFileName = $"D:\\MoldInjection\\Log\\PressureLog\\LeakTest\\{DateTime.Now:yyyy-MM-dd}\\{DateTime.Now:yyyy-MM-dd_HH}.txt";
                    PressureLog(CurrentPressure, "Valve Open");

                    tickCount = Environment.TickCount;
                    enableExternalTimerAction = true;
                    specReached = false;
                });
            }
        }

        public ICommand LeakTestStopCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (!enableExternalTimerAction) return;

                    _devices.Cylinders.AngleValve.Close();
                    PressureLog(CurrentPressure, "Valve Close");

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
            : base(navigationStore, machineStatus, recipeSelector)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;

            if (GroupedPositions != null && GroupedPositions.Count > 0)
            {
                SelectedGroupedPosition = GroupedPositions.FirstOrDefault()!;
            }
        }

        protected override void ActualInit()
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
                _devices.Inputs.ChamberClose,
                _devices.Inputs.ChamberOpen,
                _devices.Inputs.AngleValveClose,
                _devices.Inputs.AngleValveOpen,
                _devices.Inputs.BelowsDown,
                _devices.Inputs.BelowsUp,
            };
            Outputs = new ObservableCollection<IDOutput>
            {
                _devices.Outputs.DryPumpRun,
                _devices.Outputs.ChamberPurgeOn,
                _devices.Outputs.DryPumpAlarmReset,
                 _devices.Outputs.VacChamberOpen,
                _devices.Outputs.VacChamberClose,
                _devices.Outputs.BellowsDown,
                _devices.Outputs.BellowsUp,
            };
        }

        protected override RecipePositionManagerBase<RecipeList> UpdatePositionManager()
        {
            var positionManager = new RecipePositionManager(_recipeSelector.CurrentRecipe, _devices.Motions.All);

            var readyGroup = new MultiPointPosition
            {
                Name = "Ready Pos",
                Points = new ObservableCollection<PositionPoint>
                {
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.XAxisReadyPos, _devices.Motions.XAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.InjectRecipe.YAxisReadyPos, _devices.Motions.StageYAxis),
                    positionManager.CreatePositionPoint(2, _currentRecipe => _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos, _devices.Motions.Z1Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos, _devices.Motions.Z2Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos, _devices.Motions.Z3Axis),
                    positionManager.CreatePositionPoint(1, _currentRecipe => _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos, _devices.Motions.Z4Axis),
                }
            };

            positionManager.GroupedPositions.Add(readyGroup);

            return positionManager;
        }

        protected override void ExternalTimerElapsedAction()
        {
            double pressure = CurrentPressure;
            OnPropertyChanged(nameof(CurrentPressure));

            if (enableExternalTimerAction == false) return;

            TimeInSecond = 1.0 * (Environment.TickCount - tickCount) / 1000.0;
            OnPropertyChanged(nameof(TimeInSecond));

            if (pressure < PressureSpec && specReached == false)
            {
                specReached = true;
                _devices.Cylinders.AngleValve.Close();
                PressureLog(pressure, "Valve Close");
                return;
            }

            if (pressure > _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec && specReached)
            {
                enableExternalTimerAction = false;
                PressureLog(pressure, "Reach 1.0 Torr");
                return;
            }

            PressureLog(pressure);
        }

        private void PressureLog(double pressure, string action = "")
        {
            if (string.IsNullOrEmpty(logFileName)) return;

            string message = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss},{pressure:F3}";
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
        private bool specReached;
        #endregion
    }
}