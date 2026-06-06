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
        private bool _skipDummyShot;
        private bool _skipNeedleClean;
        private bool _skipDispensing;
        private bool _useCIM;
        private bool _readBarcode;

        #endregion

        [SingleRecipeDescription(Description = "Skip Head 1 & 2", Detail = "Check to Skip Head 1 & Head 2")]
        [ParameterDescription(197)]
        public bool SkipHead12
        {
            get => _skipHead12;
            set
            {
                if (!SetRecipe(ref _skipHead12, value))
                    return;
                Head12SkipChanged?.Invoke(value);
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 3 & 4", Detail = "Check to Skip Head 3 & Head 4")]
        [ParameterDescription(198)]
        public bool SkipHead34
        {
            get => _skipHead34;
            set
            {
                if (!SetRecipe(ref _skipHead34, value))
                    return;
                Head34SkipChanged?.Invoke(value);
            }
        }

        [ParameterDescription(199)]
        public bool InputTypeManual
        {
            get => _inputTypeManual;
            set
            {
                if (!SetRecipe(ref _inputTypeManual, value))
                    return;

                if (value && _inputTypeAuto)
                {
                    _inputTypeAuto = false;
                    OnPropertyChanged(nameof(InputTypeAuto));
                }
            }
        }

        [ParameterDescription(200)]
        public bool InputTypeAuto
        {
            get => _inputTypeAuto;
            set
            {
                if (!SetRecipe(ref _inputTypeAuto, value))
                    return;

                if (value && _inputTypeManual)
                {
                    _inputTypeManual = false;
                    OnPropertyChanged(nameof(InputTypeManual));
                }
            }
        }

        [ParameterDescription(201)]
        public bool SavePressureLog
        {
            get => _savePressureLog;
            set => SetRecipe(ref _savePressureLog, value);
        }

        [ParameterDescription(202)]
        public bool OneTorrAndPurge
        {
            get => _1torrAndPurge;
            set => SetRecipe(ref _1torrAndPurge, value);
        }

        [ParameterDescription(203)]
        public bool SkipVentTime
        {
            get => _skipVentTime;
            set => SetRecipe(ref _skipVentTime, value);
        }

        [ParameterDescription(204)]
        public bool DelayAfterInjectFinish
        {
            get => _delayAfterInjectFinish;
            set => SetRecipe(ref _delayAfterInjectFinish, value);
        }

        [ParameterDescription(205)]
        public bool EnableInjectionPath
        {
            get => _enableInjectionPath;
            set => SetRecipe(ref _enableInjectionPath, value);
        }

        [ParameterDescription(206)]
        public bool InjectAfterOpenAngleValve
        {
            get => _injectAfterOpenAngleValve;
            set => SetRecipe(ref _injectAfterOpenAngleValve, value);
        }

        [ParameterDescription(207)]
        public bool SkipDummyShot
        {
            get => _skipDummyShot;
            set => SetRecipe(ref _skipDummyShot, value);
        }

        [ParameterDescription(208)]
        public bool SkipNeedleClean
        {
            get => _skipNeedleClean;
            set => SetRecipe(ref _skipNeedleClean, value);
        }

        [ParameterDescription(209)]
        public bool SkipDispensing
        {
            get => _skipDispensing;
            set => SetRecipe(ref _skipDispensing, value);
        }

        [ParameterDescription(227)]
        public bool UseCIM
        {
            get { return _useCIM; }
            set { _useCIM = value; }
        }
        [ParameterDescription(228)]
        public bool ReadBarcode
        {
            get { return _readBarcode; }
            set => SetRecipe(ref _readBarcode, value);
        }

    }
}
