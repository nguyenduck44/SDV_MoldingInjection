using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.Device.Balance;
using EQX.InOut;
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
            SyringAmountStatusList syringeAmountStatusList,
            MachineStatus machineStatus)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            _processIO = processIO;
            _balanceLeft = balanceLeft;
            _balanceRight = balanceRight;
            _syringeAmountStatusList = syringeAmountStatusList;
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

        public override bool ProcessToOrigin()
        {
            if (ProcessStatus != EProcessStatus.ToOriginDone)
            {
                procOutputs.ClearOutputs();
            }

            return base.ProcessToOrigin();
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
                    if (procInputs[ESPDHeadProcInput.XYAxisInDummyPos].Value == false)
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
                case ESPDHeadProcOriginStep.GAxis_OpenPosition_Move:
                    Log.Debug($"{GAxis.Name} move to GateOpenPos [{_currentSPDHeadRecipe.GateOpenPos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateOpenPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => IsGateOnOpenPos);
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.GAxis_OpenPosition_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveOpenPos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moved to GateOpenPos [{_currentSPDHeadRecipe.GateOpenPos}°] done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.PAxis_BasePosition_Move:
                    Log.Debug($"{PAxis.Name} move to BasePos [{_pAxisBase_Pos}mm]");
                    PAxis.MoveAbs(_pAxisBase_Pos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => PAxis.IsOnPosition(_pAxisBase_Pos));
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.PAxis_BasePosition_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1PAxis_MoveBasePos_Timeout);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moved to BasePos [{_pAxisBase_Pos}mm] done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.SetFlag_SPDHeadOriginDone:
                    Log.Debug($"Set flag {head} OriginDone");
                    procOutputs[ESPDHeadProcOutput.OriginDone].Value = true;
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.ClearFlag_SPDHeadOriginDone:
                    if (procInputs[ESPDHeadProcInput.XYAxisInDummyPos].Value)
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
                    if (_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    Sequence_AutoRun();
                    break;
                case ESequence.ResinInject:
                    if (_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    Sequence_SPDHeadCommon(ESequence.ResinInject);
                    break;
                case ESequence.DummyShot_H1:
                    if (head == ESPDHead.SPDHead1 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot_H1);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DummyShot_H2:
                    if (head == ESPDHead.SPDHead2 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot_H2);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DummyShot_H3:
                    if (head == ESPDHead.SPDHead3 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot_H3);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DummyShot_H4:
                    if (head == ESPDHead.SPDHead4 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot_H4);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DotWeighting_H1:
                    if (head == ESPDHead.SPDHead1)
                    {
                        Sequence_DotWeighting();
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DotWeighting_H2:
                    if (head == ESPDHead.SPDHead2)
                    {
                        Sequence_DotWeighting();
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DotWeighting_H3:
                    if (head == ESPDHead.SPDHead3)
                    {
                        Sequence_DotWeighting();
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DotWeighting_H4:
                    if (head == ESPDHead.SPDHead4)
                    {
                        Sequence_DotWeighting();
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.BubbleRemove_H1:
                    if (head == ESPDHead.SPDHead1 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove_H1);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.BubbleRemove_H2:
                    if (head == ESPDHead.SPDHead2 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove_H2);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.BubbleRemove_H3:
                    if (head == ESPDHead.SPDHead3 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove_H3);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.BubbleRemove_H4:
                    if (head == ESPDHead.SPDHead4 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove_H4);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadAssemble_H1:
                    if (head == ESPDHead.SPDHead1 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadAssemble_H2:
                    if (head == ESPDHead.SPDHead2 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadAssemble_H3:
                    if (head == ESPDHead.SPDHead3 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadAssemble_H4:
                    if (head == ESPDHead.SPDHead4 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadDisassemble_H1:
                    if (head == ESPDHead.SPDHead1 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadDisassemble_H2:
                    if (head == ESPDHead.SPDHead2 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadDisassemble_H3:
                    if (head == ESPDHead.SPDHead3 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadDisassemble_H4:
                    if (head == ESPDHead.SPDHead4 && !_currentSPDHeadRecipe.HeadSkip)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                default:
                    Sequence = ESequence.Stop;
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

        // Resin Inject
        // Dummy Shot
        // Bubble Remove
        private void Sequence_SPDHeadCommon(ESequence sequence)
        {
            switch ((ESPDHeadProcCommonStep)Step.RunStep)
            {
                case ESPDHeadProcCommonStep.Start:
                    Log.Debug($"{sequence} start");
                    if (sequence == ESequence.BubbleRemove_H1 ||
                        sequence == ESequence.BubbleRemove_H2 ||
                        sequence == ESequence.BubbleRemove_H3 ||
                        sequence == ESequence.BubbleRemove_H4)
                    {
                        _bubbleRemoveCount = 1;
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.MachineCalibration_Check:
                    Log.Debug("Machine Calibration check");
                    if (_machineStatus.MachineCalibration.All(x => x) == false && _machineStatus.IsDryRunMode == false && sequence == ESequence.ResinInject)
                    {
                        RaiseWarning(EWarning.Machine_Need_Calibration);
                        break;
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.Gate_Close:
                    if (IsGateOnClosePos)
                    {
                        Step.RunStep = (int)ESPDHeadProcCommonStep.Charge_PosVel_Calculte;
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
                            _pAxisCharge_Pos = _currentSPDHeadRecipe.PAxisInjectChargePos;
                            break;
                        case ESequence.DummyShot_H1:
                        case ESequence.DummyShot_H2:
                        case ESequence.DummyShot_H3:
                        case ESequence.DummyShot_H4:
                            _pAxisCharge_Pos = _pAxisBase_Pos - 10;
                            break;
                        case ESequence.BubbleRemove_H1:
                        case ESequence.BubbleRemove_H2:
                        case ESequence.BubbleRemove_H3:
                        case ESequence.BubbleRemove_H4:
                            _pAxisCharge_Pos = 1;
                            break;
                        default: throw new NotImplementedException();
                    }
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.PAxis_ChargePos_Move:
                    Log.Debug($"{PAxis.Name} moving to ChargePos [{_pAxisCharge_Pos}mm]");
                    PAxis.MoveAbs(_pAxisCharge_Pos);
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
                case ESPDHeadProcCommonStep.Base_PosVel_Calculte:
                    _pAxisInject_Pos = _pAxisBase_Pos;
                    switch (sequence)
                    {
                        case ESequence.ResinInject:
                            _pAxisInject_Vel = Math.Abs(_currentSPDHeadRecipe.PAxisInjectChargePos - _pAxisBase_Pos) / _currentRecipe.CommonRecipe.InjectTime;
                            break;
                        case ESequence.DummyShot_H1:
                        case ESequence.DummyShot_H2:
                        case ESequence.DummyShot_H3:
                        case ESequence.DummyShot_H4:
                            _pAxisInject_Vel = PAxis.Parameter.Velocity;
                            break;
                        case ESequence.BubbleRemove_H1:
                        case ESequence.BubbleRemove_H2:
                        case ESequence.BubbleRemove_H3:
                        case ESequence.BubbleRemove_H4:
                            if (_bubbleRemoveCount != _currentRecipe.CommonRecipe.BubbleRemoveTurn)
                            {
                                _pAxisInject_Pos = _pAxisCharge_Pos + (_bubbleRemoveCount * Math.Abs(_pAxisCharge_Pos - _pAxisBase_Pos) / _currentRecipe.CommonRecipe.BubbleRemoveTurn);
                            }

                            _gAxisBubbleRemove_Pos = GAxis.Status.ActualPosition + 180;
                            _pAxisInject_Vel = PAxis.Parameter.Velocity;
                            break;
                        default: throw new NotImplementedException();
                    }

                    if (_pAxisInject_Pos > _pAxisBase_Pos)
                    {
                        RaiseHeadWarning(EWarning.H1_BubbleRemove_InjectPosOverBasePos);
                        break;
                    }

                    if (sequence != ESequence.BubbleRemove_H1 &&
                        sequence != ESequence.BubbleRemove_H2 &&
                        sequence != ESequence.BubbleRemove_H3 &&
                        sequence != ESequence.BubbleRemove_H4)
                    {
                        Step.RunStep = (int)ESPDHeadProcCommonStep.Gate_Open;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.GAxis_BubbleRemove:
                    Log.Debug("GAxis bubble remove start");
                    GAxis.MoveAbs(_gAxisBubbleRemove_Pos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                            GAxis.IsOnPosition(_gAxisBubbleRemove_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.GAxis_BubbleRemove_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.H1_GAxis_BubbleRemove_Timeout);
                    }

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

                    if (sequence == ESequence.BubbleRemove_H1 ||
                        sequence == ESequence.BubbleRemove_H2 ||
                        sequence == ESequence.BubbleRemove_H3 ||
                        sequence == ESequence.BubbleRemove_H4)
                    {
                        _bubbleRemoveCount++;
                        Log.Debug($"Bubble remove turn: {_bubbleRemoveCount}");
                        if (_bubbleRemoveCount <= _currentRecipe.CommonRecipe.BubbleRemoveTurn)
                        {
                            Step.RunStep = (int)ESPDHeadProcCommonStep.Base_PosVel_Calculte;
                            break;
                        }
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
                    _syringeAmountStatusList.ConsumeSyringeAmount(head, _currentRecipe.CommonRecipe.ResinWeight);

                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Debug($"{sequence} end");


                    if (sequence == ESequence.ResinInject)
                    {
                        Log.Info($"Set next sequence: {ESequence.DummyShot_H1}");
                        Sequence = ESequence.DummyShot_H1;
                    }
                    else if (sequence >= ESequence.DummyShot_H1 && sequence < ESequence.DummyShot_H4)
                    {
                        Sequence = (ESequence)((int)sequence + 1);
                        Log.Info($"Set next sequence: {Sequence}");
                    }
                    else
                        Sequence = ESequence.Stop;

                    break;
            }
        }

        private void Sequence_DotWeighting()
        {
            switch ((ESPDHeadProcDotWeightingStep)Step.RunStep)
            {
                case ESPDHeadProcDotWeightingStep.Start:
                    _removeResinCount = 0;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Request_XYAxis_DotWeightingPos_Move:
                    Log.Debug($"Set flag {procOutputs[ESPDHeadProcOutput.SPDHeadRequestDotWeighting]}");
                    procOutputs[ESPDHeadProcOutput.SPDHeadRequestDotWeighting].Value = true;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Charge_PosVel_Calculte:
                    Log.Debug("Calculate Charge Position");
                    if (_machineStatus.IsDotWeightingTest == false)
                    {
                        _pAxisInjectCharge_Height = V380Weight2mg(_currentRecipe.CommonRecipe.ResinWeight);
                    }

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
                    Log.Debug($"{PAxis.Name} moving to ChargePos [{_pAxisInjectCharge_Pos}mm]");
                    PAxis.MoveAbs(_pAxisInjectCharge_Pos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => PAxis.IsOnPosition(_pAxisInjectCharge_Pos));
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
                    if (procInputs[ESPDHeadProcInput.XYAxisInDotWeightingPos].Value == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("DotWeighting Position Move Finish");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Balance_Zero:
                    if (_removeResinCount == 0 && _machineStatus.IsDotWeightingTest == false)
                    {
                        _removeResinCount++;
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.PAxis_InjectPos_Move;
                        break;
                    }

                    Log.Debug("Balance Zeroing start");
                    Balance.SendZeroCommand();
                    Wait(10000, () => Balance.WeightData != null);

                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Balance_Zero_Check:
                    if (Balance.WeightData == null)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_Zero_Fail);
                        break;
                    }

                    double weight = Balance.WeightData.Weight * 1000;
                    if (weight < -3 || weight > 3)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_Zero_Fail);
                        break;
                    }

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

                    Log.Debug($"{PAxis.Name} moving to InjectPos turn {_removeResinCount} done");

                    if (_removeResinCount <= 1 && _machineStatus.IsDotWeightingTest == false)
                    {
                        // Ignore balance first time
                        _removeResinCount++;
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.Gate_Close;
                        break;
                    }

                    Thread.Sleep(1000);

                    Log.Debug("Calibrate_Weight (DotWeighting)");
                    Balance.SendRequestStableWeightCommand();
                    Wait(20000, () => Balance.WeightData != null);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.Calibrate_Weight:
                    if (WaitTimeOutOccurred || Balance.WeightData == null)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_RequestWeight_Fail);
                        break;
                    }

                    if (Balance.WeightData.IsStable == false)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_NotStable);
                        break;
                    }

                    double _calibWeight_mg = Balance.WeightData.Weight * (Balance.WeightData.Unit == "g" ? 1000 : 1); // g -> mg

                    if (_calibWeight_mg <= 0)
                    {
                        RaiseHeadWarning(EWarning.H1_Balance_ZeroWeighting_Fail);
                        break;
                    }

                    Log.Info($"WEIGHT = {_calibWeight_mg} [mg] | IsStable = {Balance.WeightData.IsStable}");

                    if(_machineStatus.IsDotWeightingTest)
                    {
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.ClearFlag_Request_XYAxis_DotWeightingPos;
                        break;
                    }

                    if (_calibWeight_mg <= _currentRecipe.CommonRecipe.ResinWeight + _currentRecipe.CommonRecipe.ResinWeightSpec &&
                        _calibWeight_mg >= _currentRecipe.CommonRecipe.ResinWeight - _currentRecipe.CommonRecipe.ResinWeightSpec)
                    {
                        Log.Debug("DotWeighting Pass");
                        _currentSPDHeadRecipe.PAxisInjectChargePos = _pAxisInjectCharge_Pos;
                        _recipeSelector.Save();
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.ClearFlag_Request_XYAxis_DotWeightingPos;
                        break;
                    }

                    _pAxisInjectCharge_Height = (_pAxisInjectCharge_Height * _currentRecipe.CommonRecipe.ResinWeight) / _calibWeight_mg;

                    Step.RunStep = (int)ESPDHeadProcDotWeightingStep.Gate_Close;
                    break;
                case ESPDHeadProcDotWeightingStep.ClearFlag_Request_XYAxis_DotWeightingPos:
                    _machineStatus.MachineCalibration[(int)head - 1] = true;
                    procOutputs[ESPDHeadProcOutput.SPDHeadRequestDotWeighting].Value = false;
                    Log.Debug($"Clear output {ESPDHeadProcOutput.SPDHeadRequestDotWeighting}");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    _syringeAmountStatusList.ConsumeSyringeAmount(head, _currentRecipe.CommonRecipe.ResinWeight);
                    Sequence = ESequence.AutoRun;
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
                        Step.RunStep = (int)ESPDHeadProcAssembleDisAssembleStep.WorkRequest_Wait;
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
                case ESPDHeadProcAssembleDisAssembleStep.WorkRequest_Wait:
                    if (procInputs[ESPDHeadProcInput.WorkRequest].Value == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug($"Input detect {ESPDHeadProcInput.WorkRequest}");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PAxis_AssemblePos_Move:
                    //if (PAxis.IsOnPosition(_pAxisAssemble_Pos))
                    //{
                    //    Step.RunStep = (int)ESPDHeadProcAssembleDisAssembleStep.PistonCyl_Down;
                    //    break;
                    //}

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
                case ESPDHeadProcAssembleDisAssembleStep.PistonCyl_Down:
                    Log.Debug($"{PistonCyl} moving down");
                    PistonCyl.Down();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, PistonCyl.IsDown);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PistonCyl_DownWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.H1_PistonCyl_DownFail);
                        break;
                    }

                    Log.Debug($"Move {PistonCyl} down done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.WaitDisOrAssembleSensorStatus:
                    Wait(30000, () => isAssemble == In_AssembleCheck.Value);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.DisOrAssembleSensorStatusCheck:
                    if (WaitTimeOutOccurred)
                    {
                        if (isAssemble) RaiseHeadWarning(EWarning.H1_Assemble_Check_Timeout);
                        else RaiseHeadWarning(EWarning.H1_Disassemble_Check_Timeout);
                        break;
                    }

                    if (!isAssemble)
                    {
                        Step.RunStep = (int)ESPDHeadProcAssembleDisAssembleStep.WorkDone_Send;
                        break;
                    }

                    Wait(3000);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.AssembleSensorStatus_Confirm:
                    if (In_AssembleCheck.Value == false)
                    {
                        Step.RunStep = (int)ESPDHeadProcAssembleDisAssembleStep.WaitDisOrAssembleSensorStatus;
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

                case ESPDHeadProcAssembleDisAssembleStep.GAxis_OpenPosition_Move:
                    Log.Debug($"{GAxis.Name} move to GateOpenPos [{_currentSPDHeadRecipe.GateOpenPos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateOpenPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => IsGateOnOpenPos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.GAxis_OpenPosition_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveOpenPos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moved to GateOpenPos [{_currentSPDHeadRecipe.GateOpenPos}°] done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PAxis_BasePosition_Move:
                    Log.Debug($"{PAxis.Name} move to BasePos [{_pAxisBase_Pos}mm]");
                    PAxis.MoveAbs(_pAxisBase_Pos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => PAxis.IsOnPosition(_pAxisBase_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PAxis_BasePosition_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1PAxis_MoveBasePos_Timeout);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moved to BasePos [{_pAxisBase_Pos}mm] done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.WorkDone_Send:
                    Log.Debug($"Set output {ESPDHeadProcOutput.InjectFinish}");
                    procOutputs[ESPDHeadProcOutput.InjectFinish].Value = true;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.WorkDone_Clear:
                    if (procInputs[ESPDHeadProcInput.WorkRequest].Value == true)
                    {
                        Wait(10);
                        break;
                    }

                    procOutputs[ESPDHeadProcOutput.InjectFinish].Value = false;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.End:
                    Log.Debug(isAssemble ? "Head Assemble End" : "Head DisAssemble End");
                    Sequence = ESequence.Stop;
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
        private readonly SyringAmountStatusList _syringeAmountStatusList;
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

        private double V380Weight2mg(double weight, double constant = 1)
        {
            return Math.Round(weight / (Math.Pow(2.5, 2) * Math.PI), 3);
        }

        private double _pAxisCharge_Pos;
        private double _pAxisCharge_Vel;
        private double _pAxisInject_Pos;
        private double _pAxisInject_Vel;
        private double _gAxisBubbleRemove_Pos;

        private double _pAxisAssemble_Pos = 13.4375;
        private double _pAxisBase_Pos = 20.875;

        public int _removeResinCount;
        private int _bubbleRemoveCount;

        private double _pAxisInjectCharge_Height;
        private double _pAxisInjectCharge_Pos => _pAxisBase_Pos - _pAxisInjectCharge_Height;
        #endregion
    }
}
