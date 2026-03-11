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
                && _dHead1InputDevice.Disconnect()
                && _dHead2InputDevice.Disconnect()
                && _dHead3InputDevice.Disconnect()
                && _dHead4InputDevice.Disconnect();
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

        public IDInput OPButtonStart => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_SW_START);
        public IDInput OPButtonStop => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_SW_STOP);
        public IDInput OPButtonReset => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_SW_RESET);
        public IDInput Emergency => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_SW_EMO);
        public IDInput AutoSW => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_KEY_SW_AUTO);
        public IDInput TeachSW => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_KEY_SW_TEACH);
        public IDInput Fan1Run => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_PANEL_FAN_RUN1);
        public IDInput Fan2Run => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_PANEL_FAN_RUN2);
        public IDInput SmokeDetectRun => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SMOKE_DETECT_RUN);
        public IDInput SmokeDetectAlarm => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SMOKE_DETECT_DETECT_ALARM);
        public IDInput TempHighAlarm => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SMOKE_DETECT_TEMP_HIGH_ALARM);
        public IDInput TempHighWarning => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SMOKE_DETECT_TEMP_HIGH_WARNING);
        public IDInput PanelClodeCheck => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.PANEL_CLOSE_CHECK);

        public IDInput ServoOn => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SERVO_ON);
        public IDInput MainBreakerTrip => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_BREAKER_TRIP);
        public IDInput MainCDACheck => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_CDA_CHECK);

        public IDInput DoorOpenLeft => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DOOR_OPEN_LEFT);
        public IDInput DoorReleaseLeft => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DOOR_RELEASE_LEFT);
        public IDInput DoorOpenRight => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DOOR_OPEN_RIGHT);
        public IDInput DoorReleaseRight => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DOOR_RELEASE_RIGHT);
        public IDInput MainPanelCloseCheck => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_PANEL_CLOSE_CHECK);    

        public IDInput MainBreakerTripEBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_BREAKER_TRIP_EBOX);
        public IDInput MainPanelFanRun1EBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_PANEL_FAN_RUN1_EBOX);
        public IDInput MainPanelFanRun2EBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_PANEL_FAN_RUN2_EBOX);
        public IDInput MainPanelFanRun3EBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_PANEL_FAN_RUN3_EBOX);
        public IDInput MainPanelFanRun4EBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.MAIN_PANEL_FAN_RUN4_EBOX);
        public IDInput SmokeDetectRunEBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SMOKE_DETECT_RUN_EBOX);
        public IDInput SmokeDetectAlarmEBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SMOKE_DETECT_DETECT_ALARM_EBOX);
        public IDInput TempHighAlarmEBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SMOKE_DETECT_TEMP_HIGH_ALARM_EBOX);
        public IDInput TempHighWarningEBox => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SMOKE_DETECT_TEMP_HIGH_WARNING_EBOX);

        public IDInput Nozzle1Touch => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.NOZZLE1_TOUCH_DETECT);
        public IDInput Nozzle2Touch => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.NOZZLE2_TOUCH_DETECT);
        public IDInput Nozzle3Touch => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.NOZZLE3_TOUCH_DETECT);
        public IDInput Nozzle4Touch => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.NOZZLE4_TOUCH_DETECT);
        public IDInput Nozzle1Clean => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.NOZZLE1_CLEAN_CHECK);
        public IDInput Nozzle2Clean => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.NOZZLE2_CLEAN_CHECK);
        public IDInput Nozzle3Clean => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.NOZZLE3_CLEAN_CHECK);
        public IDInput Nozzle4Clean => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.NOZZLE4_CLEAN_CHECK);

        public IDInput Jig1Detect => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.JIG1_DETECT);
        public IDInput Jig2Detect => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.JIG2_DETECT);
        public IDInput Jig3Detect => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.JIG3_DETECT);
        public IDInput Jig4Detect => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.JIG4_DETECT);

        // Vacuum perform
        public IDInput AngleValveOpen => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.ANGLE_VALVE_OPEN);
        // Vacuum block
        public IDInput AngleValveClose => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.ANGLE_VALVE_CLOSE);

        public IDInput BelowsUp => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.BELLOWS_UP);
        public IDInput BelowsDown => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.BELLOWS_DOWN);

        public IDInput DryPumpRun => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DRY_PUMP_RUN);
        public IDInput DryPumpAlarm => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DRY_PUMP_ALARM);
        public IDInput ChamberPurgeOn => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.CHAMBER_PURGE_ON);
        public IDInput ChamberOpen => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.CHAMBER_OPEN);
        public IDInput ChamberClose => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.CHAMBER_CLOSE);
        public IDInput PanelCloseCheckLeft => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.PANEL_CLOSE_CHECK_LEFT);
        public IDInput LockMonitorSwitch => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_KEY_SW_LOCK_MONITOR);
        public IDInput LockKeyCheckSwitch => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.OP_KEY_SW_LOCK_KEY_CHECK);

        public IDInput SyringeCDACheck => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.SYLINGE_HEAD_CDA_CHECK);
        public IDInput PumpCDACheck => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.PUMP_CDA_CHECK);
        public IDInput PumpVentCDACheck => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.PUMP_VENT_CDA_CHECK);
        public IDInput PumpFanRun1 => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.PUMP_FAN_RUN1);
        public IDInput PumpFanRun2 => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.PUMP_FAN_RUN2);
        public IDInput DummyOverflowDetect1 => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DUMMY_OVERFLOW_DETECT_1);
        public IDInput DummyOverflowDetect2 => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DUMMY_OVERFLOW_DETECT_2);
        public IDInput DummyOverflowDetect3 => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DUMMY_OVERFLOW_DETECT_3);
        public IDInput DummyOverflowDetect4 => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.DUMMY_OVERFLOW_DETECT_4);
        public IDInput EmoInterfaceKNK => _dMachineInputDevice.Inputs.First(i => i.Id == (int)EMachineInput.EMO_INTERFACE_KNK);

        #region SPD Head Inputs
        public IDInput H1_CylUp => _dHead1InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.CYL_UP_POS);
        public IDInput H1_CylDown => _dHead1InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.CYL_DONW_POS);
        public IDInput H1_GateOpen => _dHead1InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.GATE_OPEN);
        public IDInput H1_GateClose => _dHead1InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.GATE_CLOSE);
        public IDInput H1_AssembleCheck => _dHead1InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.ASSEMBLE_CHECK);
        public IDInput H1_SyringeCheck => _dHead1InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.SYRINGE_CHECK);
        public IDInput H1_SyringeAir => _dHead1InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.SYRINGE_AIR);

        public IDInput H2_CylUp => _dHead2InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.CYL_UP_POS);
        public IDInput H2_CylDown => _dHead2InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.CYL_DONW_POS);
        public IDInput H2_GateOpen => _dHead2InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.GATE_OPEN);
        public IDInput H2_GateClose => _dHead2InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.GATE_CLOSE);
        public IDInput H2_AssembleCheck => _dHead2InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.ASSEMBLE_CHECK);
        public IDInput H2_SyringeCheck => _dHead2InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.SYRINGE_CHECK);
        public IDInput H2_SyringeAir => _dHead2InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.SYRINGE_AIR);

        public IDInput H3_CylUp => _dHead3InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.CYL_UP_POS);
        public IDInput H3_CylDown => _dHead3InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.CYL_DONW_POS);
        public IDInput H3_GateOpen => _dHead3InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.GATE_OPEN);
        public IDInput H3_GateClose => _dHead3InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.GATE_CLOSE);
        public IDInput H3_AssembleCheck => _dHead3InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.ASSEMBLE_CHECK);
        public IDInput H3_SyringeCheck => _dHead3InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.SYRINGE_CHECK);
        public IDInput H3_SyringeAir => _dHead3InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.SYRINGE_AIR);

        public IDInput H4_CylUp => _dHead4InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.CYL_UP_POS);
        public IDInput H4_CylDown => _dHead4InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.CYL_DONW_POS);
        public IDInput H4_GateOpen => _dHead4InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.GATE_OPEN);
        public IDInput H4_GateClose => _dHead4InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.GATE_CLOSE);
        public IDInput H4_AssembleCheck => _dHead4InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.ASSEMBLE_CHECK);
        public IDInput H4_SyringeCheck => _dHead4InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.SYRINGE_CHECK);
        public IDInput H4_SyringeAir => _dHead4InputDevice.Inputs.First(i => i.Id == (int)EHeadInput.SYRINGE_AIR);
        #endregion

        public bool DoorClose =>
            DoorOpenLeft.Value == false &&
            DoorReleaseLeft.Value == false &&
            DoorOpenRight.Value == false &&
            DoorReleaseRight.Value == false;
    }
}
