using EQX.Core.Recipe;

namespace SDV_MoldingInjection.Recipe
{
    public class OptionRecipe : RecipeBase
    {
        #region Privates
        public event Action<bool> Head12SkipChanged;
        public event Action<bool> Head34SkipChanged;
        public event Action<bool> Head1SkipChanged;
        public event Action<bool> Head2SkipChanged;
        public event Action<bool> Head3SkipChanged;
        public event Action<bool> Head4SkipChanged;
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
        private bool _skipHead1;
        private bool _skipHead2;
        private bool _skipHead3;
        private bool _skipHead4;
        private bool _dummyBeforeInject;
        private bool _skipDryPumpPressure;

        #endregion

        [SingleRecipeDescription(Description = "Skip Head 1 & 2", Detail = "Check to Skip Head 1 & Head 2")]
        [ParameterDescription(204)]
        public bool SkipHead12
        {
            get => _skipHead12;
            set
            {
                if (!SetRecipe(ref _skipHead12, value))
                    return;
                _skipHead1 = value;
                _skipHead2 = value;
                Head1SkipChanged?.Invoke(value);
                Head2SkipChanged?.Invoke(value);
                OnPropertyChanged(nameof(SkipHead1));
                OnPropertyChanged(nameof(SkipHead2));
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 3 & 4", Detail = "Check to Skip Head 3 & Head 4")]
        [ParameterDescription(205)]
        public bool SkipHead34
        {
            get => _skipHead34;
            set
            {
                if (!SetRecipe(ref _skipHead34, value))
                    return;
                _skipHead3 = value;
                _skipHead4 = value;
                Head3SkipChanged?.Invoke(value);
                Head4SkipChanged?.Invoke(value);
                OnPropertyChanged(nameof(SkipHead3));
                OnPropertyChanged(nameof(SkipHead4));
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 1", Detail = "Check to Skip Head 1")]
        [ParameterDescription(206)]
        public bool SkipHead1
        {
            get => _skipHead1;
            set
            {
                if (value == false)
                {
                    _skipHead12 = value;
                    OnPropertyChanged(nameof(SkipHead12));
                }
                SetRecipe(ref _skipHead1, value);
                Head1SkipChanged?.Invoke(value);
            }
        }
        [SingleRecipeDescription(Description = "Skip Head 2", Detail = "Check to Skip Head 2")]
        [ParameterDescription(207)]
        public bool SkipHead2
        {
            get => _skipHead2;
            set
            {
                if (value == false)
                {
                    _skipHead12 = value;
                    OnPropertyChanged(nameof(SkipHead12));
                }
                SetRecipe(ref _skipHead2, value);
                Head2SkipChanged?.Invoke(value);
            }
        }
        [SingleRecipeDescription(Description = "Skip Head 3", Detail = "Check to Skip Head 3")]
        [ParameterDescription(208)]
        public bool SkipHead3
        {
            get => _skipHead3;
            set
            {
                if (value == false)
                {
                    _skipHead34 = value;
                    OnPropertyChanged(nameof(SkipHead34));
                }
                SetRecipe(ref _skipHead3, value);
                Head3SkipChanged?.Invoke(value);
            }
        }
        [SingleRecipeDescription(Description = "Skip Head 4", Detail = "Check to Skip Head 4")]
        [ParameterDescription(209)]
        public bool SkipHead4
        {
            get => _skipHead4;
            set
            {
                if (value == false)
                {
                    _skipHead34 = value;
                    OnPropertyChanged(nameof(SkipHead34));
                }
                SetRecipe(ref _skipHead4, value);
                Head4SkipChanged?.Invoke(value);
            }
        }

        [ParameterDescription(210)]
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

        [ParameterDescription(211)]
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

        [ParameterDescription(212)]
        public bool SavePressureLog
        {
            get => _savePressureLog;
            set => SetRecipe(ref _savePressureLog, value);
        }

        [ParameterDescription(213)]
        public bool OneTorrAndPurge
        {
            get => _1torrAndPurge;
            set => SetRecipe(ref _1torrAndPurge, value);
        }

        [ParameterDescription(214)]
        public bool SkipVentTime
        {
            get => _skipVentTime;
            set => SetRecipe(ref _skipVentTime, value);
        }

        [ParameterDescription(215)]
        public bool DelayAfterInjectFinish
        {
            get => _delayAfterInjectFinish;
            set => SetRecipe(ref _delayAfterInjectFinish, value);
        }

        [ParameterDescription(216)]
        public bool EnableInjectionPath
        {
            get => _enableInjectionPath;
            set => SetRecipe(ref _enableInjectionPath, value);
        }

        [ParameterDescription(217)]
        public bool InjectAfterOpenAngleValve
        {
            get => _injectAfterOpenAngleValve;
            set => SetRecipe(ref _injectAfterOpenAngleValve, value);
        }

        [ParameterDescription(217)]
        public bool SkipDummyShot
        {
            get => _skipDummyShot;
            set => SetRecipe(ref _skipDummyShot, value);
        }

        [ParameterDescription(219)]
        public bool SkipNeedleClean
        {
            get => _skipNeedleClean;
            set => SetRecipe(ref _skipNeedleClean, value);
        }

        [ParameterDescription(220)]
        public bool SkipDispensing
        {
            get => _skipDispensing;
            set => SetRecipe(ref _skipDispensing, value);
        }

        [ParameterDescription(221)]
        public bool UseCIM
        {
            get { return _useCIM; }
            set { _useCIM = value; }
        }
        [ParameterDescription(222)]
        public bool DummyBeforeInject
        {
            get { return _dummyBeforeInject; }
            set => SetRecipe(ref _dummyBeforeInject, value);
        }

        [ParameterDescription(223)]
        public bool SkipDryPumpPressure
        {
            get { return _skipDryPumpPressure; }
            set => SetRecipe(ref _skipDryPumpPressure, value);
        }
    }
}
