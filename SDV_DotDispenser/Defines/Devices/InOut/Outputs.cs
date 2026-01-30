using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace SDV_DotDispenser.Defines
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

        public IDOutput TowerLampRed => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TOWER_LAMP_RED);
        public IDOutput TowerLampYellow => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TOWER_LAMP_YELLOW);
        public IDOutput TowerLampGreen => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TOWER_LAMP_GREEN);
        public IDOutput TowerBuzzer => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TOWER_BUZZER);
        public IDOutput OPButtonStartLamp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OP_BUTTON_START_LAMP);
        public IDOutput OPButtonStopLamp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OP_BUTTON_STOP_LAMP);
        public IDOutput OPButtonResetLamp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OP_BUTTON_RESET_LAMP);
        public IDOutput DoorOpen => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.DOOR_OPEN);
        public IDOutput InCst_StopperUp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.IN_CV_STOPPER_UP);
        public IDOutput InCst_StopperDown => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.IN_CV_STOPPER_DOWN);
        public IDOutput OutCst_StopperUp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OUT_CV_STOPPER_UP);
        public IDOutput OutCst_StopperDown => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OUT_CV_STOPPER_DOWN);
        
        public void Lamp_Run()
        {
            Lamp_Clear();

            TowerLampGreen.Value = true;
            OPButtonStartLamp.Value = true;

            TowerBuzzer.Value = false;
        }

        public void Lamp_Stop()
        {
            Lamp_Clear();

            TowerLampYellow.Value = true;
            OPButtonStopLamp.Value = true;
        }

        public void Lamp_Alarm(bool isUseBuzzer)
        {
            Lamp_Clear();

            TowerLampRed.Value = true;

            if(isUseBuzzer)
            {
                TowerBuzzer.Value = true;
            }
        }
        private void Lamp_Clear()
        {
            TowerLampGreen.Value = false;
            TowerLampRed.Value = false;
            TowerLampYellow.Value = false;

            OPButtonStartLamp.Value = false;
            OPButtonStopLamp.Value = false;
            OPButtonResetLamp.Value = false;
        }

    }
}
