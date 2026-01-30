using EQX.Core.Motion;
using EQX.TemplateProject.Defines;
using EQX.TemplateProject.Defines.Devices;
using EQX.TemplateProject.MVVM.ViewModels.Manual;
using EQX.TemplateProject.Recipe;
using System.Collections.ObjectModel;
using System.Windows;

namespace EQX.TemplateProject.Services.Factories
{
    public class ManualViewModelFactory
    {
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;

        private RecipeList CurrentRecipe => _recipeSelector.CurrentRecipe;

        public ManualViewModelFactory(Devices devices,RecipeSelector recipeSelector)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            _manualVMs = new Dictionary<string, ManualUnitViewModel>();
        }

        ~ManualViewModelFactory()
        {
            _manualVMs.Clear();
        }

        public ManualUnitViewModel Create(string name)
        {
            if (_manualVMs.ContainsKey(name) == false)
            {
                _manualVMs[name] = CreateVM(name);
            }

            return _manualVMs[name];
        }

        private ManualUnitViewModel CreateVM(string name)
        {
            return new ManualUnitViewModel(name);
        }

        #region Private Fields
        private Dictionary<string, ManualUnitViewModel> _manualVMs;
        #endregion
    }
}
