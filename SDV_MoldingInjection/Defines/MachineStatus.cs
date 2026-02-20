using EQX.Core.Common;
using EQX.Core.Sequence;

namespace SDV_MoldingInjection.Defines
{
    public class MachineStatus : MachineStatusBase<ESemiSequence>
    {
        public MachineStatus()
        {
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

        public bool MachineReadyDone { get; set; }

        public bool OriginDone
        {
            get { return _originDone; }
            set { _originDone = value; }
        }

        public bool MachineTestMode { get; set; }

        #region Privates
        private EProcessMode currentProcessMode;
        
        private bool _originDone;
        #endregion
    }
}
