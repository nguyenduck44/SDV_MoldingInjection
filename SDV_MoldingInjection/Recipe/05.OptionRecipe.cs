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

        #endregion

        [SingleRecipeDescription(Description = "Skip Head 1 & 2", Detail = "Check to Skip Head 1 & Head 2")]
        public bool SkipHead12
        {
            get { return _skipHead12; }
            set
            {
                if (_skipHead12 == value) return;
                OnRecipeChanged(_skipHead12, value);
                Head12SkipChanged?.Invoke(value);
                _skipHead12 = value;
                OnPropertyChanged();
            }
        }

        [SingleRecipeDescription(Description = "Skip Head 3 & 4", Detail = "Check to Skip Head 3 & Head 4")]
        public bool SkipHead34
        {
            get { return _skipHead34; }
            set
            {
                if (_skipHead34 == value) return;
                OnRecipeChanged(_skipHead34, value);
                Head34SkipChanged?.Invoke(value);
                _skipHead34 = value;
                OnPropertyChanged();
            }
        }

        public bool InputTypeManual
        {
            get { return _inputTypeManual; }
            set
            {
                if (_inputTypeManual != value)
                {
                    OnRecipeChanged(_inputTypeManual, value);
                    _inputTypeManual = value;
                    OnPropertyChanged();

                    if (value == true && InputTypeAuto == true)
                    {
                        _inputTypeAuto = false;
                        OnPropertyChanged(nameof(InputTypeAuto));
                    }
                }
            }
        }

        public bool InputTypeAuto
        {
            get { return _inputTypeAuto; }
            set
            {
                if (_inputTypeAuto != value)
                {
                    OnRecipeChanged(_inputTypeAuto, value);
                    _inputTypeAuto = value;
                    OnPropertyChanged();

                    if (value == true && InputTypeManual == true)
                    {
                        _inputTypeManual = false;
                        OnPropertyChanged(nameof(InputTypeManual));
                    }
                }
            }
        }

        public bool SavePressureLog
        {
            get { return _savePressureLog; }
            set
            {
                if (_savePressureLog != value)
                {
                    OnRecipeChanged(_savePressureLog, value);
                    _savePressureLog = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool OneTorrAndPurge
        {
            get { return _1torrAndPurge; }
            set
            {
                if (_1torrAndPurge != value)
                {
                    OnRecipeChanged(_1torrAndPurge, value);
                    _1torrAndPurge = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
