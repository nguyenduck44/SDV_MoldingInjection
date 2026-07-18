using EQX.Core.Common;
using EQX.Core.Recipe;
using EQX.Core.Sequence;
using SDV_MoldingInjection.Defines.ErrorLog;
using System.Collections.ObjectModel;

namespace SDV_MoldingInjection.Defines
{
    public class MachineStatus : MachineStatusBase<ESemiSequence>
    {
        #region Publics
        public ObservableCollection<ErrorLogEntry> CurrentAlarms { get; set; } = new ObservableCollection<ErrorLogEntry>();
        public MultiPointPosition MultiPointPosition { get; set; }

        public static short[] AlarmTotalWords = new short[400];

        public bool[] MachineCalibration { get; set; } = new bool[4];
        public bool[] MachineCalibrationSkip { get; set; } = new bool[4];
        public bool MachineReadyDone { get; set; }
        public bool MachineTestMode { get; set; }
        public bool DisableSyringeCheck { get; set; }
        public bool DisableDetectJig { get; set; }
        public bool ConfirmLoadingFinish { get; set; }
        public bool InOutHandlerStartRequest { get; set; }
        public bool IsAutoMode => MachineMode == EMachineMode.Auto;
        public bool IsTeachMode => MachineMode == EMachineMode.Teach;
        public bool CanJogWithDoorOpen => IsTeachMode && IsDoorPasswordVerified;
        public bool SetDummyShotBeforeInject { get; set; }

        public int MachineIdleTick { get; set; } = Environment.TickCount;
        public int DummyShotCount { get; set; }

        public double RatioVent {  get; set; }
        #endregion

        #region Contructors
        public MachineStatus()
        {
            
        }
        #endregion

        #region Private Methods
        #endregion

        #region Properties
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

        public bool RunWithoutInOut
        {
            get { return runWithoutInOut; }
            set
            {
                runWithoutInOut = value;
                OnPropertyChanged();
            }
        }

        public bool DryRunUseChamberPressure
        {
            get { return dryRunUseChamberPressure; }
            set
            {
                dryRunUseChamberPressure = value;
                OnPropertyChanged();
            }
        }

        public bool IsDoorPasswordVerified
        {
            get => _isDoorPasswordVerified;
            set
            {
                if (_isDoorPasswordVerified == value) return;
                _isDoorPasswordVerified = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanJogWithDoorOpen));
            }
        }

        public EMachineMode MachineMode
        {
            get { return machineMode; }
            set
            {
                machineMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAutoMode));
                OnPropertyChanged(nameof(IsTeachMode));
                OnPropertyChanged(nameof(CanJogWithDoorOpen));
            }
        }


        public int ActionCount
        {
            get { return countAction; }
            set { countAction = value; }
        }


        public override void MoveMultiPointPositionSequence(MultiPointPosition multiPointPosition)
        {
            if (multiPointPosition == null || multiPointPosition.Points.Count <= 0) return;

            MultiPointPosition = multiPointPosition;

            OPCommand = EOperationCommand.SemiAuto;
            SemiAutoSequence = ESemiSequence.MoveMultiPoint;
        }
        #endregion

        #region Privates
        private double timeInject;
        private int countAction = 1;
        private string message;
        private bool isDotWeightingTest;
        private bool _originDone;

        private bool isSkipHead1 = true;
        private bool isSkipHead2 = true;
        private bool isSkipHead3 = true;
        private bool isSkipHead4 = true;
        private bool runWithoutInOut;
        private EMachineMode machineMode;
        private bool _isDoorPasswordVerified;
        private bool dryRunUseChamberPressure;
        #endregion
    }
}
