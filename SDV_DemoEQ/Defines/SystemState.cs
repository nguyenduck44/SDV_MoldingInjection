using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.Common;
using EQX.Core.InOut;
using EQX.Core.Process;
using EQX.Core.Sequence;

namespace SDV_DemoEQ.Defines
{
    public class SystemState<TESequence, TESemiSequence> : ObservableObject
        where TESequence : Enum
        where TESemiSequence : Enum
    {
        public ProcessState<TESequence> ProcessState { get; set; }
        public UserActionState<TESemiSequence> UserActionState { get; set; } = new UserActionState<TESemiSequence>();

        public SystemState()
        {
        }
    }

    public class ProcessState<TESequence> : ObservableObject where TESequence : Enum
    {
        private readonly IProcess<TESequence> _rootProcesses;

        public EProcessMode CurrentProcessMode => _rootProcesses.ProcessMode;

        public ProcessState(IProcess<TESequence> rootProcess)
        {
            _rootProcesses = rootProcess;
            _rootProcesses.ProcessModeUpdated += ProcessModeUpdated;
        }

        private void ProcessModeUpdated(object? sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentProcessMode));
        }
    }

    public class UserActionState<TESemiSequence> where TESemiSequence : Enum
    {
        #region Privates
        private int _hmiCommand;
        private int _semiSequence;
        #endregion

        /// <summary>
        /// User requested command via HMI
        /// </summary>
        public EOperationCommand HMICommand
        {
            get => (EOperationCommand)_hmiCommand;
            set
            {
                MultiThreadingHelpers.SafeSetValue(ref _hmiCommand, value);
            }
        }

        public TESemiSequence SemiSequence
        {
            get => (TESemiSequence)(object)_semiSequence;
            set
            {
                MultiThreadingHelpers.SafeSetValue(ref _semiSequence, value);
            }
        }
    }

    public delegate bool InputPushedPredicate(params IDInput[] args);

    public class UserPhysicalAction
    {
        public Func<bool>? StartPushed { get; set; }

        public EOperationCommand PhysicCommand
        {
            get
            {
                if (StartPushed?.Invoke() == true)
                {
                    return EOperationCommand.Start;
                }
                
                return EOperationCommand.None;
            }
        }
    }
}
