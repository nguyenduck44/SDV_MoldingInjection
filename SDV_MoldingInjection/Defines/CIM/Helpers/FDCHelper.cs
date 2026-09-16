using EQX.Core.Communication.CIM.Custom.WordArea;
using TOPENG_Device;

namespace SDV_MoldingInjection.Defines.CIM
{
    public class FDCHelper
    {
        public static void GetFDCInfo(EFDCValue fDCValue, out int address, out int length)
        {
            switch (fDCValue)
            {
                case EFDCValue.DISPENSING_CELL_ID: address = 0xABEC; length = 40; break;
                case EFDCValue.DISPENSING_STEP_ID: address = 0xAC14; length = 2; break;
                case EFDCValue.CHAMBER_DISPENSING_PIRANI_CONTROL: address = 0xAC16; length = 2; break;
                case EFDCValue.HEAD_DISPENSING_PRESSURE: address = 0xAC18; length = 2; break;
                case EFDCValue.HEAD_1_INJECT_TIME: address = 0xAC1A; length = 2; break;
                case EFDCValue.HEAD_2_INJECT_TIME: address = 0xAC1C; length = 2; break;
                case EFDCValue.HEAD_3_INJECT_TIME: address = 0xAC1E; length = 2; break;
                case EFDCValue.HEAD_4_INJECT_TIME: address = 0xAC20; length = 2; break;
                case EFDCValue.HEAD_1_INJECT_SPEED: address = 0xAC22; length = 2; break;
                case EFDCValue.HEAD_2_INJECT_SPEED: address = 0xAC24; length = 2; break;
                case EFDCValue.HEAD_3_INJECT_SPEED: address = 0xAC26; length = 2; break;
                case EFDCValue.HEAD_4_INJECT_SPEED: address = 0xAC28; length = 2; break;
                case EFDCValue.VENT_TIME: address = 0xAC2A; length = 2; break;
                case EFDCValue.RATIO_VENT: address = 0xAC2C; length = 2; break;
                case EFDCValue.PUMP_STEP_ID: address = 0xACB0; length = 2; break;
                case EFDCValue.CDA_ANGLE_VALVE_PRESSURE: address = 0xACB2; length = 2; break;
                case EFDCValue.CDA_VENT_PRESSURE: address = 0xACB4; length = 2; break;
                case EFDCValue.MAIN_PANEL_TEMPRATURE: address = 0xACE0; length = 2; break;
                case EFDCValue.MAIN_PANEL_HUMIDITY: address = 0xACE2; length = 2; break;
                case EFDCValue.MAIN_POWER_PANEL_TEMPRATURE: address = 0xACE4; length = 2; break;
                case EFDCValue.MAIN_POWER_PANEL_HUMIDITY: address = 0xACE6; length = 2; break;
                case EFDCValue.CDA_MAIN_AIR_PRESSURE: address = 0xACE8; length = 2; break;
                case EFDCValue.FUNCTION_STEP_ID: address = 0xAD30; length = 1; break;
                case EFDCValue.DISPENSING1_ONOFF: address = 0xAD31; length = 1; break;
                case EFDCValue.DISPENSING2_ONOFF: address = 0xAD32; length = 1; break;
                case EFDCValue.DISPENSING3_ONOFF: address = 0xAD33; length = 1; break;
                case EFDCValue.DISPENSING4_ONOFF: address = 0xAD34; length = 1; break;
                case EFDCValue.SKIP_VENT: address = 0xAD35; length = 1; break;
                case EFDCValue.SKIP_DUMMYSHOT: address = 0xAD36; length = 1; break;
                case EFDCValue.SKIP_NEEDLECLEAN: address = 0xAD37; length = 1; break;
                case EFDCValue.SKIP_DISPENSING: address = 0xAD38; length = 1; break;
                case EFDCValue.USE_ADD_TAIL: address = 0xAD39; length = 1; break;
                case EFDCValue.SKIP_HEAD12: address = 0xAD3A; length = 1; break;
                case EFDCValue.SKIP_HEAD34: address = 0xAD3B; length = 1; break;
                case EFDCValue.INPUT_TYPE_MANUAL: address = 0xAD3C; length = 1; break;
                case EFDCValue.INPUT_TYPE_AUTO: address = 0xAD3D; length = 1; break;
                case EFDCValue.SAVE_PRESSURE_LOG: address = 0xAD3E; length = 1; break;
                case EFDCValue.ONE_TORR_AND_PURGE: address = 0xAD3F; length = 1; break;
                case EFDCValue.DELAY_AFTER_INJECT_FINISH: address = 0xAD40; length = 1; break;
                case EFDCValue.ENABLE_INJECTION_PATH: address = 0xAD41; length = 1; break;
                case EFDCValue.INJECT_AFTER_OPEN_ANGLE_VALVE: address = 0xAD42; length = 1; break;
                case EFDCValue.CIM_ONOFF: address = 0xAD43; length = 1; break;
                case EFDCValue.ENABLE_IDLEPURGE: address = 0xAD44; length = 1; break;
                case EFDCValue.N02_U_T_PW: address = 0xAF0C; length = 2; break;
                case EFDCValue.N02_U_I_PW: address = 0xAF0E; length = 2; break;
                case EFDCValue.N02_U_R_VOL: address = 0xAF10; length = 2; break;
                case EFDCValue.N02_U_T_VOL: address = 0xAF12; length = 2; break;
                case EFDCValue.N02_U_R_CUR: address = 0xAF14; length = 2; break;
                case EFDCValue.N02_U_T_CUR: address = 0xAF16; length = 2; break;

                default: address = 0; length = 0; break;
            }
        }

        public static void WriteFDCValue(EFDCValue fDCValue, string value)
        {
            GetFDCInfo(fDCValue, out int address, out int length);

            CIMAddressMap.WriteWords(address, length, new short[length], false);

            if (string.IsNullOrEmpty(value)) return;

            CIMAddressMap.WriteWords(address, length, CIMScenarioDispatcher.EncodeAsciiToWords(value), false);
        }

        public static void WriteFDCValue(EFDCValue fDCValue, int value, bool isNotWrite = false)
        {
            if (isNotWrite) return;

            GetFDCInfo(fDCValue, out int address, out int length);

            short[] data = new short[2];
            PlcDataConverter.SetInt32(data, 0, value);

            CIMAddressMap.WriteWords(address, length, data, false);
        }
    }
}
