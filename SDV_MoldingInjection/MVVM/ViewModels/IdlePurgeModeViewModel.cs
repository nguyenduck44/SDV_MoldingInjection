using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Sequence;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using SDV_MoldingInjection.Recipe;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class IdlePurgeModeViewModel : ViewModelBase
    {
        public IdlePurgeModeViewModel(MachineStatus machineStatus,
            RecipeSelector recipeSelector,
            INavigationService navigationService,
            Processes processes,
            NavigationStore navigationStore)
        {
            _machineStatus = machineStatus;
            _recipeSelector = recipeSelector;
            _navigationService = navigationService;
            _processes = processes;
            _navigationStore = navigationStore;
            _idlePurgeOverlayTimer = new System.Timers.Timer(150);
            _idlePurgeOverlayTimer.Elapsed += IdlePurgeOverlayTimer_Elapsed;
            _idlePurgeOverlayTimer.Start();
        }

        private void IdlePurgeOverlayTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel != this) return;

            if (Application.Current == null) return;
            Application.Current.Dispatcher.BeginInvoke(new Action(RefreshIdlePurgeOverlay), DispatcherPriority.DataBind);
        }
        public int IdlePurgeCount
        {
            get => _idlePurgeCount;
            private set
            {
                if (_idlePurgeCount == value) return;
                _idlePurgeCount = value;
                OnPropertyChanged();
            }
        }

        public string IdlePurgeRemainTime
        {
            get => _idlePurgeRemainTime;
            private set
            {
                if (_idlePurgeRemainTime == value) return;
                _idlePurgeRemainTime = value;
                OnPropertyChanged();
            }
        }

        public ICommand IdlePurgeStopCommand => new RelayCommand(() =>
        {
            _machineStatus.OPCommand = EOperationCommand.Stop;
            _navigationService.NavigateTo<AutoViewModel>();
            _machineStatus.MachineIdleTick = (int)Environment.TickCount64;
        });

        private void RefreshIdlePurgeOverlay()
        {
            int cycleMs = (int)Math.Max(0, _recipeSelector.CurrentRecipe.IdlePurgeRecipe.IdlePurgeAfterStopTime * 60000);
            long elapsedMs = Math.Max(0, Environment.TickCount64 - _machineStatus.MachineIdleTick);
            int remainMs = Math.Max(0, cycleMs - (int)Math.Min(int.MaxValue, elapsedMs));
            IdlePurgeRemainTime = TimeSpan.FromMilliseconds(remainMs).ToString(@"hh\:mm\:ss");
        }


        private int _idlePurgeCount;
        private string _idlePurgeRemainTime = "00:00:00";
        private readonly System.Timers.Timer _idlePurgeOverlayTimer;
        private readonly MachineStatus _machineStatus;
        private readonly RecipeSelector _recipeSelector;
        private InjectProcess _injectProcess => _processes.All.OfType<InjectProcess>().FirstOrDefault()!;
        private readonly INavigationService _navigationService;
        private readonly Processes _processes;
        private readonly NavigationStore _navigationStore;
    }
}
