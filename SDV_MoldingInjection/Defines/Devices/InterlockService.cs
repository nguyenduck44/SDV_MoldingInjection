using SDV_MoldingInjection.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines.Devices
{
    public class InterlockService
    {
        #region Properties

        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        #endregion

        #region Constructor(s)
        public InterlockService(Devices devices, RecipeSelector recipeSelector)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
        }
        #endregion

        public void Config()
        {
            CylinderInterlock();
        }

        private void CylinderInterlock()
        {
            _devices.Cylinders.ChamberOpenClose.ForwardInterlocks = new Dictionary<string, Func<bool>>
            {
                { "YAxis not in Ready Pos", () => _devices.Motions.StageYAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos) },
            };
            _devices.Outputs.VacChamberOpen.OutputEnableInterlocks = new Dictionary<string, Func<bool>>
            {
                { "YAxis not in Ready Pos", () => _devices.Motions.StageYAxis.IsOnPosition(_currentRecipe.InjectRecipe.YAxisReadyPos) },
            };
        }

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        #endregion
    }
}
