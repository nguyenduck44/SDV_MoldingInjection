using EQX.Core.Recipe;
using EQX.Core.Units;

namespace SDV_MoldingInjection.Recipe
{
    public class IdlePurgeRecipe : RecipeBase
    {
        #region Privates
        private bool enableIdlePurge;
        private double idlePurgeAfterStopTime;
        private double idlePurgeCycleTime;
        private double _idlePurgeWeight;
        #endregion

        [SingleRecipeDescription(Description = "IdlePurge Weight", Unit = Unit.mg)]
        [SingleRecipeMinMax(Max = 380, Min = 0)]
        public double IdlePurgeWeight
        {
            get => _idlePurgeWeight;
            set
            {
                if (_idlePurgeWeight == value) return;
                SetRecipe(ref _idlePurgeWeight, value, nameof(IdlePurgeWeight));
            }
        }
        [SingleRecipeDescription(
            Description = "Enable Idle Purge",
            Detail = "Check to Enable Idle Purge")]
        public bool EnableIdlePurge
        {
            get { return enableIdlePurge; }
            set
            {
                if (enableIdlePurge == value) return;
                SetRecipe(ref enableIdlePurge, value);
            }
        }

        [SingleRecipeDescription(Description = "Idle Purge After Stop Time", Unit = Unit.Minute)]
        public double IdlePurgeAfterStopTime
        {
            get { return idlePurgeAfterStopTime; }
            set
            {
                if (idlePurgeAfterStopTime == value) return;
                SetRecipe(ref idlePurgeAfterStopTime, value);
            }
        }
    }
}
