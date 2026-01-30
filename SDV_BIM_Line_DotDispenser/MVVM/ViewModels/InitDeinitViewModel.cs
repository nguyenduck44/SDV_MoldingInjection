using EQX.Core.Common;
using EQX.Core.Communication;
using EQX.Core.Communication.Modbus;
using EQX.Core.Robot;
using EQX.Core.Units;
using EQX.UI.Controls;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDV_BIM_Line_DotDispenser.Defines;
using SDV_BIM_Line_DotDispenser.Defines.Devices;
using SDV_BIM_Line_DotDispenser.Process;
using SDV_BIM_Line_DotDispenser.Recipe;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace SDV_BIM_Line_DotDispenser.MVVM.ViewModels
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
        #region Properties
        private string LogFolder => _configuration["Folders:LogFolder"] ?? "";

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
            RecipeSelector recipeSelector,
            VirtualIO virtualIO,
            IConfiguration configuration)
        {
            _devices = devices;
            _processes = processes;
            _navigationService = navigationService;
            _recipeSelector = recipeSelector;
            _virtualIO = virtualIO;
            _configuration = configuration;
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

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.FileSystemHandle:

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.CommunicationHandle:
                        Log.Debug("Connect Modbus Communication");

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.MotionDeviceHandle:
                        Log.Debug("Connect Motion Devices");

                        MessageText = "Connect Motion Devices";

                        _devices.Motions.AjinMaster.Connect();

                        _devices.Motions.All.ForEach(m => m.Initialization());

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.IODeviceHandle:
                        Log.Debug("Connect IO Devices");

                        MessageText = "Connect IO Devices";
                        _isSuccess = true;

                        _isSuccess &= _devices.Inputs.Connect();
                        _isSuccess &= _devices.Outputs.Connect();
                        _isSuccess &= _devices.AnalogInputs.Connect();
         
                        if (_isSuccess == false)
                        {
                            ErrorMessages.Add("IO Devices init failed.");
                        }

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.RecipeHandle:
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

                        _virtualIO.Initialize();
                        _virtualIO.Mappings();

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
                        CleanupOldLogs(LogFolder, _recipeSelector.CurrentRecipe.CommonRecipe.LogSaveDay);

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

                        MessageText = "Stop Processes";
                        _processes.ProcessesStop();

                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.FileSystemHandle:
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.CommunicationHandle:
                        Thread.Sleep(50);
                        _step++;
                        break;
                    case EHandleStep.MotionDeviceHandle:
                        MessageText = "Disconnect Motion Devices";

                        _devices.Outputs.Lamp_Stop();

                        _devices.Outputs.DoorOpen.Value = true;

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
        private readonly RecipeSelector _recipeSelector;
        private readonly VirtualIO _virtualIO;
        private readonly IConfiguration _configuration;
        private readonly Devices _devices;
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
