using EQX.Core.InOut;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.Device.Balance;
using EQX.InOut;
using EQX.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using ScottPlot.Plottables;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.MVVM.Models;
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
            MachineStatus machineStatus,
            CarrierJigStatusList carrierJigStatusList)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            _processIO = processIO;
            _balanceLeft = balanceLeft;
            _balanceRight = balanceRight;
            _syringeAmountStatusList = syringeAmountStatusList;
            _machineStatus = machineStatus;
            _carrierJigStatusList = carrierJigStatusList;
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
                case ESPDHeadProcToRunStep.SyringeAmountCheck:
                    Log.Debug("Syringe Amount Check");
                    if (SyringeAmountIsTimeOver && In_SyringeCheck.Value == true && CurrentHeadSkip == false)
                    {
                        RaiseHeadWarning(EWarning.VA_SPD_H01_SYRING_AMOUNT_TIMEOUT);
                    }

                    Step.ToRunStep++;
                    break;
                case ESPDHeadProcToRunStep.SyringeAirCheck:
                    Log.Debug("Syringe Air Check");
#if !SIMULATION
                    if (In_SyringeAir.Value == false && In_SyringeCheck.Value && CurrentHeadSkip == false)
                    {
                        RaiseHeadWarning(EWarning.SE_SPD_H01_SYRING_AIR_NOT_DETECT);
                    }
