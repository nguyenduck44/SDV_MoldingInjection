using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.InOut;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Process
{
    public class DryPumpProcess : MIProcess
    {
        #region Inputs
        private IDInput In_DryPumpRun => _devices.Inputs.DryPumpRun;
        #endregion

        #region Outputs
        private IDOutput Out_DryPumpRun => _devices.Outputs.DryPumpRun;
        private IDOutput Out_ChamberPurgeOn => _devices.Outputs.ChamberPurgeOn;
        #endregion

        #region AInputs
        #endregion

        #region Process IOs
        private IDInputDevice<EDryPumpProcInput> procInputs;
        private IDOutputDevice<EDryPumpProcOutput> procOutputs;
        #endregion

        #region Motions
        #endregion

        #region Cylinders
        private ICylinder AngleValve => _devices.Cylinders.AngleValve;
        #endregion

        #region Constructors
        public DryPumpProcess(Devices devices, RecipeSelector recipeSelector,
            ProcessIO processIO, MachineStatus machineStatus)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            _machineStatus = machineStatus;

            procInputs = processIO.DryPumpProcInput;
            procOutputs = processIO.DryPumpProcOutput;
        }
        #endregion

        #region Process Methods

        public override bool ProcessToRun()
        {
            switch ((EDryPumpProcToRunStep)Step.ToRunStep)
            {
                case EDryPumpProcToRunStep.Start:
                    Log.Debug("ToRun start");
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.AngleValve_Close:
                    if (AngleValve.IsForward)
                    {
                        Step.ToRunStep = (int)EDryPumpProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"Closing {AngleValve.Name}");
                    AngleValve.Forward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => AngleValve.IsForward);
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.AngleValve_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.AngleValve_CloseFail);
                        break;
                    }

                    Log.Debug($"{AngleValve.Name} closed");
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.DryPump_Run:
                    if (In_DryPumpRun.Value)
                    {
                        Step.ToRunStep = (int)EDryPumpProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"{Out_DryPumpRun.Name} run");
                    Out_DryPumpRun.Value = true;
                    Step.ToRunStep++;
                    break;
                case EDryPumpProcToRunStep.End:
                    procOutputs.ClearOutputs();

                    Log.Debug("ToRun end");
                    Step.ToRunStep++;
                    base.ProcessToRun();
                    break;
            }
            return true;
        }

        public override bool ProcessOrigin()
        {
            switch ((EDryPumpProcOriginStep)Step.OriginStep)
            {
                case EDryPumpProcOriginStep.Start:
                    Step.OriginStep++;
                    break;
                case EDryPumpProcOriginStep.End:
                    Step.OriginStep++;
                    base.ProcessOrigin();
                    break;
            }

            return true;
        }

        public override bool ProcessRun()
        {
            switch (Sequence)
            {
                case ESequence.Stop:
                    break;
                case ESequence.Ready:
                    Sequence_Ready();
                    break;
                case ESequence.AutoRun:
                    Sequence_ResinInject();
                    break;
                case ESequence.ResinInject:
                    Sequence_ResinInject();
                    break;
            }
            return true;
        }
        #endregion

        #region Sequence Methods
        private void Sequence_Ready()
        {
            Sequence = ESequence.Stop;
        }

        private void Sequence_ResinInject()
        {
            switch ((EDryPumpProcResinInjectStep)Step.RunStep)
            {
                case EDryPumpProcResinInjectStep.Start:
                    Log.Info("ResinInject start");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Run:
                    if (In_DryPumpRun.Value)
                    {
                        Step.RunStep = (int)EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait;
                        break;
                    }

                    Log.Debug($"{Out_DryPumpRun.Name} run");
                    Out_DryPumpRun.Value = true;
                    Wait(10000, () => In_DryPumpRun.Value);
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_RunWait:
                    if (WaitTimeOutOccurred &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        RaiseWarning(EWarning.DryPump_Run_Timeout);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Vacuum_RequestWait:
                    if (procInputs[EDryPumpProcInput.Vacuum_WorkRequest].Value == false)
                    {
                        break;
                    }

                    Log.Info($"Input detect {EDryPumpProcInput.Vacuum_WorkRequest}");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_Open:
                    AngleValve.Open();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, AngleValve.IsOpen);
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_OpenWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.AngleValve_OpenFail);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.VacuumGauge_SpecIn_Wait:
                    if (_devices.AnalogInputs.VacuumPressureInTorr > _currentRecipe.DryPumpRecipe.VacuumPressureSpec_Upper &&
                        _machineStatus.IsDryRunMode == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Info($"DryPump vacuum in-spec {_devices.AnalogInputs.VacuumPressureInTorr} Torr");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_Close:
                    AngleValve.Close();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, AngleValve.IsClose);
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.AngleValve_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.AngleValve_CloseFail);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_VacuumDone_Send:
                    procOutputs[EDryPumpProcOutput.ChamberVacuumSuccess].Value = true;
                    Log.Info($"Set output {EDryPumpProcOutput.ChamberVacuumSuccess}");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_Purge_RequestWait:
                    if (procInputs[EDryPumpProcInput.Purge_WorkRequest].Value == false)
                    {
                        Wait(50);
                        break;
                    }

                    procOutputs[EDryPumpProcOutput.ChamberVacuumSuccess].Value = false;
                    Log.Info($"Input detect {EDryPumpProcInput.Purge_WorkRequest}");
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_PurgeAndWait:
                    Out_ChamberPurgeOn.Value = true;
                    Wait(3000);
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.DryPump_PurgeDone_Send:
                    Out_ChamberPurgeOn.Value = false;
                    procOutputs[EDryPumpProcOutput.ChamberPurgeSuccess].Value = true;
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.WaitToClear_ProcOutput:
                    if (procInputs[EDryPumpProcInput.Purge_WorkRequest].Value == true)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Info($"Clear output {EDryPumpProcOutput.ChamberVacuumSuccess}");
                    procOutputs[EDryPumpProcOutput.ChamberPurgeSuccess].Value = false;
                    Step.RunStep++;
                    break;
                case EDryPumpProcResinInjectStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info("ResinInject end, starting new cycle");
                    Step.RunStep = (int)EDryPumpProcResinInjectStep.DryPump_Run;
                    break;
            }
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private readonly MachineStatus _machineStatus;

        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        #endregion
    }
}
