using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using SDV_MoldingInjection.Extensions;
using System.Linq;

namespace SDV_MoldingInjection.Defines
{
    public class Outputs
    {
        private readonly IDOutputDevice _dMachineOutputDevice;
        private readonly IDOutputDevice _dHead1OutputDevice;
        private readonly IDOutputDevice _dHead2OutputDevice;
        private readonly IDOutputDevice _dHead3OutputDevice;
        private readonly IDOutputDevice _dHead4OutputDevice;

        public Outputs(IEnumerable<IDOutputDevice> dOutputDevices)
        {
            _dMachineOutputDevice = dOutputDevices.First(device => device.Name == EOutputDevice.MachineOutput.ToString());
            _dHead1OutputDevice = dOutputDevices.First(device => device.Name == EOutputDevice.Head1Output.ToString());
            _dHead2OutputDevice = dOutputDevices.First(device => device.Name == EOutputDevice.Head2Output.ToString());
            _dHead3OutputDevice = dOutputDevices.First(device => device.Name == EOutputDevice.Head3Output.ToString());
            _dHead4OutputDevice = dOutputDevices.First(device => device.Name == EOutputDevice.Head4Output.ToString());

            Initialize();
        }

        public bool Initialize()
        {
            return _dMachineOutputDevice.Initialize()
                && _dHead1OutputDevice.Initialize()
                && _dHead2OutputDevice.Initialize()
                && _dHead3OutputDevice.Initialize()
                && _dHead4OutputDevice.Initialize();
        }

        public bool Connect()
        {
            return _dMachineOutputDevice.Connect()
                && _dHead1OutputDevice.Connect()
                && _dHead2OutputDevice.Connect()
                && _dHead3OutputDevice.Connect()
                && _dHead4OutputDevice.Connect();
        }

        public bool Disconnect()
        {
            return _dMachineOutputDevice.Disconnect()
                && _dHead1OutputDevice.Disconnect()
                && _dHead2OutputDevice.Disconnect()
                && _dHead3OutputDevice.Disconnect()
                && _dHead4OutputDevice.Disconnect();
        }

        public List<IDOutput> All => _dMachineOutputDevice.Outputs
            .Concat(_dHead1OutputDevice.Outputs)
            .Concat(_dHead2OutputDevice.Outputs)
            .Concat(_dHead3OutputDevice.Outputs)
            .Concat(_dHead4OutputDevice.Outputs)
            .ToList();

        public List<IDOutput> Machine => _dMachineOutputDevice.Outputs;
        public List<IDOutput> Head1 => _dHead1OutputDevice.Outputs;
        public List<IDOutput> Head2 => _dHead2OutputDevice.Outputs;
        public List<IDOutput> Head3 => _dHead3OutputDevice.Outputs;
        public List<IDOutput> Head4 => _dHead4OutputDevice.Outputs;

        public IDOutput Buzzer1On => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.BUZZER1_ON);

        public void Lamp_Run()
        {
            Lamp_Clear();
        }

        public void Lamp_Stop()
        {
            Lamp_Clear();
        }

        public void Lamp_Alarm(bool isUseBuzzer)
        {
            Lamp_Clear();
        }

        private void Lamp_Clear()
        {
        }
    }
}
