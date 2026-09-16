using EQX.Core.Interlock;
using EQX.Core.Motion;
using EQX.Core.Sequence;
using EQX.InOut;
using EQX.UI.Controls;
using log4net;
using Microsoft.Win32;
using SDV_MoldingInjection.Recipe;
using System.CodeDom;
using System.Windows;

namespace SDV_MoldingInjection.Defines
{
    public class InterlockService : IDisposable
    {
        #region Properties
        private double Z1_Axis_InjectPos_Interlock { get; }
        private double Z2_Axis_InjectPos_Interlock { get; }
        private double Z3_Axis_InjectPos_Interlock { get; }
        private double Z4_Axis_InjectPos_Interlock { get; }

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
#if !SIMULATION
            try
            {
                object? z1_Axis_InjectPos_Interlock = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Top Engineering", "Z1_Axis_InjectPos_Interlock", "-70");
                Z1_Axis_InjectPos_Interlock = double.Parse(z1_Axis_InjectPos_Interlock.ToString());

                object? z2_Axis_InjectPos_Interlock = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Top Engineering", "Z2_Axis_InjectPos_Interlock", "-70");
                Z2_Axis_InjectPos_Interlock = double.Parse(z2_Axis_InjectPos_Interlock.ToString());

                object? z3_Axis_InjectPos_Interlock = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Top Engineering", "Z3_Axis_InjectPos_Interlock", "-70");
                Z3_Axis_InjectPos_Interlock = double.Parse(z3_Axis_InjectPos_Interlock.ToString());

                object? z4_Axis_InjectPos_Interlock = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Top Engineering", "Z4_Axis_InjectPos_Interlock", "-70");
                Z4_Axis_InjectPos_Interlock = double.Parse(z4_Axis_InjectPos_Interlock.ToString());
            }
            catch
            {
                LogManager.GetLogger("Interlock").Error("Get Interlock Z Axis InjectPos. Check Registry Z_Axis_InjectPos_Interlock Value");
                MessageBoxEx.ShowDialog("Get Interlock Z Axis InjectPos. Check Registry Z_Axis_InjectPos_Interlock Value");
            }
#endif
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
            else
            {
                _devices.Cylinders.ChamberOpenClose.ForwardInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "YAxis not in Ready Pos", () => _devices.Motions.StageYAxis.Status.ActualPosition <= _currentRecipe.InjectRecipe.YAxisReadyPos + 5 },
                };
            }

        }

        private void OutputInterlock(bool disable)
        {
            if (disable) _devices.Outputs.VacChamberOpen.OutputEnableInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Outputs.VacChamberOpen.OutputEnableInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized()},
                    { "YAxis not in Ready Pos", () => _devices.Motions.StageYAxis.Status.ActualPosition <= _currentRecipe.InjectRecipe.YAxisReadyPos + 5},
                };
            }

        }

        private void MotionInterlock(bool disable)
        {
            if (disable == false)
            {
                foreach (var motion in _devices.Motions.All)
                {
                    motion.OriginInterlocks = new Dictionary<string, Func<bool>>()
                    {
                        { "Machine is Teach Mode", () => _devices.Inputs.AutoSW.Value && _devices.Inputs.TeachSW.Value == false},
                    };
                }
            }
            else
            {
                foreach (var motion in _devices.Motions.All)
                {
                    motion.OriginInterlocks = new Dictionary<string, Func<bool>>();
                }
            }

            if (disable) _devices.Motions.StageYAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.StageYAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized()},
                    { "Chamber is not CLOSE", () => _devices.Cylinders.ChamberOpenClose.IsClose() },
                    { "Bellow cylinder is not DOWN", () => _devices.Cylinders.BellowCyl.IsDown() },
                    { "Z1Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z1Axis) },
                    { "Z2Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z2Axis) },
                    { "Z3Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z3Axis) },
                    { "Z4Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z4Axis) },
                };
            }


            if (disable) _devices.Motions.StageYAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.StageYAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized()},
                    { "Chamber is not CLOSE", () => _devices.Cylinders.ChamberOpenClose.IsClose() },
                    { "Bellow cylinder is not DOWN", () => _devices.Cylinders.BellowCyl.IsDown() },
                    { "Z1Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z1Axis) },
                    { "Z2Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z2Axis) },
                    { "Z3Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z3Axis) },
                    { "Z4Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z4Axis) },
                };
            }

            if (disable) _devices.Motions.XAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.XAxis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized()},
                    { "Z1Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z1Axis) },
                    { "Z2Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z2Axis) },
                    { "Z3Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z3Axis) },
                    { "Z4Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z4Axis) },
                };
            }

            if (disable) _devices.Motions.XAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.XAxis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                    { "Z1Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z1Axis) },
                    { "Z2Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z2Axis) },
                    { "Z3Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z3Axis) },
                    { "Z4Axis not in Safety Pos", () => IsZAxisOnSafetyPosition(_devices.Motions.Z4Axis) },
                };
            }

            if (disable) _devices.Motions.Z1Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.Z1Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                };
            }

            if (disable) _devices.Motions.Z1Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.Z1Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                };
                _devices.Motions.Z1Axis.PositionAbsMoveInterlocks = new Dictionary<string, Func<double, bool>>
                {
                    { "Z1 Axis Inject Position OVER", pos => Z1Axis_InterlockAtInjectPosition(pos) },
                };
            }

            if (disable) _devices.Motions.Z2Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.Z2Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                };
            }

            if (disable) _devices.Motions.Z2Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.Z2Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                };
                _devices.Motions.Z2Axis.PositionAbsMoveInterlocks = new Dictionary<string, Func<double, bool>>
                {
                    { "Z2 Axis Inject Position OVER", pos => Z2Axis_InterlockAtInjectPosition(pos) },
                };
            }

            if (disable) _devices.Motions.Z3Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.Z3Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                };
            }

            if (disable) _devices.Motions.Z3Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.Z3Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                };
                _devices.Motions.Z3Axis.PositionAbsMoveInterlocks = new Dictionary<string, Func<double, bool>>
                {
                    { "Z3 Axis Inject Position OVER", pos => Z3Axis_InterlockAtInjectPosition(pos) },
                };
            }

            if (disable) _devices.Motions.Z4Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.Z4Axis.PositionIncreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                };
            }

            if (disable) _devices.Motions.Z4Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>();
            else
            {
                _devices.Motions.Z4Axis.PositionDecreaseInterlocks = new Dictionary<string, Func<bool>>
                {
                    { "Door is not CLOSE", () => DoorCloseOrTeachJogAuthorized() },
                };
                _devices.Motions.Z4Axis.PositionAbsMoveInterlocks = new Dictionary<string, Func<double, bool>>
                {
                    { "Z4 Axis Inject Position OVER", pos => Z4Axis_InterlockAtInjectPosition(pos) },
                };
            }
        }

        public void Dispose()
        {
            InterlockMonitor.OnInterlockBlocked -= HandleInterlockBlocked;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="position">Motion target position (berfore interlock checking position)</param>
        /// <returns></returns>
        private bool Z1Axis_InterlockAtInjectPosition(double position)
        {
            bool interlockBlockCondition =
                _devices.Motions.XAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.XAxisInjectPos) &
                _devices.Motions.StageYAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.YAxisInjectPos) &
                position < Z1_Axis_InjectPos_Interlock;

            return !interlockBlockCondition;
        }

        private bool Z2Axis_InterlockAtInjectPosition(double position)
        {
            bool interlockBlockCondition =
                _devices.Motions.XAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.XAxisInjectPos) &
                _devices.Motions.StageYAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.YAxisInjectPos) &
                position < Z2_Axis_InjectPos_Interlock;

            return !interlockBlockCondition;
        }

        private bool Z3Axis_InterlockAtInjectPosition(double position)
        {
            bool interlockBlockCondition =
                _devices.Motions.XAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.XAxisInjectPos) &
                _devices.Motions.StageYAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.YAxisInjectPos) &
                position < Z3_Axis_InjectPos_Interlock;

            return !interlockBlockCondition;
        }

        private bool Z4Axis_InterlockAtInjectPosition(double position)
        {
            bool interlockBlockCondition =
                _devices.Motions.XAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.XAxisInjectPos) &
                _devices.Motions.StageYAxis.IsOnPosition(_recipeSelector.CurrentRecipe.InjectRecipe.YAxisInjectPos) &
                position < Z4_Axis_InjectPos_Interlock;

            return !interlockBlockCondition;
        }

        private bool DoorCloseOrTeachJogAuthorized()
        {
            return _devices.Inputs.DoorClose || _machineStatus.CanJogWithDoorOpen;
        }

        private void HandleInterlockBlocked(object? sender, InterlockEventAgrs e)
        {
            string message = $"{e.Obj.Name} blocked by '{e.Message}' while '{e.Action}'";
            if (_machineStatus.IsStandByProcessMode)
            {
                MessageBoxEx.Show(message, false, "WARNING");
                _machineStatus.OPCommand = EOperationCommand.Stop;
            }
            else
            {
                MessageBoxEx.Show(message, false);
                LogManager.GetLogger("Interlock").Error(message);
                _machineStatus.OPCommand = EOperationCommand.Stop;
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
