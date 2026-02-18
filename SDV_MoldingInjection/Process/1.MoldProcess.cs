using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Process
{
    public class MoldProcess : MIProcess
    {
        #region Motions
        private IMotion XAxis => _devices.Motions.XAxis;
        private IMotion YAxis => _devices.Motions.StageYAxis;
        private IMotion Z1Axis => _devices.Motions.Z1Axis;
        private IMotion Z2Axis => _devices.Motions.Z2Axis;
        private IMotion Z3Axis => _devices.Motions.Z3Axis;
        private IMotion Z4Axis => _devices.Motions.Z4Axis;
        #endregion

        #region Cylinders
        private ICylinder NozzleClean_H1 => _devices.Cylinders.NozzleClean_H1;
        private ICylinder NozzleClean_H2 => _devices.Cylinders.NozzleClean_H2;
        private ICylinder NozzleClean_H3 => _devices.Cylinders.NozzleClean_H3;
        private ICylinder NozzleClean_H4 => _devices.Cylinders.NozzleClean_H4;

        private ICylinder BellowUpDown => _devices.Cylinders.BellowUpDown;
        private ICylinder ChamberOpenClose => _devices.Cylinders.ChamberOpenClose;
        #endregion

        #region Inputs
        #endregion

        #region Process IOs
        private IDInputDevice<EMoldProcInput> procInputs => _processIO.MoldProcInput;
        private IDOutputDevice<EMoldProcOutput> procOutputs => _processIO.MoldProcOutput;
        #endregion

        #region Constructors
        public MoldProcess(Devices devices, ProcessIO processIO,
            RecipeSelector recipeSelector)
        {
            _devices = devices;
            _processIO = processIO;
            _recipeSelector = recipeSelector;
        }
        #endregion

        #region Process Methods
        public override bool ProcessToRun()
        {
            switch ((EMoldProcToRunStep)Step.ToRunStep)
            {
                case EMoldProcToRunStep.Start:
                    Log.Debug("ToRun start");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.CheckIfChamberOpen:
                    if (ChamberOpenClose.IsForward)
                    {
                        RaiseWarning(EWarning.Mold_Chamber_OpenWarning);
                        return false;
                    }

                    Log.Debug("Chamber is closed");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Bellow_Down:
                    if (BellowUpDown.IsBackward)
                    {
                        Step.ToRunStep = (int)EMoldProcToRunStep.ZAxis_SafetyPos_Move;
                        break;
                    }

                    Log.Debug($"{BellowUpDown} moving down");
                    BellowUpDown.Backward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => BellowUpDown.IsBackward);
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.Bellow_DownWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.BellowUpDown_DownFail);
                        break;
                    }

                    Log.Debug($"Move {BellowUpDown} down done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ZAxis_SafetyPos_Move:
                    if (AllZAxisInSafetyPos())
                    {
                        Step.ToRunStep = (int)EMoldProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"Moving ZAxis safety position");
                    ZAxisSafetyPosMove();
                    Wait(_currentRecipe.CommonRecipe.MotionMoveTimeout, AllZAxisInSafetyPos);
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.ZAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z1Axis_SafetyPos_MoveTimeOut);
                        if (!Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z2Axis_SafetyPos_MoveTimeOut);
                        if (!Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z3Axis_SafetyPos_MoveTimeOut);
                        if (!Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos))
                            RaiseWarning(EWarning.Z4Axis_SafetyPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug($"ZAxis move safety position done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.XYAxis_SafetyPos_Move:
                    if (XAxis.IsOnPosition(_currentRecipe.MoldRecipe.XAxisReadyPos)
                        && YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisReadyPos))
                    {
                        Step.ToRunStep = (int)EMoldProcToRunStep.End;
                        break;
                    }

                    Log.Debug($"Moving XAxis/YAxis to ready position");
                    XAxis.MoveAbs(_currentRecipe.MoldRecipe.XAxisReadyPos);
                    YAxis.MoveAbs(_currentRecipe.MoldRecipe.YAxisReadyPos);
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.XYAxis_SafetyPos_MoveWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.IsOnPosition(_currentRecipe.MoldRecipe.XAxisReadyPos))
                            RaiseWarning(EWarning.XAxis_ReadyPos_MoveTimeOut);
                        if (!YAxis.IsOnPosition(_currentRecipe.MoldRecipe.YAxisReadyPos))
                            RaiseWarning(EWarning.YAxis_ReadyPos_MoveTimeOut);
                        break;
                    }

                    Log.Debug($"XAxis/YAxis move to ready position done");
                    Step.ToRunStep++;
                    break;
                case EMoldProcToRunStep.End:
                    Log.Debug("ToRun end");
                    Step.ToRunStep++;

                    base.ProcessToRun();
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
                    Sequence_AutoRun();
                    break;
                case ESequence.Loading:
                    Sequence_Loading();
                    break;
                case ESequence.ResinInject:
                    Sequence_ResinInject();
                    break;
                case ESequence.Unloading:
                    Sequence_Unloading();
                    break;
                case ESequence.DummyShot:
                    Sequence_DummyShot();
                    break;
                case ESequence.NeedleCleaning:
                    Sequence_NeedleCleaning();
                    break;
                case ESequence.DotWeighting:
                    Sequence_DotWeighting();
                    break;
                case ESequence.HeadAssemble:
                    Sequence_HeadAssemble();
                    break;
                case ESequence.HeadDisassemble:
                    Sequence_HeadDisassemble();
                    break;
                case ESequence.BubbleRemove:
                    Sequence_BubbleRemove();
                    break;
            }

            return true;
        }

        public override bool ProcessOrigin()
        {
            switch ((EMoldProcOriginStep)Step.OriginStep)
            {
                case EMoldProcOriginStep.Start:
                    Log.Debug("Origin start");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.CheckIfChamberOpen:
                    if (ChamberOpenClose.IsForward)
                    {
                        RaiseWarning(EWarning.Mold_Chamber_OpenWarning);
                        return false;
                    }

                    Log.Debug("Chamber is closed");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_Origin:
                    Log.Debug("Searching origin Z Axes");
                    ZAxisSearchOrigin();

                    Wait(_currentRecipe.CommonRecipe.MotionOriginTimeout, () => AllZAxisOriginCompleted());
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.ZAxis_OriginWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!Z1Axis.Status.IsHomeDone) RaiseWarning(EWarning.Z1Axis_Origin_TimeOut);
                        if (!Z2Axis.Status.IsHomeDone) RaiseWarning(EWarning.Z2Axis_Origin_TimeOut);
                        if (!Z3Axis.Status.IsHomeDone) RaiseWarning(EWarning.Z3Axis_Origin_TimeOut);
                        if (!Z4Axis.Status.IsHomeDone) RaiseWarning(EWarning.Z4Axis_Origin_TimeOut);
                        break;
                    }

                    Log.Debug("Z Axes origin search done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.Bellow_Down:
                    if (BellowUpDown.IsBackward)
                    {
                        Step.OriginStep = (int)EMoldProcOriginStep.XYAxis_Origin;
                        break;
                    }

                    Log.Debug($"{BellowUpDown} moving down");
                    BellowUpDown.Backward();
                    Wait(_currentRecipe.CommonRecipe.CylinderMoveTimeout,
                        () => BellowUpDown.IsBackward);
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.Bellow_DownWait:
                    if (WaitTimeOutOccurred)
                    {
                        RaiseWarning(EWarning.BellowUpDown_DownFail);
                        break;
                    }

                    Log.Debug($"Move {BellowUpDown} down done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_Origin:
                    XAxis.SearchOrigin();
                    YAxis.SearchOrigin();

                    Log.Debug($"Searching origin {XAxis.Name} {YAxis.Name}");
                    Wait(_currentRecipe.CommonRecipe.MotionOriginTimeout,
                        () => XAxis.Status.IsHomeDone && YAxis.Status.IsHomeDone);
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.XYAxis_OriginWait:
                    if (WaitTimeOutOccurred)
                    {
                        if (!XAxis.Status.IsHomeDone) RaiseWarning(EWarning.XAxis_Origin_TimeOut);
                        if (!YAxis.Status.IsHomeDone) RaiseWarning(EWarning.YAxis_Origin_TimeOut);
                        break;
                    }

                    Log.Debug($"{XAxis.Name} {YAxis.Name} origin search done");
                    Step.OriginStep++;
                    break;
                case EMoldProcOriginStep.End:
                    Log.Debug("Origin end");
                    Step.OriginStep++;
                    base.ProcessOrigin();
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

        private void Sequence_BubbleRemove()
        {
        }

        private void Sequence_HeadDisassemble()
        {
        }

        private void Sequence_HeadAssemble()
        {
        }

        private void Sequence_DotWeighting()
        {
        }

        private void Sequence_NeedleCleaning()
        {
        }

        private void Sequence_DummyShot()
        {
        }

        private void Sequence_Unloading()
        {
        }

        private void Sequence_ResinInject()
        {
        }

        private void Sequence_Loading()
        {
        }
        #endregion

        #region Private Methods
        private void ZAxisSearchOrigin()
        {
            Z1Axis.SearchOrigin();
            Z2Axis.SearchOrigin();
            Z3Axis.SearchOrigin();
            Z4Axis.SearchOrigin();
        }

        private bool AllZAxisOriginCompleted()
        {
            return Z1Axis.Status.IsHomeDone
                && Z2Axis.Status.IsHomeDone
                && Z3Axis.Status.IsHomeDone
                && Z4Axis.Status.IsHomeDone;
        }

        private void ZAxisSafetyPosMove()
        {
            Z1Axis.MoveAbs(_currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos);
            Z2Axis.MoveAbs(_currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos);
            Z3Axis.MoveAbs(_currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos);
            Z4Axis.MoveAbs(_currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos);
        }

        private bool AllZAxisInSafetyPos()
        {
            return
                Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos &&
                Z2Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos &&
                Z3Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos &&
                Z4Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos;
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly ProcessIO _processIO;
        private readonly RecipeSelector _recipeSelector;
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        #endregion
    }
}
