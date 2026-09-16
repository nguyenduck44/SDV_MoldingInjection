using EQX.Core.InOut;
using EQX.UI.Controls;
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

            AlertNotifyView.BuzzerOff += BuzzerOff;
        }

        public bool Initialize()
        {
            bool ret = false;
            ret &= _dMachineOutputDevice.Initialize();
            ret &= _dHead1OutputDevice.Initialize();
            ret &= _dHead2OutputDevice.Initialize();
            ret &= _dHead3OutputDevice.Initialize();
            ret &= _dHead4OutputDevice.Initialize();

            return ret;
        }

        public bool Connect()
        {
            bool ret = false;
            ret &= _dMachineOutputDevice.Connect();
            ret &= _dHead1OutputDevice.Connect();
            ret &= _dHead2OutputDevice.Connect();
            ret &= _dHead3OutputDevice.Connect();
            ret &= _dHead4OutputDevice.Connect();

            return ret;
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

        public IDOutput StartLamp => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.OP_SW_START_LP);
        public IDOutput StopLamp => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.OP_SW_STOP_LP);
        public IDOutput ResetLamp => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.OP_SW_RESET_LP);

        public IDOutput SWKeyLock => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.OP_KEY_SW_LOCK);

        public IDOutput TowerLampRed => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.TOWER_LAMP_RED);
        public IDOutput TowerLampYellow => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.TOWER_LAMP_YELLOW);
        public IDOutput TowerLampGreen => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.TOWER_LAMP_GREEN);

        public IDOutput Buzzer1On => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.BUZZER1_ON);
        public IDOutput Buzzer2On => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.BUZZER1_ON);
        public IDOutput Buzzer3On => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.BUZZER1_ON);
        public IDOutput Buzzer4On => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.BUZZER1_ON);

        public IDOutput EQPStop => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.EQP_STOP);

        public IDOutput ChamberPurgeOn => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.VAC_CHAMBER_PURGE_ON);

        public IDOutput NozzleCleanH1H2 => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.NOZZLE_CLEAN_H1H2);
        public IDOutput NozzleCleanH3H4 => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.NOZZLE_CLEAN_H3H4);

        public IDOutput DryPumpRun => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.DRY_PUMP_RUN);
        public IDOutput DryPumpAlarmReset => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.DRY_PUMP_ALARM_RESET);
        public IDOutput ChamberVacOn => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.CHAMBER_VAC_ON);
        public IDOutput BellowsUp => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.BELLOW_VALVE_UP);
        public IDOutput BellowsDown => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.BELLOW_VALVE_DOWN);

        public IDOutput VacChamberOpen => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.VAC_CHAMBER_OPEN);
        public IDOutput VacChamberClose => _dMachineOutputDevice.Outputs.First(o => o.Id == (int)EMachineOutput.VAC_CHAMBER_CLOSE);

        #region SPD Head Outputs
        public IDOutput H1_CylUp => _dHead1OutputDevice.Outputs.First(o => o.Id == (int)EHeadOutput.CYL_UP);
        public IDOutput H1_CylDown => _dHead1OutputDevice.Outputs.First(o => o.Id == (int)EHeadOutput.CYL_DOWN);

        public IDOutput H2_CylUp => _dHead2OutputDevice.Outputs.First(o => o.Id == (int)EHeadOutput.CYL_UP);
        public IDOutput H2_CylDown => _dHead2OutputDevice.Outputs.First(o => o.Id == (int)EHeadOutput.CYL_DOWN);

        public IDOutput H3_CylUp => _dHead3OutputDevice.Outputs.First(o => o.Id == (int)EHeadOutput.CYL_UP);
        public IDOutput H3_CylDown => _dHead3OutputDevice.Outputs.First(o => o.Id == (int)EHeadOutput.CYL_DOWN);

        public IDOutput H4_CylUp => _dHead4OutputDevice.Outputs.First(o => o.Id == (int)EHeadOutput.CYL_UP);
        public IDOutput H4_CylDown => _dHead4OutputDevice.Outputs.First(o => o.Id == (int)EHeadOutput.CYL_DOWN);
        #endregion

        public void Lamp_Run()
        {
            Lamp_Clear();

            EQPStop.Value = false;

            TowerLampGreen.Value = true;
            StartLamp.Value = true;
        }

        public void Lamp_Stop()
        {
            Lamp_Clear();

            TowerLampYellow.Value = true;
            StopLamp.Value = true;
        }

        public void Lamp_Alarm(bool isUseBuzzer)
        {
            Lamp_Clear();

            TowerLampRed.Value = true;
            ResetSW_Blink = true;
            if (isUseBuzzer)
            {
                Buzzer1On.Value = true;
            }
        }

        public void ResetSW_Clear()
        {
            ResetSW_Blink = false;
            ResetLamp.Value = false;
        }

        private void Lamp_Clear()
        {
            TowerLampGreen.Value = false;
            TowerLampRed.Value = false;
            TowerLampYellow.Value = false;

            StartLamp.Value = false;
            StopLamp.Value = false;

            ResetSW_Blink = false;
        }

        private void BuzzerOff()
        {
            Buzzer1On.Value = false;
            Buzzer2On.Value = false;
            Buzzer3On.Value = false;
            Buzzer4On.Value = false;
        }

        private Timer ResetSW_BlinkTimer;
        private bool ResetSW_Blink
        {
            set
            {
                if (value == true)
                {
                    if (ResetSW_BlinkTimer != null)
                    {
                        ResetSW_BlinkTimer.Dispose();
                    }
                    ResetSW_BlinkTimer = new Timer((sender) =>
                    {
                        ResetLamp.Value = !ResetLamp.Value;
                    }, null, 0, 500);
                }
                else
                {
                    ResetLamp.Value = false;
                    if (ResetSW_BlinkTimer != null)
                    {
                        ResetSW_BlinkTimer.Dispose();
                    }
                }
            }
        }
    }
}
