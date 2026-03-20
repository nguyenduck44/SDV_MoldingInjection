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
        public bool DisableSyringeCheck { get; set; }
        public bool DisableDetectJig { get; set; }
        public bool ConfirmLoadingFinish { get; set; }

        public MachineStatus(Inputs inputs)
        {
            _inputs = inputs;

            _inputs.Jig1Detect.ValueChanged += JigDetect_ValueChanged;
            _inputs.Jig2Detect.ValueChanged += JigDetect_ValueChanged;
            _inputs.Jig3Detect.ValueChanged += JigDetect_ValueChanged;
            _inputs.Jig4Detect.ValueChanged += JigDetect_ValueChanged;
        }

        private void JigDetect_ValueChanged(object? sender, EventArgs e)
        {
            EquipState.IsCellInEquip = _inputs.Jig1Detect.Value || _inputs.Jig2Detect.Value ||
                _inputs.Jig3Detect.Value || _inputs.Jig4Detect.Value;
        }

        public bool OriginDone
        {
            get { return _originDone; }
            set { _originDone = value; }
        }

        public bool IsDotWeightingTest
        {
            get { return isDotWeightingTest; }
            set
            {
                isDotWeightingTest = value;
                OnPropertyChanged();
            }
        }

        public double TimeInject
        {
            get { return timeInject; }
            set
            {
                timeInject = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                OnPropertyChanged();
            }
        }

        public bool IsSkipHead1
        {
            get { return isSkipHead1; }
            set
            {
                isSkipHead1 = value;
                OnPropertyChanged();
            }
        }

        public bool IsSkipHead2
        {
            get { return isSkipHead2; }
            set
            {
                isSkipHead2 = value;
                OnPropertyChanged();
            }
        }

        public bool IsSkipHead3
        {
            get { return isSkipHead3; }
            set
            {
                isSkipHead3 = value;
                OnPropertyChanged();
            }
        }

        public bool IsSkipHead4
        {
            get { return isSkipHead4; }
            set
            {
                isSkipHead4 = value;
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
        private double timeInject;
        private string message;
        private bool isDotWeightingTest;
        private bool _originDone;
        private readonly Inputs _inputs;

        private bool isSkipHead1;
        private bool isSkipHead2;
        private bool isSkipHead3;
        private bool isSkipHead4;
        #endregion
    }
}
