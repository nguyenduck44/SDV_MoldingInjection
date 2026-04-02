using EQX.Core.Common;
using EQX.Core.Communication.Modbus;
using EQX.Core.Vision.Algorithms;
using EQX.Core.Vision.Grabber;
using EQX.Device.Balance;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.CIM;
using SDV_MoldingInjection.Defines.Productions;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    /// <summary>
    /// Step list for initialization / deinitialization machine
    /// </summary>
    public enum EHandleStep
    {
        Start,

        FileSystemHandle,

        CommunicationHandle,

        MotionDeviceHandle,
        IODeviceHandle,

        RecipeHandle,

        ProcessHandle,

        ProcessHandle_Wait,

        WorkDataHandle,

        End,
        Error,

        Navigate,
    }

    public enum EHandleMode
    {
        None,
        Init,
        Deinit,
    }

    public class InitDeinitViewModel : ViewModelBase
    {
        private Timer? _logCleanupTimer;
        private int _logCleanupInProgress;
        private bool _logCleanupStarted;

        #region Properties
        private string LogFolder => _configuration["Folders:LogFolder"] ?? "";
        private string ErrorFolder => _configuration["Folders:ErrorFolder"] ?? "";
        private string PressureLogFolder => _configuration["Folders:PressureLogFolder"] ?? "";

        public string MessageText
        {
            get { return _messageText; }
            set
            {
                _messageText = value;
                // The property may be updated from another thread.
                // So that, call OnPropertyChanged() inside and UI Invoke to make sure UI update properly
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    OnPropertyChanged();
                }), DispatcherPriority.Render);
            }
        }

        public List<string> ErrorMessages { get; private set; }
        #endregion

        public InitDeinitViewModel(Devices devices,
            Processes processes,
            INavigationService navigationService,
            IAuthenticationService authenticationService,
            RecipeSelector recipeSelector,
            ProcessIO processIO,
            IConfiguration configuration,
            [FromKeyedServices("BalanceLeft")] MettlerToledoWKC204C balanceLeft,
            [FromKeyedServices("BalanceRight")] MettlerToledoWKC204C balanceRight,
            [FromKeyedServices("IndicatorModbusCommunication")] IModbusCommunication indicatorModbusCommunication,
            InterlockService interlockService,
            SyringAmountStatusList syringAmountStatusList,
            CarrierJigStatusList carrierJigStatusList,
            CIMAction cimAction,
            ProductionService productionService,
            MachineStatus machineStatus,
            InOutHandler inOutHandler)
        {
            _devices = devices;
            _machineStatus = machineStatus;
            _inOutHandler = inOutHandler;
            _processes = processes;
            _navigationService = navigationService;
            _authenticationService = authenticationService;
            _recipeSelector = recipeSelector;
            _processIO = processIO;
            _configuration = configuration;
            _balanceLeft = balanceLeft;
            _balanceRight = balanceRight;
            _indicatorModbusCommunication = indicatorModbusCommunication;
            _interlockService = interlockService;
            _syringAmountStatusList = syringAmountStatusList;
            _carrierJigStatusList = carrierJigStatusList;
            _cimAction = cimAction;
            _productionService = productionService;
            _task = new Task(() => { });
            ErrorMessages = new List<string>();

            Log = LogManager.GetLogger("InitVM");
        }

        public void Initialization()
        {
            ErrorMessages = new List<string>();

            _task = new Task(() =>
            {
                if (HandlePreCheck(EHandleMode.Init) == false) return;

                OnInitialization();
                mode = EHandleMode.None;
            });

            _task.Start();
        }

        public void Deinitialization()
        {
            _task = new Task(() =>
            {
                if (HandlePreCheck(EHandleMode.Deinit) == false) return;

                OnDeinitialization();
                mode = EHandleMode.None;
            });

            _task.Start();
        }

        private bool HandlePreCheck(EHandleMode _mode)
        {
            if (mode == _mode)
            {
                // Already handle the same mode
                return false;
            }

            while (mode != EHandleMode.None)
            {
                Thread.Sleep(10);
            }

            mode = _mode;
            isHandling = true;
            _step = EHandleStep.Start;

            return true;
        }

        private void OnInitialization()
        {
            while (isHandling)
            {
                switch (_step)
                {
                    case EHandleStep.Start:
                        MessageText = "Init Start";

                        _cimAction.Start();

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.FileSystemHandle:
                        _syringAmountStatusList.Load();
                        _productionService.Load();
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.CommunicationHandle:
                        Log.Debug("Connect Modbus Communication");
                        _balanceLeft.Connect();
                        _balanceRight.Connect();
                        _inOutHandler.Connect();
                        if(_balanceLeft.IsConnected == false || _balanceRight.IsConnected == false)
                        {
                            ErrorMessages.Add("Balance Connection Failed.");
                        }

                        if (_indicatorModbusCommunication.Connect() == false)
                        {
                            ErrorMessages.Add("Indicator Connect Fail");
                        }

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.MotionDeviceHandle:
                        Log.Debug("Connect Motion Devices");

                        MessageText = "Connect Motion Devices";

                        _devices.Motions.AjinMaster.Connect();
                        _devices.Motions.FastechPlusRMaster.Connect();

                        _devices.Motions.All.ForEach(m => m.Initialization());

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.IODeviceHandle:
                        Log.Debug("Connect IO Devices");

                        MessageText = "Connect IO Devices";
                        _isSuccess = true;

                        _isSuccess &= _devices.AnalogInputs.Connect();
#if SIMULATION
                        _isSuccess &= _devices.Inputs.Connect();
#endif

                        _interlockService.Config();

                        if (_isSuccess == false)
                        {
                            ErrorMessages.Add("IO Devices init failed.");
                        }

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.RecipeHandle:
                        SubscibeResinWeightChanged();
                        SubscribeSkipHeadChanged();
                        Log.Debug("Load Recipes");
                        MessageText = "Load Recipes";
                        if (_recipeSelector.Load() == false)
                        {
                            ErrorMessages.Add("Recipes Load Fail.");
                            Log.Debug("Recipe Load Fail");
                        }
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.ProcessHandle:
                        Log.Debug("Init processes");

                        _processes.Initialize();

                        _processIO.Initialize();
                        _processIO.Mappings();

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.ProcessHandle_Wait:
                        if (_processes.RootProcess.IsAlive && _processes.RootProcess.Childs.All(c => c.IsAlive))
                        {
                            _step++;
                            break;
                        }

                        Thread.Sleep(50);
                        break;
                    case EHandleStep.WorkDataHandle:
                        StartLogCleanupScheduler();
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.End:
                        _devices.Outputs.Lamp_Stop();
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.Error:
                        if (ErrorMessages.Count > 0)
                        {
                            Log.Error("Error: " + string.Join(", ", ErrorMessages));
                            MessageText = "Error: " + string.Join(", ", ErrorMessages);
                            Thread.Sleep(500);
                        }
                        else
                        {
                            MessageText = "Initialization completed successfully.";
                        }
                        _step++;
                        break;
                    case EHandleStep.Navigate:
                        MessageText = "Navigating...";

                        // TODO: Remove this on Production
                        _authenticationService.ValidatePermission(EPermission.SuperUser, "3141");

                        _machineStatus.MachineIdleTick = Environment.TickCount;

                        _navigationService.NavigateTo<AutoViewModel>();
                        _step++;
                        break;
                    default:
                        isHandling = false;
                        return;
                }
                Thread.Sleep(2);
            }
        }

        private void OnDeinitialization()
        {
            while (isHandling)
            {
                switch (_step)
                {
                    case EHandleStep.Start:
                        MessageText = "Deinit Start";
                        StopLogCleanupScheduler();
                        MessageText = "Stop Processes";
                        _processes.ProcessesStop();

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.FileSystemHandle:
                        _syringAmountStatusList.Save();
                        _productionService.Save();
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.CommunicationHandle:
                        MessageText = "Disconnect Balance Devices";
                        _balanceLeft.Disconnect();
                        _balanceRight.Disconnect();
                        _inOutHandler.Disconnect();
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.MotionDeviceHandle:
                        MessageText = "Disconnect Motion Devices";

                        _devices.Outputs.Lamp_Stop();

                        _devices.Motions.AjinMaster.Disconnect();

                        _devices.Motions.All.ForEach(m => m.Disconnect());
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.IODeviceHandle:
                        MessageText = "Disconnect IO Devices";
                        _devices.Inputs.Disconnect();
                        _devices.Outputs.Disconnect();
                        _devices.AnalogInputs.Disconnect();
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.RecipeHandle:
                        _recipeSelector.Save();
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.ProcessHandle:
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.ProcessHandle_Wait:
                        if (_processes.RootProcess.IsAlive == false && _processes.RootProcess.Childs.All(c => c.IsAlive == false))
                        {
                            _step++;
                            break;
                        }

                        Thread.Sleep(50);
                        break;
                    case EHandleStep.WorkDataHandle:
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.End:
                        _devices.Outputs.Lamp_Alarm(false);
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.Error:
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.Navigate:
                        Thread.Sleep(50);
                        _step++;
                        break;
                    default:
                        // Set isHandling to false to stop the loop

                        Thread.Sleep(1000);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Application.Current.Shutdown(100);
                        });
                        break;
                }
                Thread.Sleep(2);
            }
        }
        private void SubscribeSkipHeadChanged()
        {
            _carrierJigStatusList.CarrierJigStatusH1.HeadSkipChanged += (isSkip) =>
            {
                _recipeSelector.CurrentRecipe.OptionRecipe.SkipHead12 = isSkip;
            };
            _carrierJigStatusList.CarrierJigStatusH2.HeadSkipChanged += (isSkip) =>
            {
                _recipeSelector.CurrentRecipe.OptionRecipe.SkipHead12 = isSkip;
            };
            _carrierJigStatusList.CarrierJigStatusH3.HeadSkipChanged += (isSkip) =>
            {
                _recipeSelector.CurrentRecipe.OptionRecipe.SkipHead34 = isSkip;
            };
            _carrierJigStatusList.CarrierJigStatusH4.HeadSkipChanged += (isSkip) =>
            {
                _recipeSelector.CurrentRecipe.OptionRecipe.SkipHead34 = isSkip;
            };

            _recipeSelector.CurrentRecipe.OptionRecipe.Head12SkipChanged += (isSkip) =>
            {
                _carrierJigStatusList.CarrierJigStatusH1.HeadSkip = isSkip;
                _carrierJigStatusList.CarrierJigStatusH2.HeadSkip = isSkip;
            };
           
            _recipeSelector.CurrentRecipe.OptionRecipe.Head34SkipChanged += (isSkip) =>
            {
                _carrierJigStatusList.CarrierJigStatusH3.HeadSkip = isSkip;
                _carrierJigStatusList.CarrierJigStatusH4.HeadSkip = isSkip;
            };
        }

        private void SubscibeResinWeightChanged()
        {
            _recipeSelector.CurrentRecipe.SPDHead1_Recipe.ResinWeightChanged += (weight) =>
            {
                _carrierJigStatusList.CarrierJigStatusH1.ResinWeight = weight;
            };
            _recipeSelector.CurrentRecipe.SPDHead2_Recipe.ResinWeightChanged += (weight) =>
            {
                _carrierJigStatusList.CarrierJigStatusH2.ResinWeight = weight;
            };
            _recipeSelector.CurrentRecipe.SPDHead3_Recipe.ResinWeightChanged += (weight) =>
            {
                _carrierJigStatusList.CarrierJigStatusH3.ResinWeight = weight;
            };
            _recipeSelector.CurrentRecipe.SPDHead4_Recipe.ResinWeightChanged += (weight) =>
            {
                _carrierJigStatusList.CarrierJigStatusH4.ResinWeight = weight;
            };
        }

        private void StartLogCleanupScheduler()
        {
            if (_logCleanupStarted)
                return;

            _logCleanupStarted = true;

            // Run the first cleanup after startup settles a bit, then periodically.
            TimeSpan dueTime = TimeSpan.FromMinutes(1);
            TimeSpan period = TimeSpan.FromHours(0);

            _logCleanupTimer = new Timer(_ =>
            {
                TriggerLogCleanup();
            }, state: null, dueTime, period);
        }

        private void StopLogCleanupScheduler()
        {
            _logCleanupStarted = false;
            try
            {
                _logCleanupTimer?.Dispose();
            }
            catch
            {
                // ignore
            }
            _logCleanupTimer = null;
        }

        private void TriggerLogCleanup()
        {
            if (Interlocked.Exchange(ref _logCleanupInProgress, 1) == 1)
                return;

            _ = Task.Run(() =>
            {
                try
                {
                    int keepDays = _recipeSelector.CurrentRecipe.CommonRecipe.LogSaveDay;
                    CleanupOldLogs(LogFolder, keepDays);
                    CleanupOldLogs(ErrorFolder, keepDays);
                    CleanupOldLogs(PressureLogFolder, keepDays);
                    _productionService.CleanupOldProductionFiles(keepDays);
                }
                catch
                {
                    // ignore
                }
                finally
                {
                    Interlocked.Exchange(ref _logCleanupInProgress, 0);
                }
            });
        }

        private void CleanupOldLogs(string logRootPath, int keepDays = 15)
        {
            if (!Directory.Exists(logRootPath))
                return;

            var dayFolders = new List<(DateTime date, string path)>();

            foreach (var monthDir in Directory.GetDirectories(logRootPath))
            {
                foreach (var dayDir in Directory.GetDirectories(monthDir))
                {
                    var dayName = Path.GetFileName(dayDir);

                    if (DateTime.TryParseExact(
                        dayName,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date))
                    {
                        dayFolders.Add((date, dayDir));
                    }
                }
            }

            var ordered = dayFolders
                .OrderByDescending(x => x.date)
                .ToList();

            var foldersToDelete = ordered
                .Skip(keepDays)
                .Select(x => x.path)
                .ToList();

            foreach (var folder in foldersToDelete)
            {
                try
                {
                    Directory.Delete(folder, recursive: true);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Failed to delete log folder: {folder} - {ex.Message}");
                }
            }

            CleanupEmptyMonthFolders(logRootPath);
        }

        private void CleanupEmptyMonthFolders(string logRootPath)
        {
            foreach (var monthDir in Directory.GetDirectories(logRootPath))
            {
                if (!Directory.EnumerateFileSystemEntries(monthDir).Any())
                {
                    try
                    {
                        Directory.Delete(monthDir);
                    }
                    catch { }
                }
            }
        }

        #region Private fields
        private readonly INavigationService _navigationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly RecipeSelector _recipeSelector;
        private readonly ProcessIO _processIO;
        private readonly IConfiguration _configuration;
        private readonly MettlerToledoWKC204C _balanceLeft;
        private readonly MettlerToledoWKC204C _balanceRight;
        private readonly IModbusCommunication _indicatorModbusCommunication;
        private readonly InterlockService _interlockService;
        private readonly SyringAmountStatusList _syringAmountStatusList;
        private readonly CarrierJigStatusList _carrierJigStatusList;
        private readonly CIMAction _cimAction;
        private readonly ProductionService _productionService;
        private readonly ICamera _alignCamera1;
        private readonly IVisionFlowRepository _visionFlowRepository;
        private readonly Devices _devices;
        private readonly MachineStatus _machineStatus;
        private readonly InOutHandler _inOutHandler;
        private readonly Processes _processes;

        private string _messageText = "";
        private Task _task;

        EHandleMode mode = EHandleMode.None;
        bool isHandling = true;

        private EHandleStep _step;

        /// <summary>
        /// Common use for all case (in switch statement)
        /// </summary>
        private bool _isSuccess = true;
        #endregion
    }
}
