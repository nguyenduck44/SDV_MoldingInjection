using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Device.Balance;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Process
{
    // Gate Close position : Close to Exhaust (Nozzle)
    // Gate Open position : Open to Syringe
    public class SPDHeadProcess : MIProcess
    {
        #region Inputs
        private IDInput In_GateOpen => Name switch
        {
            "SPDHead1" => _devices.Inputs.H1_GateOpen,
            "SPDHead2" => _devices.Inputs.H2_GateOpen,
            "SPDHead3" => _devices.Inputs.H3_GateOpen,
            "SPDHead4" => _devices.Inputs.H4_GateOpen,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IDInput In_GateClose => Name switch
        {
            "SPDHead1" => _devices.Inputs.H1_GateClose,
            "SPDHead2" => _devices.Inputs.H2_GateClose,
            "SPDHead3" => _devices.Inputs.H3_GateClose,
            "SPDHead4" => _devices.Inputs.H4_GateClose,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IDInput In_AssembleCheck => Name switch
        {
            "SPDHead1" => _devices.Inputs.H1_AssembleCheck,
            "SPDHead2" => _devices.Inputs.H2_AssembleCheck,
            "SPDHead3" => _devices.Inputs.H3_AssembleCheck,
            "SPDHead4" => _devices.Inputs.H4_AssembleCheck,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IDInput In_SyringeCheck => Name switch
        {
            "SPDHead1" => _devices.Inputs.H1_SyringeCheck,
            "SPDHead2" => _devices.Inputs.H2_SyringeCheck,
            "SPDHead3" => _devices.Inputs.H3_SyringeCheck,
            "SPDHead4" => _devices.Inputs.H4_SyringeCheck,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IDInput In_SyringeAir => Name switch
        {
            "SPDHead1" => _devices.Inputs.H1_SyringeAir,
            "SPDHead2" => _devices.Inputs.H2_SyringeAir,
            "SPDHead3" => _devices.Inputs.H3_SyringeAir,
            "SPDHead4" => _devices.Inputs.H4_SyringeAir,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        #endregion

        #region Outputs
        #endregion

        #region Process IOs
        private IDInputDevice<ESPDHeadProcInput> procInputs => Name switch
        {
            "SPDHead1" => _processIO.SPDHead1_ProcInput,
            "SPDHead2" => _processIO.SPDHead2_ProcInput,
            "SPDHead3" => _processIO.SPDHead3_ProcInput,
            "SPDHead4" => _processIO.SPDHead4_ProcInput,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private IDOutputDevice<ESPDHeadProcOutput> procOutputs => Name switch
        {
            "SPDHead1" => _processIO.SPDHead1_ProcOutput,
            "SPDHead2" => _processIO.SPDHead2_ProcOutput,
            "SPDHead3" => _processIO.SPDHead3_ProcOutput,
            "SPDHead4" => _processIO.SPDHead4_ProcOutput,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        #endregion

        #region Motions
        private IMotion PAxis => Name switch
        {
            "SPDHead1" => _devices.Motions.P1Axis,
            "SPDHead2" => _devices.Motions.P2Axis,
            "SPDHead3" => _devices.Motions.P3Axis,
            "SPDHead4" => _devices.Motions.P4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };

        private IMotion GAxis => Name switch
        {
            "SPDHead1" => _devices.Motions.G1Axis,
            "SPDHead2" => _devices.Motions.G2Axis,
            "SPDHead3" => _devices.Motions.G3Axis,
            "SPDHead4" => _devices.Motions.G4Axis,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        #endregion

        #region Cylinders
        private ICylinder PistonCyl => head switch
        {
            ESPDHead.SPDHead1 => _devices.Cylinders.PistonCyl_H1,
            ESPDHead.SPDHead2 => _devices.Cylinders.PistonCyl_H2,
            ESPDHead.SPDHead3 => _devices.Cylinders.PistonCyl_H3,
            ESPDHead.SPDHead4 => _devices.Cylinders.PistonCyl_H4,
            _ => throw new Exception($"Invalid head: {head}")
        };
        #endregion

        #region Balance
        private MettlerToledoWKC204C Balance => head switch
        {
            ESPDHead.SPDHead1 or ESPDHead.SPDHead2 => _balanceLeft,
            ESPDHead.SPDHead3 or ESPDHead.SPDHead4 => _balanceRight,
            _ => throw new Exception($"Invalid head: {head}")
        };
        #endregion

        public SPDHeadProcess(Devices devices,
            RecipeSelector recipeSelector,
            ProcessIO processIO,
            [FromKeyedServices("BalanceLeft")] MettlerToledoWKC204C balanceLeft,
            [FromKeyedServices("BalanceRight")] MettlerToledoWKC204C balanceRight,
            MachineStatus machineStatus)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            _processIO = processIO;
            _balanceLeft = balanceLeft;
            _balanceRight = balanceRight;
            _machineStatus = machineStatus;
        }

        #region Process Methods
        public override bool ProcessToRun()
        {
            switch ((ESPDHeadProcToRunStep)Step.ToRunStep)
            {
                case ESPDHeadProcToRunStep.Start:
                    Log.Debug("ToRun start");
                    Step.ToRunStep++;
                    break;
                case ESPDHeadProcToRunStep.GAxis_ClosePosition_Move:
                    if (IsGateOnClosePos)
                    {
                        Step.ToRunStep = (int)ESPDHeadProcToRunStep.PistonCyl_Up;
                        break;
                    }

                    Log.Debug($"{GAxis.Name} move to GateClosePos [{_currentSPDHeadRecipe.GateClosePos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateClosePos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => IsGateOnClosePos);
                    Step.ToRunStep++;
                    break;
                case ESPDHeadProcToRunStep.GAxis_ClosePosition_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveClosePos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moved to GateClosePos [{_currentSPDHeadRecipe.GateClosePos}°] done");
                    Step.ToRunStep++;
                    break;
                case ESPDHeadProcToRunStep.PistonCyl_Up:
                    if (PistonCyl.IsBackward)
                    {
                        Step.ToRunStep = (int)ESPDHeadProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"{PistonCyl} moving up");
                    PistonCyl.Backward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => PistonCyl.IsBackward);
                    Step.ToRunStep++;
                    break;
                case ESPDHeadProcToRunStep.PistonCyl_UpWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.H1_PistonCyl_UpFail);
                        break;
                    }

                    Log.Debug($"Move {PistonCyl} up done");
                    Step.ToRunStep++;
                    break;
                case ESPDHeadProcToRunStep.End:
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
            switch ((ESPDHeadProcOriginStep)Step.OriginStep)
            {
                case ESPDHeadProcOriginStep.Start:
                    Log.Debug("Origin start");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.GAxis_Origin:
                    Log.Debug($"Searching origin {GAxis.Name}");
                    GAxis.SearchOrigin();

                    Wait(_currentRecipe.CommonRecipe.MotionOriginTimeout,
                        () => GAxis.Status.IsHomeDone);

                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.GAxis_OriginWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.G1Axis_Origin_Timeout);
                        break;
                    }

                    Wait(5000);

                    Log.Debug($"{GAxis.Name} origin search done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.GAxis_ClosePosition_Move:
                    Log.Debug($"{GAxis.Name} move to GateClosePos [{_currentSPDHeadRecipe.GateClosePos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateClosePos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => IsGateOnClosePos);
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.GAxis_ClosePosition_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveClosePos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moved to GateClosePos [{_currentSPDHeadRecipe.GateClosePos}°] done");
                    Log.Debug("Wait for XY axis move to dummy pos");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.WaitXYAxisMoveDummyPos:
                    if (procInputs[ESPDHeadProcInput.XYAxisMoveDummyPosFinish].Value == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("Dummy Pos move done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.PistonCyl_Up:
                    if (PistonCyl.IsBackward)
                    {
                        Step.OriginStep = (int)ESPDHeadProcOriginStep.PAxis_Origin;
                        break;
                    }

                    Log.Debug($"{PistonCyl} moving up");
                    PistonCyl.Backward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => PistonCyl.IsBackward);
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.PistonCyl_UpWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.H1_PistonCyl_UpFail);
                        break;
                    }

                    Log.Debug($"Move {PistonCyl} up done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.PAxis_Origin:
                    Log.Debug($"Searching origin {PAxis.Name}");
                    PAxis.SearchOrigin();

                    Wait(_currentRecipe.CommonRecipe.MotionOriginTimeout,
                        () => PAxis.Status.IsHomeDone);

                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.PAxis_OriginWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.P1Axis_Origin_Timeout);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} origin search done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.SetFlag_SPDHeadOriginDone:
                    Log.Debug($"Set flag {head} OriginDone");
                    procOutputs[ESPDHeadProcOutput.OriginDone].Value = true;
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.ClearFlag_SPDHeadOriginDone:
                    if (procInputs[ESPDHeadProcInput.XYAxisMoveDummyPosFinish].Value)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug($"Clear flag {head} OriginDone");
                    procOutputs[ESPDHeadProcOutput.OriginDone].Value = false;
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.End:
                    Log.Debug("Origin end");
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
                    if (_currentSPDHeadRecipe.HeadSkip) Sequence = ESequence.Stop;
                    Sequence_AutoRun();
                    break;
                case ESequence.ResinInject:
                    if (_currentSPDHeadRecipe.HeadSkip) Sequence = ESequence.Stop;
                    Sequence_SPDHeadCommon(ESequence.ResinInject);
                    break;
                case ESequence.DummyShot:
                    if (_currentSPDHeadRecipe.HeadSkip) Sequence = ESequence.Stop;
                    Sequence_SPDHeadCommon(ESequence.DummyShot);
                    break;
                case ESequence.DotWeighting:
                    if (_currentSPDHeadRecipe.HeadSkip) Sequence = ESequence.Stop;
                    Sequence_DotWeighting();
                    break;
                case ESequence.BubbleRemove:
                    if (_currentSPDHeadRecipe.HeadSkip) Sequence = ESequence.Stop;
                    Sequence_SPDHeadCommon(ESequence.BubbleRemove);
                    break;
                case ESequence.HeadAssemble:
                    Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    break;
                case ESequence.HeadDisassemble:
                    Sequence_HeadAssembleDisAssemble(isAssemble: false);
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

        private void Sequence_AutoRun()
        {
            Sequence = ESequence.ResinInject;
        }

        private void Sequence_SPDHeadCommon(ESequence sequence)
        {
            switch ((ESPDHeadProcCommonStep)Step.RunStep)
            {
                case ESPDHeadProcCommonStep.Start:
                    Log.Debug($"{sequence} start");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.Gate_Close:
                    if (IsGateOnClosePos)
                    {
                        Step.RunStep = (int)ESPDHeadProcCommonStep.PAxis_ChargePos_Move;
                        break;
                    }
                    Log.Debug($"{GAxis.Name} moving to ClosePos [{_currentSPDHeadRecipe.GateClosePos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateClosePos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => IsGateOnClosePos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.Gate_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveClosePos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to ClosePos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.Charge_PosVel_Calculte:
                    switch (sequence)
                    {
                        case ESequence.ResinInject:
                            _pAxisCharge_Pos = _currentSPDHeadRecipe.PAxisChargePos;
                            _pAxisCharge_Vel = PAxis.Parameter.Velocity;
                            break;
                        case ESequence.DummyShot:
                            break;
                        case ESequence.DotWeighting:
                            break;
                        case ESequence.BubbleRemove:
                            break;
                        default: throw new NotImplementedException();
                    }
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.PAxis_ChargePos_Move:
                    if (PAxis.IsOnPosition(_currentSPDHeadRecipe.PAxisChargePos))
                    {
                        Step.RunStep = (int)ESPDHeadProcCommonStep.WorkRequest_Wait;
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to ChargePos [{_pAxisCharge_Pos}mm]");
                    PAxis.MoveAbs(_pAxisCharge_Pos, _pAxisCharge_Vel);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => PAxis.IsOnPosition(_pAxisCharge_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.PAxis_ChargePos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1PAxis_MoveChargePos_Timeout);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to ChargePos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.WorkRequest_Wait:
                    if (procInputs[ESPDHeadProcInput.WorkRequest].Value == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug($"Input detect {ESPDHeadProcInput.WorkRequest}");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.Gate_Open:
                    Log.Debug($"{GAxis.Name} moving to OpenPos [{_currentSPDHeadRecipe.GateOpenPos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateOpenPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => IsGateOnOpenPos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.Gate_OpenWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveOpenPos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to OpenPos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.Inject_PosVel_Calculte:
                    switch (sequence)
                    {
                        case ESequence.ResinInject:
                            _pAxisInject_Pos = _currentSPDHeadRecipe.PAxisInjectPos;
                            _pAxisInject_Vel = PAxis.Parameter.Velocity;
                            break;
                        case ESequence.DummyShot:
                            break;
                        case ESequence.BubbleRemove:
                            break;
                        default: throw new NotImplementedException();
                    }
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.PAxis_InjectPos_Move:
                    Log.Debug($"{PAxis.Name} moving to InjectPos [{_pAxisInject_Pos}mm]");
                    PAxis.MoveAbs(_pAxisInject_Pos, _pAxisInject_Vel);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => PAxis.IsOnPosition(_pAxisInject_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.PAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1PAxis_MoveInjectPos_Timeout);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to InjectPos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.WorkDone_Send:
                    Log.Debug($"Set output {ESPDHeadProcOutput.InjectFinish}");
                    procOutputs[ESPDHeadProcOutput.InjectFinish].Value = true;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.WorkDone_Clear:
                    if (procInputs[ESPDHeadProcInput.WorkRequest].Value == true)
                    {
                        Wait(10);
                        break;
                    }

                    procOutputs[ESPDHeadProcOutput.InjectFinish].Value = false;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    // TODO: Implement this later
                    Log.Debug($"{sequence} end, starting new cycle");
                    Step.RunStep = (int)ESPDHeadProcCommonStep.Gate_Close;
                    break;
            }
        }

        private void Sequence_DotWeighting()
        {
            switch ((ESPDHeadProcDotWeightingStep)Step.RunStep)
            {
                case ESPDHeadProcDotWeightingStep.Start:
                    Log.Debug($"{ESequence.DotWeighting} start");
                    _removeResinCount = 0;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Charge_PosVel_Calculte:
                    _pAxisCharge_Pos = _currentRecipe.CommonRecipe.ResinWeight / (Math.Pow(2.5, 2) * Math.PI);
                    _pAxisCharge_AfterCal_Pos[(int)head - 1] = _pAxisCharge_Pos;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Gate_Close:
                    if (IsGateOnClosePos)
                    {
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.PAxis_ChargePos_Move;
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to ClosePos [{_currentSPDHeadRecipe.GateClosePos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateClosePos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => IsGateOnClosePos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Gate_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveClosePos_Timeout);
                        break;
                    }
                    Log.Debug($"{GAxis.Name} moving to ClosePos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.PAxis_ChargePos_Move:
                    Log.Debug($"{PAxis.Name} moving to ChargePos [{_pAxisCharge_AfterCal_Pos[(int)head - 1]}mm]");
                    PAxis.MoveAbs(_pAxisCharge_AfterCal_Pos[(int)head - 1]);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => PAxis.IsOnPosition(_pAxisCharge_AfterCal_Pos[(int)head - 1]));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.PAxis_ChargePos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1PAxis_MoveChargePos_Timeout);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to ChargePos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Gate_Open:
                    Log.Debug($"{GAxis.Name} moving to OpenPos [{_currentSPDHeadRecipe.GateOpenPos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateOpenPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => IsGateOnOpenPos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Gate_OpenWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveOpenPos_Timeout);
                        break;
                    }
                    Log.Debug($"{GAxis.Name} moving to OpenPos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Wait_DotWeightingPos_Move:
                    if (procInputs[ESPDHeadProcInput.DotWeightingRequest].Value == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("DotWeighting Position Move Finish");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Balance_Zero:
                    Log.Debug("Balance Zeroing start");

                    Balance.Tare();

                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Balance_Zero_Check:
                    if (Balance.WeightData == null)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_RequestWeight_Fail);
                        break;
                    }

                    //TODO: Tolerance
                    if (Balance.WeightData.Weight != 0)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_Zero_Fail);
                        break;
                    }

                    Balance.RequestStableWeight();

                    Log.Debug("Balance Zero done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.PAxis_InjectPos_Move:
                    Log.Debug($"{PAxis.Name} moving inject position [{_pAxisBase_Pos}mm]");
                    PAxis.MoveAbs(_pAxisBase_Pos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => PAxis.IsOnPosition(_pAxisBase_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.PAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1PAxis_MoveInjectPos_Timeout);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to InjectPos done {_removeResinCount} step");
                    _removeResinCount++;

                    if(_removeResinCount < 2)
                    {
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.Gate_Close;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Calibrate_Weight:
                    Log.Debug("Calibrate_Weight (DotWeighting)");

                    if (Balance.WeightData == null)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_RequestWeight_Fail);
                        break;
                    }

                    if (Balance.WeightData.Weight <= 0)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_Fail);
                        break;
                    }

                    _dotWeightingWeight = Balance.WeightData.Weight * 1000; // g -> mg


                    if (_dotWeightingWeight <= _currentRecipe.CommonRecipe.ResinWeight + _currentRecipe.CommonRecipe.ResinWeightSpec &&
                        _dotWeightingWeight >= _currentRecipe.CommonRecipe.ResinWeight - _currentRecipe.CommonRecipe.ResinWeightSpec)
                    {
                        Log.Debug("DotWeighting Pass");
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.SetFlag_DotWeightingDone;
                        break;
                    }

                    _pAxisCharge_AfterCal_Pos[(int)head - 1] = (_pAxisCharge_Pos * _currentRecipe.CommonRecipe.ResinWeight) / _dotWeightingWeight;

                    Step.RunStep = (int)ESPDHeadProcDotWeightingStep.PAxis_ChargePos_Move;
                    break;
                case ESPDHeadProcDotWeightingStep.SetFlag_DotWeightingDone:
                    Log.Debug($"Set output {ESPDHeadProcOutput.DotWeightingDone}");
                    procOutputs[ESPDHeadProcOutput.DotWeightingDone].Value = true;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.ClearFlag_DotWeightingDone:
                    if (procInputs[ESPDHeadProcInput.DotWeightingRequest].Value == true)
                    {
                        Wait(20);
                        break;
                    }

                    procOutputs[ESPDHeadProcOutput.DotWeightingDone].Value = false;
                    Log.Debug($"Clear output {ESPDHeadProcOutput.DotWeightingDone}");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    Log.Info($"{ESequence.DotWeighting} end, starting new cycle");
                    Step.RunStep = (int)ESPDHeadProcDotWeightingStep.Gate_Close;
                    break;
            }
        }

        private void Sequence_HeadAssembleDisAssemble(bool isAssemble)
        {
            switch ((ESPDHeadProcAssembleDisAssembleStep)Step.RunStep)
            {
                case ESPDHeadProcAssembleDisAssembleStep.Start:
                    Log.Debug(isAssemble ? "HeadAssemble start" : "HeadDisassemble start");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.Gate_Close:
                    if (IsGateOnClosePos)
                    {
                        Step.RunStep = (int)ESPDHeadProcAssembleDisAssembleStep.PAxis_AssemblePos_Move;
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to ClosePos [{_currentSPDHeadRecipe.GateClosePos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateClosePos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => IsGateOnClosePos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.Gate_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveClosePos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to ClosePos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PAxis_AssemblePos_Move:
                    if (PAxis.IsOnPosition(_pAxisAssemble_Pos))
                    {
                        Step.RunStep = isAssemble
                            ? (int)ESPDHeadProcAssembleDisAssembleStep.AssembleCheck
                            : (int)ESPDHeadProcAssembleDisAssembleStep.PistonCyl_Down;
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to AssemblePos [{_pAxisAssemble_Pos}mm]");
                    PAxis.MoveAbs(_pAxisAssemble_Pos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => PAxis.IsOnPosition(_pAxisAssemble_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PAxis_AssemblePos_Move_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1PAxis_MoveAssemblePos_Timeout);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to AssemblePos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.AssembleCheck:
                    if (!isAssemble)
                    {
                        Step.RunStep = (int)ESPDHeadProcAssembleDisAssembleStep.PistonCyl_Down;
                        break;
                    }

                    Log.Debug($"Waiting for {In_AssembleCheck} detect");
                    Wait(1000, () => In_AssembleCheck.Value);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.AssembleCheckWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.H1_Assemble_CheckFail);
                        break;
                    }

                    Log.Debug($"{In_AssembleCheck} detected, proceed PistonCyl Up");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PistonCyl_Up:
                    Log.Debug($"{PistonCyl} moving up");
                    PistonCyl.Backward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => PistonCyl.IsBackward);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PistonCyl_UpWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.H1_PistonCyl_UpFail);
                        break;
                    }

                    Log.Debug($"Move {PistonCyl} up done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PistonCyl_Down:
                    if (!isAssemble)
                    {
                        Log.Debug($"{PistonCyl} moving down");
                        PistonCyl.Forward();
                        Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => PistonCyl.IsForward);
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PistonCyl_DownWait:
                    if (!isAssemble)
                    {
                        if (WaitTimeOutOccurred)
                        {
                            RaiseHeadWarning(EWarning.H1_PistonCyl_DownFail);
                            break;
                        }

                        Log.Debug($"Move {PistonCyl} down done");
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Debug(isAssemble ? "Head Assemble End" : "Head DisAssemble End");
                    Sequence = ESequence.AutoRun;
                    break;
            }
        }
        #endregion

        #region Private Methods
        private void RaiseHeadWarning(EWarning warning)
        {
            RaiseWarning(warning + 1000 * ((int)head - 1));
        }
        private void RaiseHeadAlarm(EAlarm warning)
        {
            RaiseAlarm(warning + 1000 * ((int)head - 1));
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private readonly ProcessIO _processIO;
        private readonly MettlerToledoWKC204C _balanceLeft;
        private readonly MettlerToledoWKC204C _balanceRight;
        private readonly MachineStatus _machineStatus;

        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;

        private bool IsGateOnClosePos =>
            GAxis.IsOnPosition(_currentSPDHeadRecipe.GateClosePos) &&
            (In_GateClose.Value || In_GateClose.Value
#if SIMULATION
            || true
#endif
            );

        private bool IsGateOnOpenPos =>
            GAxis.IsOnPosition(_currentSPDHeadRecipe.GateOpenPos) &&
            (In_GateOpen.Value || In_GateOpen.Value
#if SIMULATION
            || true
#endif
            );

        private ESPDHead head => Name switch
        {
            "SPDHead1" => ESPDHead.SPDHead1,
            "SPDHead2" => ESPDHead.SPDHead2,
            "SPDHead3" => ESPDHead.SPDHead3,
            "SPDHead4" => ESPDHead.SPDHead4,
            _ => throw new Exception($"Invalid process name: {Name}")
        };

        private SPDHeadRecipe _currentSPDHeadRecipe => Name switch
        {
            "SPDHead1" => _currentRecipe.SPDHead1_Recipe,
            "SPDHead2" => _currentRecipe.SPDHead2_Recipe,
            "SPDHead3" => _currentRecipe.SPDHead3_Recipe,
            "SPDHead4" => _currentRecipe.SPDHead4_Recipe,
            _ => throw new Exception($"Invalid process name: {Name}")
        };

        private void WKC204C_WeightReceived(object? sender, WeightEventArgs e)
        {
        }

        private double _pAxisCharge_Pos;
        private double _pAxisCharge_Vel;
        private double _pAxisInject_Pos;
        private double _pAxisInject_Vel;
        private double _pAxisAssemble_Pos = 13.4375;
        private double _pAxisBase_Pos = 20.875;
        private double[] _pAxisCharge_AfterCal_Pos = new double[4];
        private double _dotWeightingWeight;
        public int _removeResinCount;
        private ESequence _prevSequence;
        WeightEventArgs? weightData;
        #endregion
    }
}
