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

        [SingleRecipeDescription(Description = "Resin Weight", Unit = Unit.mg)]
        public double ResinWeight
        {
            get { return _resinWeight; }
            set
            {
                if (_resinWeight == value) return;
                OnRecipeChanged(_resinWeight, value);
                _resinWeight = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Resin Weight Spec", Unit = Unit.mg)]
        public double ResinWeightSpec
        {
            get { return _resinWeightSpec; }
            set
            {
                if (_resinWeightSpec == value) return;
                OnRecipeChanged(_resinWeightSpec, value);
                _resinWeightSpec = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Inject Time", Unit = Unit.Second)]
        public double InjectTime
        {
            get { return _injectTime; }
            set
            {
                if (_injectTime == value) return;
                OnRecipeChanged(_injectTime, value);
                _injectTime = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Bubble remove turn")]
        public double BubbleRemoveTurn
        {
            get { return _bubbleRemoveTurn; }
            set
            {
                if (_bubbleRemoveTurn == value) return;

                OnRecipeChanged(_bubbleRemoveTurn, value);
                _bubbleRemoveTurn = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Cylinder Move Timeout", Unit = Unit.Second)]
        public double CylinderMoveTimeout
        {
            get { return cylinderMoveTimeout; }
            set
            {
                if (cylinderMoveTimeout == value) return;

                OnRecipeChanged(cylinderMoveTimeout, value);
                cylinderMoveTimeout = value;
            }
        }

        [SingleRecipeDescription(Description = "Motion Origin Timeout", Unit = Unit.Second)]
        public double MotionOriginTimeout
        {
            get { return motionOriginTimeout; }
            set
            {
                if (motionOriginTimeout == value) return;

                OnRecipeChanged(motionOriginTimeout, value);
                motionOriginTimeout = value;
            }
        }

        [SingleRecipeDescription(Description = "Motion move timeout", Unit = Unit.Second)]
        public double MotionMoveTimeout
        {
            get { return motionMoveTimeout; }
            set
            {
                if (motionMoveTimeout == value) return;

                OnRecipeChanged(motionMoveTimeout, value);
                motionMoveTimeout = value;
            }
        }

        [SingleRecipeDescription(Description = "Vacuum Delay", Unit = Unit.Second)]
        public double VacDelay
        {
            get { return vacDelay; }
            set
            {
                if (vacDelay == value) return;

                OnRecipeChanged(vacDelay, value);
                vacDelay = value;
            }
        }

        private double materialInputTimeout = 1800.0;

        [SingleRecipeDescription(Description = "Material Input Timeout",
            Detail = "Time Machine Not Have Material To Stop", Unit = Unit.Second)]
        public double MaterialInputTimeout
        {
            get { return materialInputTimeout; }
            set
            {
                OnRecipeChanged(materialInputTimeout, value);
                materialInputTimeout = value;
            }
        }

        [SingleRecipeDescription(Description = "Syringe Mount Time Change", Unit = Unit.Hour)]
        public double SyringeMountTimeChange
        {
            get { return _syringeAmountTimeChange; }
            set
            {
                OnRecipeChanged(_syringeAmountTimeChange, value);
                _syringeAmountTimeChange = value;
            }
        }

        [SingleRecipeDescription(Description = "Syringe Amount Weight", Unit = Unit.Gram)]
        public double SyringeAmountWeight
        {
            get { return _syringeAmountWeight; }
            set
            {
                if (_syringeAmountWeight == value) return;

                OnRecipeChanged(_syringeAmountWeight, value);
                _syringeAmountWeight = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Log Save Day")]
        [SingleRecipeMinMax(Max = 100, Min = 5)]
        public int LogSaveDay
        {
            get { return logSaveDay; }
            set
            {
                OnRecipeChanged(logSaveDay, value);
                logSaveDay = value;
            }
        }

        public ILanguageDefinition SelectedLanguage
        {
            get => selectLanguage;
            set
            {
                if (selectLanguage == value) return;
                OnRecipeChanged(selectLanguage, value);
                selectLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
                SelectedLanguageEvent?.Invoke(SelectedLanguage);
                SelectedLanguageLoadAllRecipe?.Invoke();
            }
        }

        #region Privates

        private double _resinWeight;
        private double _resinWeightSpec;
        private double _injectTime;
        private double _bubbleRemoveTurn;
        private double _syringeAmountTimeChange;
        private double _syringeAmountWeight;
        private ILanguageDefinition selectLanguage;
        #endregion
    }
}
