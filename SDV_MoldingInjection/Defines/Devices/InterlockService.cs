using EQX.Core.Interlock;
using EQX.Core.Motion;
using EQX.InOut;
using EQX.UI.Controls;
using log4net;
using SDV_MoldingInjection.Recipe;
using System.CodeDom;
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
                { "YAxis not in Ready Pos", () => _devices.Motions.StageYAxis.Status.ActualPosition <= _currentRecipe.InjectRecipe.YAxisReadyPos + 5 },
            };
        }

        private void OutputInterlock(bool disable)
        {
            if (disable) _devices.Outputs.VacChamberOpen.OutputEnableInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Outputs.VacChamberOpen.OutputEnableInterlocks = new Dictionary<string, Func<bool>>
            {
                { "YAxis not in Ready Pos", () => _devices.Motions.StageYAxis.Status.ActualPosition <= _currentRecipe.InjectRecipe.YAxisReadyPos + 5},
            };
        }

        private void MotionInterlock(bool disable)
        {
            if (disable) _devices.Motions.StageYAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.StageYAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
                { "Chamber is not CLOSE", () => _devices.Cylinders.ChamberOpenClose.IsClose() },
                { "Bellow cylinder is not DOWN", () => _devices.Cylinders.BellowCyl.IsDown() },
                { "Z1Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z1Axis) },
                { "Z2Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z2Axis) },
                { "Z3Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z3Axis) },
                { "Z4Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z4Axis) },
            };

            if (disable) _devices.Motions.StageYAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.StageYAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
                { "Chamber is not CLOSE", () => _devices.Cylinders.ChamberOpenClose.IsClose() },
                { "Bellow cylinder is not DOWN", () => _devices.Cylinders.BellowCyl.IsDown() },
                { "Z1Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z1Axis) },
                { "Z2Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z2Axis) },
                { "Z3Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z3Axis) },
                { "Z4Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z4Axis) },
            };

            if (disable) _devices.Motions.XAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.XAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
                { "Z1Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z1Axis) },
                { "Z2Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z2Axis) },
                { "Z3Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z3Axis) },
                { "Z4Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z4Axis) },
            };

            if (disable) _devices.Motions.XAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.XAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
                { "Z1Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z1Axis) },
                { "Z2Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z2Axis) },
                { "Z3Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z3Axis) },
                { "Z4Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z4Axis) },
            };

            if (disable) _devices.Motions.Z1Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.Z1Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
            };
            if (disable) _devices.Motions.Z1Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.Z1Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
            };

            if (disable) _devices.Motions.Z2Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.Z2Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
            };
            if (disable) _devices.Motions.Z2Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.Z2Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
            };

            if (disable) _devices.Motions.Z3Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.Z3Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
            };
            if (disable) _devices.Motions.Z3Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.Z3Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
            };

            if (disable) _devices.Motions.Z4Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.Z4Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
            };
            if (disable) _devices.Motions.Z4Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            _devices.Motions.Z4Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
            {
                { "Door is not CLOSE", () => _devices.Inputs.DoorClose },
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
            string message = $"{e.Obj.Name} blocked by '{e.Message}' while '{e.Action}'";
            if (_machineStatus.IsStandByProcessMode)
            {
                MessageBoxEx.Show(message, false, "WARNING");
            }
            else
            {
                MessageBoxEx.Show(message, false);
                LogManager.GetLogger("Interlock").Error(message);
            }
        }
        
        private bool IsZAxisOnSafetyPosition(IMotion zAxis)
        {
            if (zAxis == _devices.Motions.Z1Axis) return _devices.Motions.Z1Axis.IsOnPosition(_currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos) || _devices.Motions.Z1Axis.Status.ActualPosition >= _currentRecipe.SPDHead1_Recipe.ZAxisSafetyPos;
            if (zAxis == _devices.Motions.Z2Axis) return _devices.Motions.Z2Axis.IsOnPosition(_currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos) || _devices.Motions.Z2Axis.Status.ActualPosition >= _currentRecipe.SPDHead2_Recipe.ZAxisSafetyPos;
            if (zAxis == _devices.Motions.Z3Axis) return _devices.Motions.Z3Axis.IsOnPosition(_currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos) || _devices.Motions.Z3Axis.Status.ActualPosition >= _currentRecipe.SPDHead3_Recipe.ZAxisSafetyPos;
            if (zAxis == _devices.Motions.Z4Axis) return _devices.Motions.Z4Axis.IsOnPosition(_currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos) || _devices.Motions.Z4Axis.Status.ActualPosition >= _currentRecipe.SPDHead4_Recipe.ZAxisSafetyPos;

            throw new Exception($"{zAxis.Name} is not supported ZAxis");
        }
        #endregion

        #region Privates
        private readonly Devices _devices;
        private readonly RecipeSelector _recipeSelector;
        private readonly MachineStatus _machineStatus;
        #endregion
    }
}
