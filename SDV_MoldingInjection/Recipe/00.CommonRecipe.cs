using EQX.Core.Recipe;
using EQX.Core.Units;
using EQX.UI.Language;

namespace SDV_MoldingInjection.Recipe
{
    public class CommonRecipe : RecipeBase
    {
        #region Privates
        private double _syringeAmountTimeChange;
        private double _syringeAmountWeight;
        private double motionOriginTimeout;
        private double motionMoveTimeout;
        private double vacDelay;
        private int logSaveDay = 30;
        private ILanguageDefinition selectLanguage;
        public event Action<ILanguageDefinition> SelectedLanguageEvent;
        public event Action SelectedLanguageLoadAllRecipe;
        #endregion

        #region Properties
        [SingleRecipeDescription(Description = "Motion Origin Timeout", Unit = Unit.Second)]
        public double MotionOriginTimeout
        {
            get => motionOriginTimeout;
            set => SetRecipe(ref motionOriginTimeout, value, nameof(MotionOriginTimeout));
        }

        [SingleRecipeDescription(Description = "Motion Move Timeout", Unit = Unit.Second)]
        public double MotionMoveTimeout
        {
            get => motionMoveTimeout;
            set => SetRecipe(ref motionMoveTimeout, value, nameof(MotionMoveTimeout));
        }

        [SingleRecipeDescription(Description = "Vacuum Delay", Unit = Unit.Second)]
        public double VacDelay
        {
            get => vacDelay;
            set => SetRecipe(ref vacDelay, value, nameof(VacDelay));
        }

        [SingleRecipeDescription(Description = "Syringe Mount Time Change", Unit = Unit.Hour)]
        public double SyringeMountTimeChange
        {
            get => _syringeAmountTimeChange;
            set => SetRecipe(ref _syringeAmountTimeChange, value, nameof(SyringeMountTimeChange));
        }

        [SingleRecipeDescription(Description = "Syringe Amount Weight", Unit = Unit.Gram)]
        public double SyringeAmountWeight
        {
            get => _syringeAmountWeight;
            set => SetRecipe(ref _syringeAmountWeight, value, nameof(SyringeAmountWeight));
        }

        [SingleRecipeDescription(Description = "Log Save Day")]
        [SingleRecipeMinMax(Max = 180, Min = 5)]
        public int LogSaveDay
        {
            get => logSaveDay;
            set => SetRecipe(ref logSaveDay, value, nameof(LogSaveDay));
        }

        public ILanguageDefinition SelectedLanguage
        {
            get => selectLanguage;
            set
            {
                SetRecipe(ref selectLanguage, value, nameof(SelectedLanguage));
                SelectedLanguageEvent?.Invoke(SelectedLanguage);
                SelectedLanguageLoadAllRecipe?.Invoke();
            }
        }
        #endregion
    }
}
