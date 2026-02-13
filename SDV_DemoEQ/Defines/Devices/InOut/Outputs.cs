using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace SDV_DemoEQ.Defines
{
    public class Outputs
    {
        private readonly IDOutputDevice _dOutputDevice;

        public Outputs([FromKeyedServices("OutputDevice#1")] IDOutputDevice dOutputDevice)
        {
            _dOutputDevice = dOutputDevice;

            Initialize();
        }

        public bool Initialize()
        {
            return _dOutputDevice.Initialize();
        }

        public bool Connect()
        {
            return _dOutputDevice.Connect();
        }

        public bool Disconnect()
        {
            return _dOutputDevice.Disconnect();
        }

        public List<IDOutput> All => _dOutputDevice.Outputs;

        public IDOutput Buzzer1On => _dOutputDevice.Outputs.First(o => o.Id == (int)EOutput.BUZZER1_ON);

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
