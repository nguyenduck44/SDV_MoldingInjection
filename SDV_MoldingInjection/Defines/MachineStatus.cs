using EQX.Core.Common;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using SDV_MoldingInjection.Defines.Devices;

namespace SDV_MoldingInjection.Defines
{
    public class MachineStatus : MachineStatusBase<ESemiSequence>
    {
        public bool[] MachineCalibration { get; set; } = new bool[4];
        public bool[] MachineCalibrationSkip { get; set; } = new bool[4];
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

        public override void MoveMultiPointPositionSequence(MultiPointPosition multiPointPosition)
        {
            if (multiPointPosition == null || multiPointPosition.Points.Count <= 0) return;

            MultiPointPosition = multiPointPosition;

            OPCommand = EOperationCommand.SemiAuto;
            SemiAutoSequence = ESemiSequence.MoveMultiPoint;
        }

        #region Privates
        private bool _originDone;
        #endregion
    }
}
