using EQX.Core.InOut;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

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

        public IDOutput OPButtonStartLamp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OP_START_LAMP);
        public IDOutput OPButtonStopLamp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OP_STOP_LAMP);
        public IDOutput OPButtonResetLamp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OP_RESET_LAMP);
        public IDOutput OPKeySwLock => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.OP_KEY_SW_LOCK);
        public IDOutput TowerLampRed => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TOWER_LAMP_RED);
        public IDOutput TowerLampYellow => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TOWER_LAMP_YELLOW);
        public IDOutput TowerLampGreen => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TOWER_LAMP_GREEN);
        public IDOutput Buzzer1On => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.BUZZER1_ON);
        public IDOutput Buzzer2On => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.BUZZER2_ON);
        public IDOutput Buzzer3On => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.BUZZER3_ON);
        public IDOutput Buzzer4On => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.BUZZER4_ON);
        public IDOutput SwStartLeftLp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.SW_START_LEFT_LP);
        public IDOutput SwStartRightLp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.SW_START_RIGHT_LP);

        public IDOutput EqpStop => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.EQP_STOP);
        public IDOutput LightCurtainLeftMuting1 => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.LIGHT_CURTAIN_LEFT_MUTING_1);
        public IDOutput LightCurtainLeftMuting2 => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.LIGHT_CURTAIN_LEFT_MUTING_2);
        public IDOutput LightCurtainRightMuting1 => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.LIGHT_CURTAIN_RIGHT_MUTING_1);
        public IDOutput LightCurtainRightMuting2 => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.LIGHT_CURTAIN_RIGHT_MUTING_2);
        public IDOutput PlasmaRun => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.PLASMA_RUN);
        public IDOutput PlasmaRemote => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.PLASMA_REMOTE);
        public IDOutput PlasmaN2Sol => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.PLASMA_N2_SOL);
        public IDOutput PlasmaCdaSol => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.PLASMA_CDA_SOL);
        public IDOutput PlasmaReboot => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.PLASMA_REBOOT);
        public IDOutput JettingActive => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.JETTING_ACTIVE);
        public IDOutput JettingIn1 => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.JETTING_IN1);
        public IDOutput JettingIn2 => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.JETTING_IN2);
        public IDOutput JettingIn3 => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.JETTING_IN3);

        public IDOutput NozzleCleanerRollCw => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.NOZZLE_CLEANER_ROLL_CW);
        public IDOutput NozzleCleanerRollCcw => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.NOZZLE_CLEANER_ROLL_CCW);
        public IDOutput UvCureLedOnOff => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.UV_CURE_LED_ON_OFF);
        public IDOutput PlasmaCylFw => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.PLASMA_CYL_FW);
        public IDOutput PlasmaCylBw => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.PLASMA_CYL_BW);
        public IDOutput TransferHand1Up => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TRANSFER_HAND_1_UP);
        public IDOutput TransferHand1Down => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TRANSFER_HAND_1_DOWN);
        public IDOutput TransferHand2Up => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TRANSFER_HAND_2_UP);
        public IDOutput TransferHand2Down => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TRANSFER_HAND_2_DOWN);
        public IDOutput UvCureFw => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.UV_CURE_FW);
        public IDOutput UvCureBw => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.UV_CURE_BW);
        public IDOutput NozzleCleanerUp => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.NOZZLE_CLEANER_UP);

        public IDOutput StageLLeftVacOn => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.STAGE_L_LEFT_VAC_ON);
        public IDOutput StageLLeftPurge => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.STAGE_L_LEFT_PURGE);
        public IDOutput StageLRightVacOn => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.STAGE_L_RIGHT_VAC_ON);
        public IDOutput StageLRightPurge => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.STAGE_L_RIGHT_PURGE);
        public IDOutput StageRLeftVacOn => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.STAGE_R_LEFT_VAC_ON);
        public IDOutput StageRLeftPurge => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.STAGE_R_LEFT_PURGE);
        public IDOutput StageRRightVacOn => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.STAGE_R_RIGHT_VAC_ON);
        public IDOutput StageRRightPurge => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.STAGE_R_RIGHT_PURGE);
        public IDOutput TransferHand1VacOn => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TRANSFER_HAND_1_VAC_ON);
        public IDOutput TransferHand1Purge => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TRANSFER_HAND_1_PURGE);
        public IDOutput TransferHand2VacOn => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TRANSFER_HAND_2_VAC_ON);
        public IDOutput TransferHand2Purge => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.TRANSFER_HAND_2_PURGE);
        public IDOutput UvCureCoolingAirBlow => _dOutputDevice.Outputs.First(i => i.Id == (int)EOutput.UV_CURE_COOLING_AIR_BLOW);

        public void Lamp_Run()
        {
            Lamp_Clear();

            TowerLampGreen.Value = true;
            OPButtonStartLamp.Value = true;

            Buzzer1On.Value = false;
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

            if (isUseBuzzer)
            {
                Buzzer1On.Value = true;
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
