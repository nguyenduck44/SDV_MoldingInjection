using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Recipe;
using EQX.InOut;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.TeachingPosition;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using SDV_MoldingInjection.MVVM.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class DryPumpMaintenanceViewModel : AppMaintenanceViewModel
    {
        #region Properties
        public double PressureSpec => _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureSpec;
        public double CurrentPressure => _devices.AnalogInputs.VacuumPressureInTorr;
        public double TimeInSecond { get; set; }
        public DryPumpMaintenanceTeachingPosition DryPumpMaintenanceTeachingPosition { get; }
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
                    _leakTestSamples.Clear();
                    _milestonePressureSpecSec = null;
                    _milestoneTestEndSec = null;
                    _leakTestSamples.Add((0, CurrentPressure));
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

                    double tSec = ElapsedSecSinceLeakStart;
                    _milestoneTestEndSec = tSec;
                    _leakTestSamples.Add((tSec, CurrentPressure));

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
                    if (_leakTestSamples.Count == 0)
                    {
                        return;
                    }

                    var resultVm = _leakTestResultViewModelFactory.Create(
                        _leakTestSamples,
                        _milestonePressureSpecSec,
                        _milestoneTestEndSec,
                        PressureSpec,
                        _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec);
                    _leakTestResultDialogService.Show(resultVm);
                });
            }
        }

        #endregion

        public DryPumpMaintenanceViewModel(NavigationStore navigationStore,
            Devices devices,
            RecipeSelector recipeSelector,
            MachineStatus machineStatus,
            DryPumpMaintenanceTeachingPosition dryPumpMaintenanceTeachingPosition,
            ILeakTestResultViewModelFactory leakTestResultViewModelFactory,
            ILeakTestResultDialogService leakTestResultDialogService)
            : base(navigationStore, machineStatus, recipeSelector, devices)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            DryPumpMaintenanceTeachingPosition = dryPumpMaintenanceTeachingPosition;
            _leakTestResultViewModelFactory = leakTestResultViewModelFactory;
            _leakTestResultDialogService = leakTestResultDialogService;
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
            return DryPumpMaintenanceTeachingPosition.PositionManager;
        }

        protected override void ExternalTimerElapsedAction()
        {
            double pressure = CurrentPressure;
            OnPropertyChanged(nameof(CurrentPressure));

            if (enableExternalTimerAction == false) return;

            double tSec = ElapsedSecSinceLeakStart;
            TimeInSecond = tSec;
            OnPropertyChanged(nameof(TimeInSecond));

            if (tSec > 0)
                _leakTestSamples.Add((tSec, pressure));

            if (pressure < PressureSpec && specReached == false)
            {
                specReached = true;
                _milestonePressureSpecSec = tSec;
                _devices.Cylinders.AngleValve.Close();
                PressureLog(pressure, "Valve Close");
                return;
            }

            if (pressure > _recipeSelector.CurrentRecipe.DryPumpRecipe.VacuumPressureHoldUnderSpec && specReached)
            {
                _milestoneTestEndSec = tSec;
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
        private readonly ILeakTestResultViewModelFactory _leakTestResultViewModelFactory;
        private readonly ILeakTestResultDialogService _leakTestResultDialogService;

        private int tickCount;
        private bool enableExternalTimerAction;
        private bool specReached;

        private readonly List<(double TimeSec, double PressureTorr)> _leakTestSamples = new();

        private double? _milestonePressureSpecSec;
        private double? _milestoneTestEndSec;

        private double ElapsedSecSinceLeakStart => 1.0 * (Environment.TickCount - tickCount) / 1000.0;
        #endregion
    }
}