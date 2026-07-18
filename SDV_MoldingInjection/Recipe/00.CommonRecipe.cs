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
        private double materialWarningCount;
        private ILanguageDefinition selectLanguage;
        public event Action<ILanguageDefinition> SelectedLanguageEvent;
        public event Action SelectedLanguageLoadAllRecipe;
        #endregion

        #region Properties
        [SingleRecipeDescription(Description = "Motion Origin Timeout", Unit = Unit.Second)]
        [ParameterDescription(1)]
        public double MotionOriginTimeout
        {
            get => motionOriginTimeout;
            set
            {
                if (motionOriginTimeout == value) return;
                SetRecipe(ref motionOriginTimeout, value);
            }

        }

        [SingleRecipeDescription(Description = "Motion Move Timeout", Unit = Unit.Second)]
        [ParameterDescription(2)]
        public double MotionMoveTimeout
        {
            get => motionMoveTimeout;
            set 
            {
                if (motionMoveTimeout == value) return;
                SetRecipe(ref motionMoveTimeout, value);
            }
        }

        [SingleRecipeDescription(Description = "Vacuum Delay", Unit = Unit.Second)]
        [ParameterDescription(3)]
        public double VacDelay
        {
            get => vacDelay;
            set
            {
                if (vacDelay == value) return;
                SetRecipe(ref vacDelay, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Mount Time Change", Unit = Unit.Hour)]
        [ParameterDescription(4)]
        public double SyringeMountTimeChange
        {
            get => _syringeAmountTimeChange;
            set
            {
                if (_syringeAmountTimeChange == value) return;
                SetRecipe(ref _syringeAmountTimeChange, value);
            }
        }

        [SingleRecipeDescription(Description = "Syringe Amount Weight", Unit = Unit.Gram)]
        [ParameterDescription(5)]
        public double SyringeAmountWeight
        {
            get => _syringeAmountWeight;
            set
            {
                if (_syringeAmountWeight == value) return;
                SetRecipe(ref _syringeAmountWeight, value);
            }
        }

        [SingleRecipeDescription(Description = "Log Save Day")]
        [ParameterDescription(6)]
        [SingleRecipeMinMax(Max = 180, Min = 5)]
        public int LogSaveDay
        {
            get => logSaveDay;
            set => SetRecipe(ref logSaveDay, value);
        }

        [SingleRecipeDescription(Description = "Material Warning Count")]
        [ParameterDescription(7)]
        public double MaterialWarningCount
        {
            get { return materialWarningCount; }
            set
            {
                SetRecipe(ref materialWarningCount, value);
            }
        }

        public ILanguageDefinition SelectedLanguage
        {
            get => selectLanguage;
            set
            {
                SetRecipe(ref selectLanguage, value);
                SelectedLanguageEvent?.Invoke(SelectedLanguage);
                SelectedLanguageLoadAllRecipe?.Invoke();
            }
        }
        #endregion
    }
}
