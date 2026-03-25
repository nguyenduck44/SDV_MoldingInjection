using EQX.Core.Recipe;
using EQX.Core.Units;
using EQX.UI.Language;

namespace SDV_MoldingInjection.Recipe
{
    public class CommonRecipe : RecipeBase
    {
        private double cylinderMoveTimeout;
        private double motionOriginTimeout;
        private double motionMoveTimeout;
        private double vacDelay;
        private int logSaveDay = 30;
        public event Action<ILanguageDefinition> SelectedLanguageEvent;
        public event Action SelectedLanguageLoadAllRecipe;

        [SingleRecipeDescription(Description = "Cylinder Move Timeout", Unit = Unit.Second)]
        [CIMParameterAddress((int)ECIMParamter.CylinderMoveTimeout)]
        public double CylinderMoveTimeout
        {
            get => cylinderMoveTimeout;
            set => SetRecipe(ref cylinderMoveTimeout, value, nameof(CylinderMoveTimeout));
        }

        [SingleRecipeDescription(Description = "Motion Origin Timeout", Unit = Unit.Second)]
        public double MotionOriginTimeout
        {
            get => motionOriginTimeout;
            set => SetRecipe(ref motionOriginTimeout, value, nameof(MotionOriginTimeout));
        }

        [SingleRecipeDescription(Description = "Motion move timeout", Unit = Unit.Second)]
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

        private double materialInputTimeout = 1800.0;

        [SingleRecipeDescription(Description = "Material Input Timeout",
            Detail = "Time Machine Not Have Material To Stop", Unit = Unit.Second)]
        public double MaterialInputTimeout
        {
            get => materialInputTimeout;
            set => SetRecipe(ref materialInputTimeout, value, nameof(MaterialInputTimeout));
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
        [SingleRecipeMinMax(Max = 100, Min = 5)]
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

        #region Privates
        private double _syringeAmountTimeChange;
        private double _syringeAmountWeight;
        private ILanguageDefinition selectLanguage;
        #endregion
    }
}
