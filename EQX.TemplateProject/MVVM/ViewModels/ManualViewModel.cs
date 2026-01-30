using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Sequence;
using EQX.UI.Controls;
using EQX.TemplateProject.Defines;
using EQX.TemplateProject.Defines.Devices;
using EQX.TemplateProject.MVVM.Models;
using EQX.TemplateProject.MVVM.ViewModels.Manual;
using EQX.TemplateProject.Process;
using EQX.TemplateProject.Services.Factories;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace EQX.TemplateProject.MVVM.ViewModels
{
    public partial class ManualViewModel : ViewModelBase
    {
        #region Privates
        private bool isConnecting = false;
        private readonly ViewModelNavigationStore _navigationStore;
        private readonly ManualViewModelFactory _factory;
        private System.Timers.Timer _inoutUpdateTimer;
        private ManualUnitViewModel _currentManualUnitVM;
        private string SelectedManualUnit => ManualUnits.First(u => u.IsSelected).UnitName;
        #endregion

        #region Properties
        public Devices Devices { get; }
        public MachineStatus MachineStatus { get; }
        public bool MotionAjinIsConnected => Devices.Motions.AjinMaster.IsConnected;
        public ObservableCollection<ManualVMWithSelection> ManualUnits { get; }
        public ManualUnitViewModel CurrentManualUnitViewModel => _factory.Create(SelectedManualUnit);
        public bool IsConnecting
        {
            get { return isConnecting; }
            set { isConnecting = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public IRelayCommand<string> ManualUnitSelectCommand
        {
            get
            {
                return new RelayCommand<string>((unitName) =>
                {
                    if (string.IsNullOrEmpty(unitName)) return;
                    if (ManualUnits.Any(u => u.UnitName == unitName) == false) return;

                    foreach (var unit in ManualUnits)
                    {
                        unit.IsSelected = unit.UnitName == unitName;
                    }

                    OnPropertyChanged(nameof(CurrentManualUnitViewModel));
                    _currentManualUnitVM = CurrentManualUnitViewModel;
                });
            }
        }

        public ICommand MotionAjinConnectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Motions.AjinMaster.Connect();
                    OnPropertyChanged(nameof(MotionAjinIsConnected));
                });
            }
        }

        public ICommand SemiAutoCommand
        {
            get
            {
                return new RelayCommand<object>((o) =>
                {
                    if (Devices.Motions.All.Count(motion => motion.Status.IsMotionOn != true) > 0)
                    {
                        MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_TurnOnAllMotionsFirst"], (string)Application.Current.Resources["str_Confirm"]);
                        return;
                    }

                    if (MachineStatus.OriginDone == false)
                    {
                        MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_MachineNeedToBeOriginBeforeRun"]);
                        return;
                    }

                    if (o is ESemiSequence sequence == false) return;

                    MachineStatus.OPCommand = EOperationCommand.SemiAuto;
                    MachineStatus.SemiAutoSequence = sequence;
                });
            }
        }
        #endregion

        #region Constructor
        public ManualViewModel(Devices devices,
            MachineStatus machineStatus,
            ViewModelNavigationStore navigationStore,
            ManualViewModelFactory factory)
        {
            Devices = devices;
            MachineStatus = machineStatus;

            _navigationStore = navigationStore;
            _factory = factory;

            ManualUnits = new ObservableCollection<ManualVMWithSelection>()
            {
                new ManualVMWithSelection("In Conveyor", true),
                new ManualVMWithSelection("In Work Conveyor", false),
            };

            _inoutUpdateTimer = new System.Timers.Timer(100);
            _inoutUpdateTimer.Elapsed += _inoutUpdateTimer_Elapsed;
            _inoutUpdateTimer.Start();

            _currentManualUnitVM = CurrentManualUnitViewModel;
        }

        private void _inoutUpdateTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_navigationStore.CurrentViewModel.GetType() != typeof(ManualViewModel)) return;

            if (_currentManualUnitVM == null) return;
            if (_currentManualUnitVM.Inputs == null) return;
            if (_currentManualUnitVM.Inputs.Count <= 0) return;

            foreach (var input in _currentManualUnitVM.Inputs)
            {
                input.RaiseValueUpdated();
            }
        }
        #endregion

        #region Private Methods
        #endregion
    }
}
