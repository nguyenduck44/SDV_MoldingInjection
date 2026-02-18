using EQX.Core.InOut;
using EQX.Core.Motion;
using SDV_MoldingInjection.Defines;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Process
{
    public class DryPumpProcess : MIProcess
    {
        #region Motions
        #endregion

        #region Cylinders
        private ICylinder AngleValve => _devices.Cylinders.AngleValve;
        #endregion

        #region Constructors
        public DryPumpProcess(Devices devices, RecipeSelector recipeSelector)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
        }
        #endregion

        #region Process Methods
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
                    break;
                case ESequence.AutoRun:
                    Sequence_AutoRun();
                    break;
                case ESequence.ResinInject:
                    break;
            }
            return true;
        }
        #endregion

        #region Sequence Methods
        private void Sequence_AutoRun()
        {

        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        #endregion
    }
}
