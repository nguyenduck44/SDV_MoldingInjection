using EQX.Core.Interlock;
using EQX.InOut;
using EQX.UI.Controls;
using SDV_MoldingInjection.Recipe;
using System.Windows;

namespace SDV_MoldingInjection.Defines.Devices
{
    public class InterlockService : IDisposable
    {
        #region Properties

        private RecipeList _currentRecipe => _recipeSelector.CurrentRecipe;
        #endregion

        #region Constructor(s)
        public InterlockService(Devices devices,
            RecipeSelector recipeSelector,
            MachineStatus machineStatus)
        {
            _devices = devices;
            _recipeSelector = recipeSelector;
            _machineStatus = machineStatus;

            InterlockMonitor.OnInterlockBlocked += HandleInterlockBlocked;
        }
        #endregion

        #region Public Methods
        public void Config(bool disable = false)
        {
            MotionInterlock(disable);
            OutputInterlock(disable);
            CylinderInterlock(disable);
        }

        private void CylinderInterlock(bool disable)
        {
            if (disable) _devices.Cylinders.ChamberOpenClose.ForwardInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Cylinders.ChamberOpenClose.ForwardInterlocks = new Dictionary<string, Func<bool>>
            {
                { "YAxis not in Ready Pos", () => _devices.Motions.StageYAxis.Status.ActualPosition <= _currentRecipe.InjectRecipe.YAxisReadyPos },
            };
        }

        private void OutputInterlock(bool disable)
        {
            if (disable) _devices.Outputs.VacChamberOpen.OutputEnableInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Outputs.VacChamberOpen.OutputEnableInterlocks = new Dictionary<string, Func<bool>>
            {
                { "YAxis not in Ready Pos", () => _devices.Motions.StageYAxis.Status.ActualPosition <= _currentRecipe.InjectRecipe.YAxisReadyPos },
            };
        }

        private void MotionInterlock(bool disable)
        {
            if (disable) _devices.Motions.StageYAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.StageYAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Chamber is not CLOSE", () => _devices.Cylinders.ChamberOpenClose.IsClose() },
                { "Chamber is not DOWN", () => _devices.Cylinders.BellowCyl.IsDown() },
                { "Z1Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos },
                { "Z2Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos },
                { "Z3Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos },
                { "Z4Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos },
            };

            if (disable) _devices.Motions.StageYAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.StageYAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Chamber is not CLOSE", () => _devices.Cylinders.ChamberOpenClose.IsClose() },
                { "Bellow is not DOWN", () => _devices.Cylinders.BellowCyl.IsDown() },
                { "Z1Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos },
                { "Z2Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos },
                { "Z3Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos },
                { "Z4Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos },
            };

            if (disable) _devices.Motions.XAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.XAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Z1Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos },
                { "Z2Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos },
                { "Z3Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos },
                { "Z4Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos },
            };

            if (disable) _devices.Motions.XAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.XAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Z1Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos },
                { "Z2Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos },
                { "Z3Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos },
                { "Z4Axis not in Safety Pos", () => _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos },
            };
        }

        public void Dispose()
        {
            InterlockMonitor.OnInterlockBlocked -= HandleInterlockBlocked;
        }
        #endregion

        #region Private Methods
        private void HandleInterlockBlocked(object? sender, InterlockEventAgrs e)
        {
            if (_machineStatus.IsStandByProcessMode)
            {
                string message = $"{e.Obj.Name} blocked by '{e.Message}' while '{e.Action}'";
                MessageBoxEx.Show(message, false, "WARNING");
            }
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private readonly MachineStatus _machineStatus;
        #endregion
    }
}
