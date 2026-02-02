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
        public IDInput Emergency => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_SWITCH_EMO);
        public IDInput OPSwitchAuto => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_SWITCH_AUTO);
        public IDInput OPSwitchTeach => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.OP_SWITCH_TEACH);
        public IDInput MainPanelFanRun1 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.MAIN_PANEL_FAN_RUN_1);
        public IDInput MainPanelFanRun2 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.MAIN_PANEL_FAN_RUN_2);
        public IDInput SmokeDetectRun => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SMOKE_DETECT_RUN);
        public IDInput SmokeDetectAlarm => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SMOKE_DETECT_ALARM);
        public IDInput SmokeDetectTempHighAlarm => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SMOKE_DETECT_TEMP_HIGH_ALARM);
        public IDInput SmokeDetectTempHighWarn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SMOKE_DETECT_TEMP_HIGH_WARN);
        public IDInput ServoOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SERVO_ON);
        public IDInput MainBreakerTrip => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.MAIN_BREAKER_TRIP);
        public IDInput MainCDACheck => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.MAIN_CDA_CHECK);

        public IDInput DoorCloseFrontLeft => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_CLOSE_FRONT_LEFT);
        public IDInput DoorLockFrontLeft => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_LOCK_FRONT_LEFT);
        public IDInput DoorCloseFrontRight => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_CLOSE_FRONT_RIGHT);
        public IDInput DoorLockFrontRight => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_LOCK_FRONT_RIGHT);
        public IDInput DoorCloseLeftFront => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_CLOSE_LEFT_FRONT);
        public IDInput DoorLockLeftFront => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_LOCK_LEFT_FRONT);
        public IDInput DoorCloseLeftRear => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_CLOSE_LEFT_REAR);
        public IDInput DoorLockLeftRear => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_LOCK_LEFT_REAR);
        public IDInput DoorCloseRearLeft => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_CLOSE_REAR_LEFT);
        public IDInput DoorLockRearLeft => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_LOCK_REAR_LEFT);
        public IDInput DoorCloseRearRight => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_CLOSE_REAR_RIGHT);
        public IDInput DoorLockRearRight => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.DOOR_LOCK_REAR_RIGHT);
        public IDInput LightCurtainLeftDetect => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.LIGHT_CURTAIN_LEFT_DETECT);
        public IDInput LightCurtainRightDetect => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.LIGHT_CURTAIN_RIGHT_DETECT);
        public IDInput SWStartLeft => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SW_START_LEFT);
        public IDInput SWStartRight => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SW_START_RIGHT);

        public IDInput PlasmaStatus1 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.PLASMA_STATUS_1);
        public IDInput PlasmaStatus2 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.PLASMA_STATUS_2);
        public IDInput PlasmaStatus3 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.PLASMA_STATUS_3);
        public IDInput PlasmaStatus4 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.PLASMA_STATUS_4);
        public IDInput JettingOut1 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.JETTING_OUT_1);
        public IDInput JettingOut2 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.JETTING_OUT_2);
        public IDInput JettingOut3 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.JETTING_OUT_3);
        public IDInput JettingOut4 => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.JETTING_OUT_4);
        public IDInput UvCureTempAlarm => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.UV_CURE_TEMP_ALARM);
        public IDInput UvCureLedAlarm => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.UV_CURE_LED_ALARM);
        public IDInput UvCureLefOnOk => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.UV_CURE_LEF_ON_OK);

        public IDInput PlasmaCoverRight => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.PLASMA_COVER_RIGHT);
        public IDInput PlasmaCoverLeft => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.PLASMA_COVER_LEFT);
        public IDInput SylingerEpoxyCheck => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.SYLINGER_EPOXY_CHECK);
        public IDInput NozzleCleanerUp => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.NOZZLE_CLEANER_UP);
        public IDInput NozzleCleanerDown => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.NOZZLE_CLEANER_DOWN);
        public IDInput NozzleCleanerWipeCheck => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.NOZZLE_CLEANER_WIPE_CHECK);
        public IDInput UvCureFw => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.UV_CURE_FW);
        public IDInput UvCureBw => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.UV_CURE_BW);
        public IDInput TransferHand1Up => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.TRANSFER_HAND_1_UP);
        public IDInput TransferHand1Down => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.TRANSFER_HAND_1_DOWN);
        public IDInput TransferHand2Up => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.TRANSFER_HAND_2_UP);
        public IDInput TransferHand2Down => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.TRANSFER_HAND_2_DOWN);
        public IDInput TransferHand1VacOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.TRANSFER_HAND_1_VAC_ON);
        public IDInput TransferHand2VacOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.TRANSFER_HAND_2_VAC_ON);

        public IDInput StageLLeftVacOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.STAGE_L_LEFT_VAC_ON);
        public IDInput StageLRightVacOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.STAGE_L_RIGHT_VAC_ON);
        public IDInput StageRLeftVacOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.STAGE_R_LEFT_VAC_ON);
        public IDInput StageRRightVacOn => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.STAGE_R_RIGHT_VAC_ON);
        public IDInput MainN2CDACheck => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.MAIN_N2_CDA_CHECK);
        public IDInput PlasmaCDACheck => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.PLASMA_CDA_CHECK);
        public IDInput PlasmaN2CDACheck => _dInputDevice.Inputs.First(i => i.Id == (int)EInput.PLASMA_N2_CDA_CHECK);

        public bool DoorClose => DoorCloseFrontLeft.Value && DoorCloseFrontRight.Value &&
                                 DoorCloseLeftFront.Value && DoorCloseLeftRear.Value &&
                                 DoorCloseRearLeft.Value && DoorCloseRearRight.Value;

    }
}
