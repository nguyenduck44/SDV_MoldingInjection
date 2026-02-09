using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Common;
using EQX.Core.Sequence;
using SDV_DotDispenser.Defines;

namespace SDV_DotDispenser.Process
{
    public enum EPanelStatus
    {
        /// <summary>
        /// Unknown or not started
        /// </summary>
        None = 0,
        InPlasma,
        PlasmaDone,
        InVisionAlign,
        VisionAlignDone,
        InDispensing,
        DispensingDone,
        InCuring,
        CuringDone,
        InFinalInspection,
        FinalInspectionDone_OK,
        FinalInspectionDone_NG,
    }

    public class MachineStatus : ObservableObject
    {
        #region Events
        #endregion

        public MachineStatus()
        {
        }

        //public bool IsByPassMode => _machineRunMode == EMachineRunMode.ByPass;
        public bool IsDryRunMode => _machineRunMode == EMachineRunMode.DryRun;

        public EMachineRunMode MachineRunMode
        {
            get
            {
                return _machineRunMode;
            }
            set
            {
                if (_machineRunMode == value)
                {
                    return;
                }

                _machineRunMode = value;
                OnPropertyChanged(nameof(MachineRunMode));
                OnPropertyChanged(nameof(MachineRunModeDisplay));
                //OnPropertyChanged(nameof(IsByPassMode));
                OnPropertyChanged(nameof(IsDryRunMode));
            }
        }

        public string MachineRunModeDisplay
        {
            get
            {
                return _machineRunMode.ToString();
            }
        }

        public EProcessMode CurrentProcessMode
        {
            get => currentProcessMode;
            set
            {
                currentProcessMode = value;
                OnPropertyChanged(nameof(IsRunningProcessMode));
                OnPropertyChanged(nameof(IsStandByProcessMode));
                OnPropertyChanged(nameof(IsReadyToRunProcessMode));
                OnPropertyChanged();
            }
        }

        public bool IsReadyToRunProcessMode
        {
            get
            {
                return
                    currentProcessMode == EProcessMode.Warning ||
                    currentProcessMode == EProcessMode.Stop;
            }
        }

        public bool IsStandByProcessMode
        {
            get
            {
                return
                    currentProcessMode == EProcessMode.None ||
                    currentProcessMode == EProcessMode.Alarm ||
                    currentProcessMode == EProcessMode.Warning ||
                    currentProcessMode == EProcessMode.Stop;
            }
        }

        public bool IsRunningProcessMode => !IsStandByProcessMode;

        public EOperationCommand OPCommand
        {
            get => (EOperationCommand)_OPCommand;
            set
            {
                MultiThreadingHelpers.SafeSetValue(ref _OPCommand, value);
            }
        }

        public bool MachineReadyDone { get; set; }

        public bool OriginDone
        {
            get { return _originDone; }
            set { _originDone = value; }
        }

        public ESemiSequence SemiAutoSequence
        {
            get => (ESemiSequence)_SemiAutoSequence;
            set
            {
                MultiThreadingHelpers.SafeSetValue(ref _SemiAutoSequence, value);
            }
        }

        public bool MachineTestMode { get; set; }

        #region Privates
        private EMachineRunMode _machineRunMode;
        private EProcessMode currentProcessMode;
        private int _SemiAutoSequence;
        private int _OPCommand;
        private bool _originDone;
        #endregion
    }
}
