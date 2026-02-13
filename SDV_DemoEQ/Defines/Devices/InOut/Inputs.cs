using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using SDV_DemoEQ.Defines.Devices;
using System.Linq;

namespace SDV_DemoEQ.Defines
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

        public IDInput Emergency => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_SW_EMO);
        public IDInput OPButtonStart => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_SW_START);
        public IDInput OPButtonStop => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_SW_STOP);
        public IDInput OPButtonReset => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_SW_RESET);
        public IDInput ServoOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SERVO_ON);
        public IDInput MainCDACheck => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.MAIN_CDA_CHECK);
        public IDInput DoorCloseLeft => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_CLOSE_LEFT);
        public IDInput DoorCloseRight => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_CLOSE_RIGHT);

        public bool DoorClose => DoorCloseLeft.Value && DoorCloseRight.Value;
    }
}
