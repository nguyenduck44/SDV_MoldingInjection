using EQX.Core.Recipe;

namespace SDV_MoldingInjection.Recipe
{
    public class OptionRecipe : RecipeBase
    {
        #region Privates
        public event Action<bool> Head12SkipChanged;
        public event Action<bool> Head34SkipChanged;
        private bool _skipHead12;
        private bool _skipHead34;
        private bool _inputTypeManual;
        private bool _inputTypeAuto;
        private bool _savePressureLog;
        private bool _1torrAndPurge;
        private bool _skipVentTime;
        private bool _delayAfterInjectFinish;
        private bool _enableInjectionPath;
        private bool _injectAfterOpenAngleValve;

        #endregion

        [SingleRecipeDescription(Description = "Skip Head 1 & 2", Detail = "Check to Skip Head 1 & Head 2")]
        public bool SkipHead12
        {
            get => _skipHead12;
            set
            {
                if (!SetRecipe(ref _skipHead12, value, nameof(SkipHead12)))
                    return;
                Head12SkipChanged?.Invoke(value);
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 3 & 4", Detail = "Check to Skip Head 3 & Head 4")]
        public bool SkipHead34
        {
            get => _skipHead34;
            set
            {
                if (!SetRecipe(ref _skipHead34, value, nameof(SkipHead34)))
                    return;
                Head34SkipChanged?.Invoke(value);
            }
        }

        public bool InputTypeManual
        {
            get => _inputTypeManual;
            set
            {
                if (!SetRecipe(ref _inputTypeManual, value, nameof(InputTypeManual)))
                    return;

                if (value && _inputTypeAuto)
                {
                    _inputTypeAuto = false;
                    OnPropertyChanged(nameof(InputTypeAuto));
                }
            }
        }

        public bool InputTypeAuto
        {
            get => _inputTypeAuto;
            set
            {
                if (!SetRecipe(ref _inputTypeAuto, value, nameof(InputTypeAuto)))
                    return;

                if (value && _inputTypeManual)
                {
                    _inputTypeManual = false;
                    OnPropertyChanged(nameof(InputTypeManual));
                }
            }
        }

        public bool SavePressureLog
        {
            get => _savePressureLog;
            set => SetRecipe(ref _savePressureLog, value, nameof(SavePressureLog));
        }

        public bool OneTorrAndPurge
        {
            get => _1torrAndPurge;
            set => SetRecipe(ref _1torrAndPurge, value, nameof(OneTorrAndPurge));
        }

        public bool SkipVentTime
        {
            get => _skipVentTime;
            set => SetRecipe(ref _skipVentTime, value, nameof(SkipVentTime));
        }

        public bool DelayAfterInjectFinish
        {
            get => _delayAfterInjectFinish;
            set => SetRecipe(ref _delayAfterInjectFinish, value, nameof(DelayAfterInjectFinish));
        }

        public bool EnableInjectionPath
        {
            get => _enableInjectionPath;
            set => SetRecipe(ref _enableInjectionPath, value, nameof(EnableInjectionPath));
        }

        public bool InjectAfterOpenAngleValve
        {
            get => _injectAfterOpenAngleValve;
            set => SetRecipe(ref _injectAfterOpenAngleValve, value, nameof(InjectAfterOpenAngleValve));
        }
    }
}
