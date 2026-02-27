using EQX.Core.Common;
using EQX.Core.Recipe;
using EQX.Core.Sequence;

namespace SDV_MoldingInjection.Defines
{
    public class MachineStatus : MachineStatusBase<ESemiSequence>
    {
        public bool[] MachineCalibration { get; set; } = new bool[4];
        public MultiPointPosition MultiPointPosition { get; set; }
        public bool MachineReadyDone { get; set; }
        public bool MachineTestMode { get; set; }

        public MachineStatus()
        {
        }
        
        public bool OriginDone
        {
            get { return _originDone; }
            set { _originDone = value; }
        }

        private bool isDotWeightingTest;

        public bool IsDotWeightingTest
        {
            get { return isDotWeightingTest; }
            set 
            {
                isDotWeightingTest = value;
                OnPropertyChanged();
            }
        }

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

        public override void MoveMultiPointPositionSequence(MultiPointPosition multiPointPosition)
        {
            if (multiPointPosition == null || multiPointPosition.Points.Count <= 0) return;

            MultiPointPosition = multiPointPosition;

            OPCommand = EOperationCommand.SemiAuto;
            SemiAutoSequence = ESemiSequence.MoveMultiPoint;
        }

        #region Privates
        private EProcessMode currentProcessMode;
        
        private bool _originDone;

        private readonly bool[] _dotWeightingPassedByHead = new bool[4];
        #endregion
    }
}
