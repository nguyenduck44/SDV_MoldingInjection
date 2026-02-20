using EQX.InOut;
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
            MotionInterlock();
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

        private void MotionInterlock()
        {
            _devices.Motions.StageYAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Chamber is not CLOSE", () => _devices.Cylinders.ChamberOpenClose.IsClose() },
                { "Chamber is not DOWN", () => _devices.Cylinders.BellowCyl.IsDown() },
                { "Z1Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos },
                { "Z2Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos },
                { "Z3Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos },
                { "Z4Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos },
            };
            _devices.Motions.StageYAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Chamber is not CLOSE", () => _devices.Cylinders.ChamberOpenClose.IsClose() },
                { "Bellow is not DOWN", () => _devices.Cylinders.BellowCyl.IsDown() },
                { "Z1Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos },
                { "Z2Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos },
                { "Z3Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos },
                { "Z4Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos },
            };
            _devices.Motions.XAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Z1Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos },
                { "Z2Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos },
                { "Z3Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos },
                { "Z4Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos },
            };
            _devices.Motions.XAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Z1Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos },
                { "Z2Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos },
                { "Z3Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos },
                { "Z4Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos },
            };
        }

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        #endregion
    }
}
