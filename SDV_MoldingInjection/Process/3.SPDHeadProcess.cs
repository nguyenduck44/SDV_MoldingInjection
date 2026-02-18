using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;

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

        public SPDHeadProcess(Devices devices, RecipeSelector recipeSelector)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
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
                    if (GAxis.IsOnPosition(_spdHeadRecipe.GateClosePos))
                    {
                        Step.ToRunStep = (int)ESPDHeadProcToRunStep.PistonCyl_Up;
                        break;
                    }

                    Log.Debug($"{GAxis.Name} move to GateClosePos [{_spdHeadRecipe.GateClosePos}°]");
                    GAxis.MoveAbs(_spdHeadRecipe.GateClosePos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => GAxis.IsOnPosition(_spdHeadRecipe.GateClosePos));
                    Step.ToRunStep++;
                    break;
                case ESPDHeadProcToRunStep.GAxis_ClosePosition_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveOpenPos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moved to GateClosePos [{_spdHeadRecipe.GateClosePos}°] done");
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
                        RaiseHeadWarning(EWarning.G1Axis_Origin_TimeOut);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} origin search done");
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.GAxis_ClosePosition_Move:
                    Log.Debug($"{GAxis.Name} move to GateClosePos [{_spdHeadRecipe.GateClosePos}°]");
                    GAxis.MoveAbs(_spdHeadRecipe.GateClosePos);
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout,
                        () => GAxis.IsOnPosition(_spdHeadRecipe.GateClosePos));
                    Step.OriginStep++;
                    break;
                case ESPDHeadProcOriginStep.GAxis_ClosePosition_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseHeadAlarm(EAlarm.H1GAxis_MoveOpenPos_Timeout);
                        break;
                    }

                    Log.Debug($"{GAxis.Name} moved to GateClosePos [{_spdHeadRecipe.GateClosePos}°] done");
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
                        RaiseHeadWarning(EWarning.P1Axis_Origin_TimeOut);
                        break;
                    }

                    Log.Debug($"{PAxis.Name} origin search done");
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
                case ESequence.AutoRun:
                    Sequence_AutoRun();
                    break;
                case ESequence.Ready:
                    Sequence_Ready();
                    break;
                case ESequence.Loading:
                    break;
                case ESequence.ResinInject:
                    break;
                case ESequence.Unloading:
                    break;
                case ESequence.DummyShot:
                    break;
                case ESequence.NeedleCleaning:
                    break;
                case ESequence.DotWeighting:
                    break;
                case ESequence.HeadAssemble:
                    break;
                case ESequence.HeadDisassemble:
                    break;
                case ESequence.BubbleRemove:
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
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;

        private ESPDHead head => Name switch
        {
            "SPDHead1" => ESPDHead.SPDHead1,
            "SPDHead2" => ESPDHead.SPDHead2,
            "SPDHead3" => ESPDHead.SPDHead3,
            "SPDHead4" => ESPDHead.SPDHead4,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        private SPDHeadRecipe _spdHeadRecipe => Name switch
        {
            "SPDHead1" => _currentRecipe.SPDHead1_Recipe,
            "SPDHead2" => _currentRecipe.SPDHead2_Recipe,
            "SPDHead3" => _currentRecipe.SPDHead3_Recipe,
            "SPDHead4" => _currentRecipe.SPDHead4_Recipe,
            _ => throw new Exception($"Invalid process name: {Name}")
        };
        #endregion
    }
}
