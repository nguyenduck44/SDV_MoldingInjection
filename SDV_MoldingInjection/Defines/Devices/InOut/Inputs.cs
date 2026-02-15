using CommunityToolkit.Mvvm.ComponentModel;
using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Defines.Devices;
using SDV_MoldingInjection.Extensions;
using System.Linq;

namespace SDV_MoldingInjection.Defines
{
    public class Inputs : ObservableObject
    {
        private readonly IDInputDevice _dMachineInputDevice;
        private readonly IDInputDevice _dHead1InputDevice;
        private readonly IDInputDevice _dHead2InputDevice;
        private readonly IDInputDevice _dHead3InputDevice;
        private readonly IDInputDevice _dHead4InputDevice;

        public Inputs(IEnumerable<IDInputDevice> dInputDevices)
        {
            _dMachineInputDevice = dInputDevices.First(device => device.Name == EInputDevice.MachineInput.ToString());
            _dHead1InputDevice = dInputDevices.First(device => device.Name == EInputDevice.Head1Input.ToString());
            _dHead2InputDevice = dInputDevices.First(device => device.Name == EInputDevice.Head2Input.ToString());
            _dHead3InputDevice = dInputDevices.First(device => device.Name == EInputDevice.Head3Input.ToString());
            _dHead4InputDevice = dInputDevices.First(device => device.Name == EInputDevice.Head4Input.ToString());

            Initialize();
        }

        public bool Initialize()
        {
            bool ret = false;
            ret &= _dMachineInputDevice.Initialize();
            ret &= _dHead1InputDevice.Initialize();
            ret &= _dHead2InputDevice.Initialize();
            ret &= _dHead3InputDevice.Initialize();
            ret &= _dHead4InputDevice.Initialize();
            return ret;
        }

        public bool Connect()
        {
            bool ret = false;
            ret &= _dMachineInputDevice.Connect();
            ret &= _dHead1InputDevice.Connect();
            ret &= _dHead2InputDevice.Connect();
            ret &= _dHead3InputDevice.Connect();
            ret &= _dHead4InputDevice.Connect();
            return ret;
        }

        public bool Disconnect()
        {
            return _dMachineInputDevice.Disconnect()
                && _dHead1InputDevice.Disconnect();
        }

        public List<IDInput> All => _dMachineInputDevice.Inputs
            .Concat(_dHead1InputDevice.Inputs)
            .Concat(_dHead2InputDevice.Inputs)
            .Concat(_dHead3InputDevice.Inputs)
            .Concat(_dHead4InputDevice.Inputs)
            .ToList();

        public List<IDInput> Machine => _dMachineInputDevice.Inputs;
        public List<IDInput> Head1 => _dHead1InputDevice.Inputs;
        public List<IDInput> Head2 => _dHead2InputDevice.Inputs;
        public List<IDInput> Head3 => _dHead3InputDevice.Inputs;
        public List<IDInput> Head4 => _dHead4InputDevice.Inputs;

        public IDInput Emergency => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_SW_EMO);
        public IDInput OPButtonStart => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_SW_START);
        public IDInput OPButtonStop => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_SW_STOP);
        public IDInput OPButtonReset => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_SW_RESET);
        public IDInput ServoOn => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SERVO_ON);
        public IDInput MainCDACheck => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_CDA_CHECK);
        public IDInput DoorCloseLeft => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DOOR_CLOSE_LEFT);
        public IDInput DoorCloseRight => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DOOR_CLOSE_RIGHT);

        public bool DoorClose => DoorCloseLeft.Value && DoorCloseRight.Value;
    }
}
