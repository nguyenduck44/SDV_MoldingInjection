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
            return true;
        }

        public override bool ProcessRun()
        {
            switch (Sequence)
            {
                case ESequence.Stop:
                    break;
                case ESequence.AutoRun:
                    break;
                case ESequence.Ready:
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

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        #endregion
    }
}
