using CommunityToolkit.Mvvm.Input;
using EQX.Core.Common;
using EQX.UI.Controls;
using SDV_MoldingInjection.Defines;
using System.Windows.Input;

namespace SDV_MoldingInjection.MVVM.ViewModels
{
    public class AutoTeachModeViewModel : ViewModelBase
    {
        #region Privates
        private readonly MachineStatus _machineStatus;
        #endregion

        #region Contructor
        public AutoTeachModeViewModel(MachineStatus machineStatus, Devices devices)
        {
            _machineStatus = machineStatus;
            Devices = devices;
        }
        #endregion

        #region Commands
        public ICommand AutoModeSelectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    if (Devices.Inputs.DoorClose == false)
                    {
                        MessageBoxEx.ShowDialog("DOOR IS OPEN", false, "WARNING");
                        return;
                    }

                    if (Devices.Inputs.AutoSW.Value == false)
                    {
                        MessageBoxEx.ShowDialog("Switch Safety Key not in Auto Mode", false, "WARNING");
                        return;
                    }
                    Devices.Outputs.EQPStop.Value = false;
                    Devices.Outputs.SWKeyLock.Value = false;
                    _machineStatus.MachineMode = EMachineMode.Auto;

                    OnPropertyChanged(nameof(IsAutoMode));
                    OnPropertyChanged(nameof(IsTeachMode));
                });
            }
        }

        public ICommand TeachModeSelectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Devices.Outputs.SWKeyLock.Value = true;
                    _machineStatus.MachineMode = EMachineMode.Teach;

                    OnPropertyChanged(nameof(IsAutoMode));
                    OnPropertyChanged(nameof(IsTeachMode));
                });
            }
        }
        #endregion

        #region Properties
        public bool IsAutoMode => _machineStatus.IsAutoMode;
        public bool IsTeachMode => _machineStatus.IsTeachMode;

        public Devices Devices { get; }
        #endregion
    }
}
