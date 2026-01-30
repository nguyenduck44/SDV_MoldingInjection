using EQX.TemplateProject.Defines.Devices;
using EQX.TemplateProject.MVVM.ViewModels.Teaching;
using EQX.TemplateProject.Recipe;

namespace EQX.TemplateProject.Services.Factories
{
    public class TeachingViewModelFactory
    {
        public TeachingViewModelFactory(RecipeSelector recipeSelector, Devices devices)
        {
            _recipeSelector = recipeSelector;
            _devices = devices;

            _unitTeachingVMs = new Dictionary<string, UnitTeachingViewModel>();
        }

        ~TeachingViewModelFactory()
        {
            _unitTeachingVMs.Clear();
        }

        public UnitTeachingViewModel Create(string name)
        {
            if (_unitTeachingVMs.ContainsKey(name) == false)
            {
                _unitTeachingVMs.Add(name, CreateVM(name));
            }

            return _unitTeachingVMs[name];
        }

        #region Private Methods
        private UnitTeachingViewModel CreateVM(string name)
        {
            switch(name)
            {
                case "CST Load":
                    return new UnitTeachingViewModel("CST Load", _recipeSelector);
                case "CST Unload":
                    return new UnitTeachingViewModel("CST Unload", _recipeSelector);
                default:
                    return new UnitTeachingViewModel("INVALID", _recipeSelector);
            }
        }
        #endregion

        #region Private Fields
        private readonly RecipeSelector _recipeSelector;
        private readonly Devices _devices;

        private Dictionary<string, UnitTeachingViewModel> _unitTeachingVMs;
        #endregion
    }
}
