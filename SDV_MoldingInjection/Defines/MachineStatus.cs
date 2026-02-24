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

        public bool[] MachineCalibration { get; set; } = new bool[4];

        /// <summary>
        /// Trạng thái pass DotWeighting của từng head (index 0..3 = Head1..Head4).
        /// Chỉ set MachineCalibration = true khi cả 4 head đều pass.
        /// </summary>
        public void ClearDotWeightingPassedFlags()
        {
            for (int i = 0; i < _dotWeightingPassedByHead.Length; i++)
                _dotWeightingPassedByHead[i] = false;
        }

        public void SetDotWeightingPassed(int headIndex)
        {
            if (headIndex >= 0 && headIndex < _dotWeightingPassedByHead.Length)
                _dotWeightingPassedByHead[headIndex] = true;
        }

        public bool AllHeadsDotWeightingPassed()
        {
            for (int i = 0; i < _dotWeightingPassedByHead.Length; i++)
                if (!_dotWeightingPassedByHead[i]) return false;
            return true;
        }

        #region Privates
        private EProcessMode currentProcessMode;
        
        private bool _originDone;

        private readonly bool[] _dotWeightingPassedByHead = new bool[4];
        #endregion
    }
}