#endif

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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_CLOSE_POS_TIMEOUT);
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
                        RaiseHeadWarning(EWarning.CY_SPD_H01_PISTON_UP_FAIL);
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
                        RaiseHeadWarning(EWarning.MO_SPD_H01_G1_AXIS_HOME_TIMEOUT);
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_CLOSE_POS_TIMEOUT);
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
                    Log.Debug($"{PistonCyl} moving up");
                    PistonCyl.Backward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout, () => PistonCyl.IsBackward);
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.PistonCyl_UpWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.CY_SPD_H01_PISTON_UP_FAIL);
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
                        RaiseHeadWarning(EWarning.MO_SPD_H01_P1_AXIS_HOME_TIMEOUT);
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_OPEN_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moved to GateOpenPos [{_currentSPDHeadRecipe.GateOpenPos}°] done");
                    Log.Debug("Wait All P Axis Origin Done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.Wait_All_PAxis_OriginDone:
                    if (_devices.Motions.P1Axis.Status.IsHomeDone == false ||
                        _devices.Motions.P2Axis.Status.IsHomeDone == false ||
                        _devices.Motions.P3Axis.Status.IsHomeDone == false ||
                        _devices.Motions.P4Axis.Status.IsHomeDone == false)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("PAxis All Head Origin Done");
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_BASE_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moved to BasePos [{_pAxisBase_Pos}mm] done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.SetFlag_SPDHeadOriginDone:
                    Log.Debug($"Set flag {head} OriginDone");
                    procOutputs[ESPDHeadProcOutput.OriginDone].Value = true;
                    _syringeAmountStatusList.ConsumeSyringeAmount(head, _pAxisBase_Pos);
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
                    if (CurrentHeadSkip)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Sequence_AutoRun();
                    break;
                case ESequence.ResinInject:
                    if (CurrentHeadSkip)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Sequence_SPDHeadCommon(ESequence.ResinInject);
                    break;
                case ESequence.ResinInjectAddTail:
                    if (CurrentHeadSkip)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Sequence_SPDHeadAddTail();
                    break;
                case ESequence.IdlePurge:
                    if (CurrentHeadSkip)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Sequence_SPDHeadCommon(ESequence.IdlePurge);
                    break;
                case ESequence.DummyShot:
                    if (Parent!.Sequence == ESequence.AutoRun)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot);
                    }
                    else
                    {
                        if (head == ESPDHead.SPDHead1 && _machineStatus.IsSkipHead1)
                        {
                            Sequence = ESequence.Stop;
                            break;
                        }
                        if (head == ESPDHead.SPDHead2 && _machineStatus.IsSkipHead2)
                        {
                            Sequence = ESequence.Stop;
                            break;
                        }
                        if (head == ESPDHead.SPDHead3 && _machineStatus.IsSkipHead3)
                        {
                            Sequence = ESequence.Stop;
                            break;
                        }
                        if (head == ESPDHead.SPDHead4 && _machineStatus.IsSkipHead4)
                        {
                            Sequence = ESequence.Stop;
                            break;
                        }

                        Sequence_SPDHeadCommon(ESequence.DummyShot);
                    }
                    break;
                case ESequence.DummyShot_H1:
                    if (head == ESPDHead.SPDHead1 && CurrentHeadSkip == false)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot_H1);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DummyShot_H2:
                    if (head == ESPDHead.SPDHead2 && CurrentHeadSkip == false)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot_H2);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DummyShot_H3:
                    if (head == ESPDHead.SPDHead3 && CurrentHeadSkip == false)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot_H3);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DummyShot_H4:
                    if (head == ESPDHead.SPDHead4 && CurrentHeadSkip == false)
                    {
                        Sequence_SPDHeadCommon(ESequence.DummyShot_H4);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DotWeighting:
                    if (head == ESPDHead.SPDHead1 && _machineStatus.IsSkipHead1)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead2 && _machineStatus.IsSkipHead2)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead3 && _machineStatus.IsSkipHead3)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead4 && _machineStatus.IsSkipHead4)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Sequence_DotWeighting();
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
                case ESequence.BubbleRemove:
                    if (Parent!.Sequence == ESequence.AutoRun)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove);
                    }
                    else
                    {
                        if (head == ESPDHead.SPDHead1 && _machineStatus.IsSkipHead1)
                        {
                            Sequence = ESequence.Stop;
                            break;
                        }
                        if (head == ESPDHead.SPDHead2 && _machineStatus.IsSkipHead2)
                        {
                            Sequence = ESequence.Stop;
                            break;
                        }
                        if (head == ESPDHead.SPDHead3 && _machineStatus.IsSkipHead3)
                        {
                            Sequence = ESequence.Stop;
                            break;
                        }
                        if (head == ESPDHead.SPDHead4 && _machineStatus.IsSkipHead4)
                        {
                            Sequence = ESequence.Stop;
                            break;
                        }

                        Sequence_SPDHeadCommon(ESequence.BubbleRemove);
                    }
                    break;
                case ESequence.BubbleRemove_H1:
                    if (head == ESPDHead.SPDHead1 && CurrentHeadSkip == false)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove_H1);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.BubbleRemove_H2:
                    if (head == ESPDHead.SPDHead2 && CurrentHeadSkip == false)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove_H2);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.BubbleRemove_H3:
                    if (head == ESPDHead.SPDHead3 && CurrentHeadSkip == false)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove_H3);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.BubbleRemove_H4:
                    if (head == ESPDHead.SPDHead4 && CurrentHeadSkip == false)
                    {
                        Sequence_SPDHeadCommon(ESequence.BubbleRemove_H4);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadAssemble:
                    if (head == ESPDHead.SPDHead1 && _machineStatus.IsSkipHead1)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead2 && _machineStatus.IsSkipHead2)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead3 && _machineStatus.IsSkipHead3)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead4 && _machineStatus.IsSkipHead4)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    break;
                case ESequence.HeadAssemble_H1:
                    if (head == ESPDHead.SPDHead1)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadAssemble_H2:
                    if (head == ESPDHead.SPDHead2)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadAssemble_H3:
                    if (head == ESPDHead.SPDHead3)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadAssemble_H4:
                    if (head == ESPDHead.SPDHead4)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: true);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadDisassemble:
                    if (head == ESPDHead.SPDHead1 && _machineStatus.IsSkipHead1)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead2 && _machineStatus.IsSkipHead2)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead3 && _machineStatus.IsSkipHead3)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead4 && _machineStatus.IsSkipHead4)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    break;
                case ESequence.HeadDisassemble_H1:
                    if (head == ESPDHead.SPDHead1)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadDisassemble_H2:
                    if (head == ESPDHead.SPDHead2)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadDisassemble_H3:
                    if (head == ESPDHead.SPDHead3)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.HeadDisassemble_H4:
                    if (head == ESPDHead.SPDHead4)
                    {
                        Sequence_HeadAssembleDisAssemble(isAssemble: false);
                    }
                    else
                    {
                        Sequence = ESequence.Stop;
                    }
                    break;
                case ESequence.DrainShot:
                    if (head == ESPDHead.SPDHead1 && _machineStatus.IsSkipHead1)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead2 && _machineStatus.IsSkipHead2)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead3 && _machineStatus.IsSkipHead3)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
                    if (head == ESPDHead.SPDHead4 && _machineStatus.IsSkipHead4)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Sequence_SPDHeadCommon(ESequence.DrainShot);
                    break;
                default:
                    Sequence = ESequence.Stop;
                    break;
            }

            return true;
        }

        public override bool PreProcess()
        {
            if (_injectSequenceStartTick >= 0 && EnableTimerInject)
            {
                CarrierJigStatus.InjectTime = (Environment.TickCount64 - _injectSequenceStartTick) / 1000.0;
            }
#if !SIMULATION
            if (In_SyringeCheck.Value == false &&
                In_AssembleCheck.Value == true &&
                CurrentHeadSkip == false &&
                _machineStatus.DisableSyringeCheck == false &&
                (ProcessMode == EProcessMode.ToRun || ProcessMode == EProcessMode.Run) &&
                IsSequenceAssembleOrDisassemble(Sequence))
            {
                RaiseHeadWarning(EWarning.SE_SPD_H01_SYRING_NOT_DETECT);
            }
#endif
            return base.PreProcess();
        }
        #endregion

        public override bool ProcessToAlarm()
        {
            EnableTimerInject = false;
            return base.ProcessToAlarm();
        }

        public override bool ProcessToWarning()
        {
            EnableTimerInject = false;
            return base.ProcessToWarning();
        }

        public override bool ProcessToStop()
        {
            EnableTimerInject = false;
            return base.ProcessToStop();
        }

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
                    _bubbleRemoveCount = 1;
                    if (_machineStatus.IsDryRunMode)
                    {
                        Step.RunStep = (int)ESPDHeadProcCommonStep.WorkDone_Send;
                        break;
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.ResetInjectTime:
                    if (sequence == ESequence.ResinInject)
                    {
                        CarrierJigStatus.InjectTime = 0;
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.BubbleRemoveResetCountRotate:
                    if (sequence == ESequence.BubbleRemove ||
                        sequence == ESequence.BubbleRemove_H1 ||
                        sequence == ESequence.BubbleRemove_H2 ||
                        sequence == ESequence.BubbleRemove_H3 ||
                        sequence == ESequence.BubbleRemove_H4)
                    {
                        Log.Debug("Bubble Remove Reset Count Rotate For Cycle Bubble Remove New");
                        _bubbleRemoveRotateCount = 1;
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_CLOSE_POS_TIMEOUT);
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
                        case ESequence.DrainShot:
                            _pAxisCharge_Pos = 1;
                            break;
                        case ESequence.IdlePurge:
                            double heightChargeIdlePurge = _currentRecipe.IdlePurgeRecipe.IdlePurgeWeight * ((_pAxisBase_Pos - _currentSPDHeadRecipe.PAxisInjectChargePos) / _currentSPDHeadRecipe.ResinWeight);
                            _pAxisCharge_Pos = _pAxisBase_Pos - heightChargeIdlePurge;
                            break;
                        case ESequence.DummyShot:
                        case ESequence.DummyShot_H1:
                        case ESequence.DummyShot_H2:
                        case ESequence.DummyShot_H3:
                        case ESequence.DummyShot_H4:
                            double heightChargeDummyShot = _currentSPDHeadRecipe.DummyShotWeight * ((_pAxisBase_Pos - _currentSPDHeadRecipe.PAxisInjectChargePos) / _currentSPDHeadRecipe.ResinWeight);
                            _pAxisCharge_Pos = _pAxisBase_Pos - heightChargeDummyShot;
                            break;
                        case ESequence.BubbleRemove:
                        case ESequence.BubbleRemove_H1:
                        case ESequence.BubbleRemove_H2:
                        case ESequence.BubbleRemove_H3:
                        case ESequence.BubbleRemove_H4:
                            double heightChargeBubbleRemove = _currentSPDHeadRecipe.BubbleRemoveWeight * ((_pAxisBase_Pos - _currentSPDHeadRecipe.PAxisInjectChargePos) / _currentSPDHeadRecipe.ResinWeight);
                            _pAxisCharge_Pos = _pAxisBase_Pos - heightChargeBubbleRemove;
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_CHARGE_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to ChargePos done");
                    Log.Debug("Wait inject request work");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.Base_PosVel_Calculte:
                    Log.Debug("Calculate InjectPos and Velocity");
                    _pAxisInject_Pos = _pAxisBase_Pos;
                    switch (sequence)
                    {
                        case ESequence.ResinInject:
                            if (_currentRecipe.OptionRecipe.EnableInjectionPath)
                            {
                                injectPathIndexer = 0;

                                double sumDistance = 0;
                                InjectPaths.ForEach(path => sumDistance += path.VelocityRate * 0.01 * path.TimeInSecond);
                                _pAxisInject_Vel = Math.Abs(_currentSPDHeadRecipe.PAxisInjectChargePos - _pAxisBase_Pos)
                                    / sumDistance;

                                InjectPathPositions = new List<double>();
                                for (int i = 0; i < InjectPaths.Count; i++)
                                {
                                    double lastPostion = i == 0 ? _currentSPDHeadRecipe.PAxisInjectChargePos : InjectPathPositions[i - 1];
                                    double distance = _pAxisInject_Vel * (InjectPaths[i].VelocityRate * 0.01) * InjectPaths[i].TimeInSecond;
                                    double injectPos = lastPostion + distance;

                                    if (injectPos > _pAxisBase_Pos)
                                    {
                                        RaiseHeadWarning(EWarning.MO_SPD_H01_INJECT_POS_OVER_BASE);
                                        break;
                                    }

                                    InjectPathPositions.Add(injectPos);
                                }
                                break;
                            }

                            _pAxisInject_Vel = Math.Abs(_currentSPDHeadRecipe.PAxisInjectChargePos - _pAxisBase_Pos) / InjectTimeRecipe.InjectTime;
                            CarrierJigStatus.PAxisInjectVelocity = _pAxisInject_Vel; // Display UI
                            break;
                        case ESequence.DrainShot:
                        case ESequence.IdlePurge:
                            _pAxisInject_Vel = 10;
                            break;
                        case ESequence.DummyShot:
                        case ESequence.DummyShot_H1:
                        case ESequence.DummyShot_H2:
                        case ESequence.DummyShot_H3:
                        case ESequence.DummyShot_H4:
                            _pAxisInject_Vel = 10;
                            break;
                        case ESequence.BubbleRemove:
                        case ESequence.BubbleRemove_H1:
                        case ESequence.BubbleRemove_H2:
                        case ESequence.BubbleRemove_H3:
                        case ESequence.BubbleRemove_H4:
                            if (_bubbleRemoveRotateCount != 2)
                            {
                                _pAxisInject_Pos = _pAxisCharge_Pos + (_bubbleRemoveRotateCount * Math.Abs(_pAxisCharge_Pos - _pAxisBase_Pos) / _bubbleRemoveTotalRotateCount);
                            }

                            _gAxisBubbleRemove_Pos = GAxis.Status.ActualPosition + 180;
                            _pAxisInject_Vel = 20;
                            break;
                        default: throw new NotImplementedException();
                    }

                    if (_pAxisInject_Pos > _pAxisBase_Pos)
                    {
                        RaiseHeadWarning(EWarning.MO_SPD_H01_INJECT_POS_OVER_BASE);
                        break;
                    }

                    if (sequence != ESequence.BubbleRemove &&
                        sequence != ESequence.BubbleRemove_H2 &&
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
                    GAxis.MoveAbs(_gAxisBubbleRemove_Pos, GAxis.Parameter.Velocity * 2);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () =>
                            GAxis.IsOnPosition(_gAxisBubbleRemove_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.GAxis_BubbleRemove_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadWarning(EWarning.MO_SPD_H01_G1_AXIS_BUBBLE_REMOVE_TIMEOUT);
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_OPEN_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to OpenPos done");
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
                case ESPDHeadProcCommonStep.PAxis_InjectPos_Move:
                    if (sequence == ESequence.ResinInject)
                    {
                        if (Parent?.Sequence == ESequence.AutoRun)
                        {
                            IsCanStop = false;
                        }
                        if (_currentRecipe.OptionRecipe.EnableInjectionPath)
                        {
                            if (injectPathIndexer > InjectPaths.Count - 1)
                            {
                                EnableTimerInject = false;
                                IsCanStop = true;
                                Step.RunStep = (int)ESPDHeadProcCommonStep.WorkDone_Send;
                                break;
                            }

                            if (InjectPaths[injectPathIndexer].IsEnabled == false)
                            {
                                injectPathIndexer++;
                                break;
                            }

                            if (injectPathIndexer == 0)
                            {
                                EnableTimerInject = true;
                                _injectSequenceStartTick = Environment.TickCount64;
                            }

                            CarrierJigStatus.PAxisInjectVelocity = InjectPaths[injectPathIndexer].VelocityRate * 0.01 * _pAxisInject_Vel; // Display UI
                            Log.Debug($"{PAxis.Name} moving to InjectPos #{injectPathIndexer} [{InjectPathPositions[injectPathIndexer]}mm]");
                            PAxis.MoveAbs(InjectPathPositions[injectPathIndexer],
                                InjectPaths[injectPathIndexer].VelocityRate * 0.01 * _pAxisInject_Vel);

                            Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout + InjectPaths[injectPathIndexer].TimeInSecond,
                                () => PAxis.IsOnPosition(InjectPathPositions[injectPathIndexer]));

                            Step.RunStep++;
                            break;
                        }
                        else
                        {
                            Log.Debug($"{PAxis.Name} moving to InjectPos [{_pAxisInject_Pos}mm]");
                            PAxis.MoveAbs(_pAxisInject_Pos, _pAxisInject_Vel);
                            Log.Debug("Start inject timer");
                            CarrierJigStatus.InjectTime = 0;
                            EnableTimerInject = true;
                            _injectSequenceStartTick = Environment.TickCount64;
                            Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout + InjectTimeRecipe.InjectTime,
                                () => PAxis.IsOnPosition(_pAxisInject_Pos));
                        }
                    }
                    else
                    {
                        Log.Debug($"{PAxis.Name} moving to InjectPos [{_pAxisInject_Pos}mm]");
                        PAxis.MoveAbs(_pAxisInject_Pos, _pAxisInject_Vel);
                        Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                            () => PAxis.IsOnPosition(_pAxisInject_Pos));
                    }

                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.PAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_INJECT_POS_TIMEOUT);
                        break;
                    }

                    if (_currentRecipe.OptionRecipe.EnableInjectionPath && sequence == ESequence.ResinInject)
                    {
                        Log.Debug($"{PAxis.Name} move to InjectPos #{injectPathIndexer} [{InjectPathPositions[injectPathIndexer]}mm] DONE");
                        injectPathIndexer++;

                        Step.RunStep = (int)ESPDHeadProcCommonStep.PAxis_InjectPos_Move;
                        break;
                    }

                    if (sequence == ESequence.BubbleRemove ||
                        sequence == ESequence.BubbleRemove_H1 ||
                        sequence == ESequence.BubbleRemove_H2 ||
                        sequence == ESequence.BubbleRemove_H3 ||
                        sequence == ESequence.BubbleRemove_H4)
                    {
                        _bubbleRemoveRotateCount++;
                        Log.Debug($"Bubble remove rotate count: {_bubbleRemoveRotateCount}");
                        if (_bubbleRemoveRotateCount <= _bubbleRemoveTotalRotateCount)
                        {
                            Step.RunStep = (int)ESPDHeadProcCommonStep.Base_PosVel_Calculte;
                            break;
                        }
                        else
                        {
                            _bubbleRemoveCount++;
                            if (_bubbleRemoveCount <= _currentRecipe.AdditionalMolding_Recipe.BubbleRemoveCount)
                            {
                                _syringeAmountStatusList.ConsumeSyringeAmount(head, _pAxisBase_Pos - _pAxisCharge_Pos);
                                Step.RunStep = (int)ESPDHeadProcCommonStep.BubbleRemoveResetCountRotate;
                                break;
                            }
                        }
                    }

                    EnableTimerInject = false;
                    IsCanStop = true;
                    Log.Debug($"{PAxis.Name} moving to InjectPos done");

                    // Consume syringe amount
                    _syringeAmountStatusList.ConsumeSyringeAmount(head, _pAxisBase_Pos - _pAxisCharge_Pos);
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

                    Log.Debug($"Clear output {ESPDHeadProcOutput.InjectFinish} done");
                    procOutputs[ESPDHeadProcOutput.InjectFinish].Value = false;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcCommonStep.End:
                    if (sequence == ESequence.ResinInject && _currentRecipe.AdditionalMolding_Recipe.UseAddTail)
                    {
                        Log.Info($"Set next sequence SPDHeadAddTail");
                        Sequence = ESequence.ResinInjectAddTail;
                        break;
                    }
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Debug($"{sequence} end");
                    if (sequence == ESequence.ResinInject)
                    {
                        Log.Info($"Set next sequence DummyShot");
                        Sequence = ESequence.DummyShot;
                    }
                    else
                        Sequence = ESequence.ResinInject;
                    break;
            }
        }

        private void Sequence_SPDHeadAddTail()
        {
            switch ((ESPDHeadProcAddTailStep)Step.RunStep)
            {
                case ESPDHeadProcAddTailStep.Start:
                    Log.Debug("Add tail start");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.Gate_Close:
                    if (IsGateOnClosePos)
                    {
                        Step.RunStep = (int)ESPDHeadProcAddTailStep.Charge_PosVel_Calculte;
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to ClosePos [{_currentSPDHeadRecipe.GateClosePos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateClosePos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => IsGateOnClosePos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.Gate_CloseWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_CLOSE_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to ClosePos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.Charge_PosVel_Calculte:
                    double heightAddTail = _currentRecipe.AdditionalMolding_Recipe.AddTailWeight * ((_pAxisBase_Pos - _currentSPDHeadRecipe.PAxisInjectChargePos) / _currentSPDHeadRecipe.ResinWeight);
                    _pAxisCharge_Pos = _pAxisBase_Pos - heightAddTail;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.PAxis_ChargePos_Move:
                    Log.Debug($"{PAxis.Name} moving to ChargePos [{_pAxisCharge_Pos}mm]");
                    PAxis.MoveAbs(_pAxisCharge_Pos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => PAxis.IsOnPosition(_pAxisCharge_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.PAxis_ChargePos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_CHARGE_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to ChargePos done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.Base_PosVel_Calculte:
                    Log.Debug("Calculate InjectPos and Velocity");
                    _pAxisInject_Pos = _pAxisBase_Pos;
                    _pAxisInject_Vel = _currentRecipe.AdditionalMolding_Recipe.AddTailSpeed;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.Gate_Open:
                    Log.Debug($"{GAxis.Name} moving to OpenPos [{_currentSPDHeadRecipe.GateOpenPos}°]");
                    GAxis.MoveAbs(_currentSPDHeadRecipe.GateOpenPos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => IsGateOnOpenPos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.Gate_OpenWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_OPEN_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moving to OpenPos done");
                    Log.Debug("Wait InjectAddTail Request");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.Wait_ZAxisReady_ForInjectAddTail:
                    if (procInputs[ESPDHeadProcInput.InjectAddTail_Request].Value == false)
                    {
                        Wait(50);
                        break;
                    }

                    Log.Debug("Input detect InjectAddTail Request");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.PAxis_InjectPos_Move:
                    Log.Debug("Start inject add tail");
                    PAxis.MoveAbs(_pAxisInject_Pos, _pAxisInject_Vel);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout + Math.Abs(_pAxisBase_Pos - _pAxisInject_Pos) / _currentRecipe.AdditionalMolding_Recipe.AddTailSpeed,
                        () => PAxis.IsOnPosition(_pAxisInject_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.PAxis_InjectPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_INJECT_POS_TIMEOUT);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} moving to InjectPos done");
                    // Consume syringe amount
                    _syringeAmountStatusList.ConsumeSyringeAmount(head, _pAxisBase_Pos - _pAxisCharge_Pos);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.WorkDone_Send:
                    Log.Debug($"Set output {ESPDHeadProcOutput.InjectAddTailFinish}");
                    procOutputs[ESPDHeadProcOutput.InjectAddTailFinish].Value = true;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.WorkDone_Clear:
                    if (procInputs[ESPDHeadProcInput.InjectAddTail_Request].Value == true)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug($"Clear output {ESPDHeadProcOutput.InjectAddTailFinish} done");
                    procOutputs[ESPDHeadProcOutput.InjectAddTailFinish].Value = false;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAddTailStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }

                    Log.Info($"Set next sequence DummyShot");
                    Sequence = ESequence.DummyShot;
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
                case ESPDHeadProcDotWeightingStep.Charge_PosVel_Calculte:
                    Log.Debug("Calculate Charge Position");
                    if (_machineStatus.IsDotWeightingTest == false)
                    {
                        _pAxisInjectCharge_Height = V380Weight2mg(_currentSPDHeadRecipe.ResinWeight);
                    }
                    else
                    {
                        _pAxisInjectCharge_Height = _pAxisBase_Pos - _currentSPDHeadRecipe.PAxisInjectChargePos;
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_CLOSE_POS_TIMEOUT);
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_CHARGE_POS_TIMEOUT);
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_OPEN_POS_TIMEOUT);
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
                        RaiseHeadWarning(EWarning.VA_SPD_H01_BALANCE_ZERO_FAIL);
                        break;
                    }

                    double weight = Balance.WeightData.Weight * 1000;
                    if (weight < -3 || weight > 3)
                    {
                        RaiseHeadWarning(EWarning.VA_SPD_H01_BALANCE_ZERO_FAIL);
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_INJECT_POS_TIMEOUT);
                        break;
                    }

                    // Consume syringe amount for dot weighting
                    _syringeAmountStatusList.ConsumeSyringeAmount(head, _pAxisBase_Pos - _pAxisCharge_Pos);

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
                        RaiseHeadWarning(EWarning.VA_SPD_H01_BALANCE_REQ_WEIGHT_FAIL);
                        break;
                    }

                    if (Balance.WeightData.IsStable == false)
                    {
                        RaiseHeadWarning(EWarning.VA_SPD_H01_BALANCE_NOT_STABLE);
                        break;
                    }

                    double _calibWeight_mg = Balance.WeightData.Weight * (Balance.WeightData.Unit == "g" ? 1000 : 1); // g -> mg

                    if (_calibWeight_mg <= 0)
                    {
                        RaiseHeadWarning(EWarning.VA_SPD_H01_BALANCE_WEIGH_FAIL);
                        break;
                    }

                    Log.Info($"WEIGHT = {_calibWeight_mg} [mg] | IsStable = {Balance.WeightData.IsStable}");

                    if (_machineStatus.IsDotWeightingTest)
                    {
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.SetFlag_SPDHead_DotWeighting_Done;
                        break;
                    }

                    double target = _currentSPDHeadRecipe.ResinWeight;
                    double tolerance = _currentSPDHeadRecipe.ResinWeightSpec;

                    if (Math.Abs(_calibWeight_mg - target) <= tolerance)
                    {
                        Log.Debug("DotWeighting Pass");
                        _currentSPDHeadRecipe.PAxisInjectChargePos = _pAxisInjectCharge_Pos;
                        _recipeSelector.Save();
                        Step.RunStep = (int)ESPDHeadProcDotWeightingStep.SetFlag_SPDHead_DotWeighting_Done;
                        break;
                    }

                    _pAxisInjectCharge_Height = (_pAxisInjectCharge_Height * _currentSPDHeadRecipe.ResinWeight) / _calibWeight_mg;

                    Step.RunStep = (int)ESPDHeadProcDotWeightingStep.Gate_Close;
                    break;
                case ESPDHeadProcDotWeightingStep.SetFlag_SPDHead_DotWeighting_Done:
                    Log.Debug("Set flag Dot Weighting Done");
                    procOutputs[ESPDHeadProcOutput.SPDHeadDotWeightingDone].Value = true;
                    _machineStatus.MachineCalibration[(int)head - 1] = true;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.ClearFlag_SPDHead_DotWeighting_Done:
                    if (procInputs[ESPDHeadProcInput.XYAxisInDotWeightingPos].Value)
                    {
                        Wait(20);
                        break;
                    }

                    Log.Debug("CSlear flag SPDHead DotWeighting done");
                    procOutputs[ESPDHeadProcOutput.SPDHeadDotWeightingDone].Value = false;
                    Step.RunStep++;
                    break;
                case ESPDHeadProcDotWeightingStep.End:
                    if (Parent?.Sequence != ESequence.AutoRun)
                    {
                        Sequence = ESequence.Stop;
                        break;
                    }
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_CLOSE_POS_TIMEOUT);
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
                    Log.Debug($"{PAxis.Name} moving to AssemblePos [{_pAxisAssemble_Pos}mm]");
                    PAxis.MoveAbs(_pAxisAssemble_Pos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, () => PAxis.IsOnPosition(_pAxisAssemble_Pos));
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.PAxis_AssemblePos_Move_Wait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_ASSEMBLE_POS_TIMEOUT);
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
                        RaiseHeadWarning(EWarning.CY_SPD_H01_PISTON_DOWN_FAIL);
                        break;
                    }

                    Log.Debug($"Move {PistonCyl} down done");
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.WaitDisOrAssembleSensorStatus:
                    Wait(60000, () => isAssemble == In_AssembleCheck.Value);
                    Step.RunStep++;
                    break;
                case ESPDHeadProcAssembleDisAssembleStep.DisOrAssembleSensorStatusCheck:
                    if (WaitTimeOutOccurred)
                    {
                        if (isAssemble) RaiseHeadWarning(EWarning.MO_SPD_H01_ASSEMBLE_CHECK_TIMEOUT);
                        else RaiseHeadWarning(EWarning.MO_SPD_H01_DISASSEMBLE_CHECK_TIMEOUT);
                        break;
                    }

                    if (!isAssemble)
                    {
                        Step.RunStep = (int)ESPDHeadProcAssembleDisAssembleStep.WorkDone_Send;
                        break;
                    }

                    Wait(1500);
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
                        RaiseHeadWarning(EWarning.CY_SPD_H01_PISTON_UP_FAIL);
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_G1_AXIS_OPEN_POS_TIMEOUT);
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
                        RaiseHeadAlarm(EAlarm.MO_SPD_H01_P1_AXIS_BASE_POS_TIMEOUT);
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
                    string message = isAssemble ? $"{head} Assemble Finish!" : $"{head} DisAssemble Finish!";
                    MessageBoxEx.Show(message, false);
                    Sequence = ESequence.Stop;
                    break;
            }
        }
        #endregion

        #region Private Methods
        private void RaiseHeadWarning(EWarning warning)
        {
            RaiseWarning(warning + 500 * ((int)head - 1));
        }
        private void RaiseHeadAlarm(EAlarm warning)
        {
            RaiseAlarm(warning + 500 * ((int)head - 1));
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
        private readonly CarrierJigStatusList _carrierJigStatusList;
        public double PAxisInjectVelocity => _pAxisInject_Vel;

        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;

        private List<InjectPath> InjectPaths => Name switch
        {
            "SPDHead1" => _currentRecipe.InjectTimeRecipe.H1H3InjectPaths,
            "SPDHead2" => _currentRecipe.InjectTimeRecipe.H2H4InjectPaths,
            "SPDHead3" => _currentRecipe.InjectTimeRecipe.H1H3InjectPaths,
            "SPDHead4" => _currentRecipe.InjectTimeRecipe.H2H4InjectPaths,
            _ => throw new Exception($"Invalid process name: {Name}")
        };

        private List<double> InjectPathPositions = new List<double>();

        private int injectPathIndexer = 0;

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

        private CarrierJigStatus CarrierJigStatus => Name switch
        {
            "SPDHead1" => _carrierJigStatusList.CarrierJigStatusH1,
            "SPDHead2" => _carrierJigStatusList.CarrierJigStatusH2,
            "SPDHead3" => _carrierJigStatusList.CarrierJigStatusH3,
            "SPDHead4" => _carrierJigStatusList.CarrierJigStatusH4,
            _ => throw new Exception($"Invalid process name: {Name}")
        };

        private bool SyringeAmountIsTimeOver => Name switch
        {
            "SPDHead1" => _syringeAmountStatusList.SyringeAmounts[0].IsTimeOver,
            "SPDHead2" => _syringeAmountStatusList.SyringeAmounts[1].IsTimeOver,
            "SPDHead3" => _syringeAmountStatusList.SyringeAmounts[2].IsTimeOver,
            "SPDHead4" => _syringeAmountStatusList.SyringeAmounts[3].IsTimeOver,
            _ => throw new Exception($"Invalid process name: {Name}")
        };

        private bool CurrentHeadSkip => Name switch
        {
            "SPDHead1" => OptionRecipe.SkipHead12,
            "SPDHead2" => OptionRecipe.SkipHead12,
            "SPDHead3" => OptionRecipe.SkipHead34,
            "SPDHead4" => OptionRecipe.SkipHead34,
            _ => throw new Exception($"Invalid process name: {Name}")
        };

        private bool IsSequenceAssembleOrDisassemble(ESequence sequence)
        {
            return sequence == ESequence.HeadAssemble || sequence == ESequence.HeadDisassemble ||
                   sequence == ESequence.HeadAssemble_H1 || sequence == ESequence.HeadAssemble_H2 ||
                   sequence == ESequence.HeadAssemble_H3 || sequence == ESequence.HeadAssemble_H4 ||
                   sequence == ESequence.HeadDisassemble_H1 || sequence == ESequence.HeadDisassemble_H2 ||
                   sequence == ESequence.HeadDisassemble_H3 || sequence == ESequence.HeadDisassemble_H4;
        }

        private double V380Weight2mg(double weight, double constant = 1)
        {
            return Math.Round(weight / (Math.Pow(2.5, 2) * Math.PI * rho_Resin), 3);
        }

        private OptionRecipe OptionRecipe => _recipeSelector.CurrentRecipe.OptionRecipe;
        private InjectTimeRecipe InjectTimeRecipe => _recipeSelector.CurrentRecipe.InjectTimeRecipe;

        private double _pAxisCharge_Pos;
        private double _pAxisCharge_Vel;
        private double _pAxisInject_Pos;
        private double _pAxisInject_Vel;
        private double _gAxisBubbleRemove_Pos;

        private double _pAxisAssemble_Pos = 13.4375;
        private double _pAxisBase_Pos = 20.875;
        private double rho_Resin = 1.136;

        public int _removeResinCount;
        private int _bubbleRemoveCount;
        private int _bubbleRemoveRotateCount;
        private int _bubbleRemoveTotalRotateCount = 2;
        private long _injectSequenceStartTick = -1;

        private double _pAxisInjectCharge_Height;
        private double _pAxisInjectCharge_Pos => _pAxisBase_Pos - _pAxisInjectCharge_Height;

        private bool EnableTimerInject;
        private bool InjectAddTail;
        #endregion
    }
}
