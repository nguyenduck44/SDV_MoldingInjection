using EQX.UI.Controls;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Recipe;

namespace SDV_MoldingInjection.Services.Interlock
{
    public class InterlockService
    {
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;

        public InterlockService(Devices devices, RecipeSelector recipeSelector)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
        }

        public bool InterlockCheck(string unitName, string positionDescription)
        {
            if (_devices.Inputs.DoorClose == false)
            {
                MessageBoxEx.ShowDialog($"Close the Door before Move to {positionDescription}");
                return false;
            }
            return true;
        }
    }
}
