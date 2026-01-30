using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using SDV_DotDispenser.Defines.Devices;
using System.Linq;

namespace SDV_DotDispenser.Defines
{
    public class Inputs : ObservableObject
    {
        private readonly IDInputDevice _dInputDevice;

        public Inputs([FromKeyedServices("InputDevice#1")] IDInputDevice dInputDevice)
        {
            _dInputDevice = dInputDevice;

            Initialize();
        }

        public bool Initialize()
        {
            if (_dInputDevice.Initialize() == false) return false;

            return true;
        }

        public bool Connect()
        {
            if (_dInputDevice.Connect() == false) return false;

            return true;
        }

        public bool Disconnect()
        {
            return _dInputDevice.Disconnect();
        }

        public List<IDInput> All => _dInputDevice.Inputs;

        public IDInput OPButtonStart => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_BUTTON_START);
        public IDInput OPButtonStop => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_BUTTON_STOP);
        public IDInput OPButtonReset => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_BUTTON_RESET);
        public IDInput DoorSensor => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_SENSOR);
        public IDInput Emergency => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.EMERGENCY);
        public IDInput MainAir => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.MAIN_AIR);
        public IDInput PowerMCOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.POWER_MC_ON);
        public IDInput InCV_StopperUp => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.IN_CV_STOPPER_UP);
        public IDInput InCV_StopperDown => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.IN_CV_STOPPER_DOWN);
        public IDInput OutCV_StopperUp => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OUT_CV_STOPPER_UP);
        public IDInput OutCV_StopperDown => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OUT_CV_STOPPER_DOWN);
    }
}
