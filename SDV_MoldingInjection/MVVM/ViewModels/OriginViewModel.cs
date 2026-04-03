using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.UI.Controls;
using log4net;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Process;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class OriginViewModel : ViewModelBase
    {
        #region Privates
        private readonly INavigationService _navigationService;
        private readonly NavigationStore _navigationStore;
        private readonly NonOverlappingTimer _statusUpdateTimer;
        #endregion

        #region Constructor
        public OriginViewModel(Processes processes, 
            MachineStatus machineStatus,
            Devices devices,
            NavigationStore navigationStore,
            INavigationService navigationService,
            MachineStatusAutoViewModel machineStatusAutoViewModel)
        {
            Processes = processes;
            MachineStatus = machineStatus;
            Devices = devices;
            _navigationStore = navigationStore;
            XYMotions = new List<IMotion>
            {
                Devices.Motions.XAxis,
                Devices.Motions.StageYAxis
            };
            Head1Motions = new List<IMotion> { Devices.Motions.Z1Axis, Devices.Motions.P1Axis, Devices.Motions.G1Axis };
            Head2Motions = new List<IMotion> { Devices.Motions.Z2Axis, Devices.Motions.P2Axis, Devices.Motions.G2Axis };
            Head3Motions = new List<IMotion> { Devices.Motions.Z3Axis, Devices.Motions.P3Axis, Devices.Motions.G3Axis };
            Head4Motions = new List<IMotion> { Devices.Motions.Z4Axis, Devices.Motions.P4Axis, Devices.Motions.G4Axis };
            _navigationService = navigationService;
            MachineStatusAuto = machineStatusAutoViewModel;
            Log = LogManager.GetLogger("OriginVM");

            _statusUpdateTimer = new NonOverlappingTimer(100);
            _statusUpdateTimer.Start();
        }
        #endregion

        #region Properties
        public Processes Processes { get; }
        public MachineStatus MachineStatus { get; }
        public MachineStatusAutoViewModel MachineStatusAuto { get; }
        public Devices Devices { get; }
        public IReadOnlyList<IMotion> XYMotions { get; }
        public IReadOnlyList<IMotion> Head1Motions { get; }
        public IReadOnlyList<IMotion> Head2Motions { get; }
        public IReadOnlyList<IMotion> Head3Motions { get; }
        public IReadOnlyList<IMotion> Head4Motions { get; }

        #endregion

        #region Command
        public ICommand SelectAllCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Processes.RootProcess.Childs!.ToList().ForEach(p => p.IsOriginOrInitSelected = true);
                });
            }
        }

        public ICommand UnSelectAllCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Processes.RootProcess.Childs!.ToList().ForEach(p => p.IsOriginOrInitSelected = false);
                });
            }
        }

        public ICommand OriginCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (MessageBoxEx.ShowDialog((string)Application.Current.Resources["str_AreYouSureYouWantToSetMachineOrigin"], (string)Application.Current.Resources["str_Confirm"]) == false)
                    {
                        return;
                    }
                    Log.Debug("Origin Button Click");
                    MachineStatus.OPCommand = EOperationCommand.Origin;
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

        public ICommand ExitCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    _navigationService.NavigateTo<AutoViewModel>();
                });
            }
        }
        #endregion
    }
}
