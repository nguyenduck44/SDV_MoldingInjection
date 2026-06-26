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
                case EFDCValue.PUMP_STEP_ID: address = 0xAC86; length = 2; break;
                case EFDCValue.CDA_ANGLE_VALVE_PRESSURE: address = 0xAC88; length = 2; break;
                case EFDCValue.MAIN_PANEL_TEMPRATURE: address = 0xACB6; length = 2; break;
                case EFDCValue.MAIN_POWER_PANEL_TEMPRATURE: address = 0xACBA; length = 2; break;
                case EFDCValue.MAIN_POWER_PANEL_HUMIDITY: address = 0xACBC; length = 2; break;
                case EFDCValue.CDA_MAIN_AIR_PRESSURE: address = 0xACBE; length = 2; break;
                case EFDCValue.DISPENSING1_ONOFF: address = 0xAD07; length = 1; break;
                case EFDCValue.DISPENSING2_ONOFF: address = 0xAD08; length = 1; break;
                case EFDCValue.DISPENSING3_ONOFF: address = 0xAD09; length = 1; break;
                case EFDCValue.SKIP_VENT: address = 0xAD0B; length = 1; break;
                case EFDCValue.SKIP_DUMMYSHOT: address = 0xAD0C; length = 1; break;
                case EFDCValue.SKIP_NEEDLECLEAN: address = 0xAD0D; length = 1; break;
                case EFDCValue.USE_ADD_TAIL: address = 0xAD0C; length = 1; break;
                case EFDCValue.SKIP_HEAD12: address = 0xAD0D; length = 1; break;
                case EFDCValue.SKIP_HEAD34: address = 0xAD0E; length = 1; break;
                case EFDCValue.INPUT_TYPE_MANUAL: address = 0xAD0C; length = 1; break;
                case EFDCValue.INPUT_TYPE_AUTO: address = 0xAD0D; length = 1; break;
                case EFDCValue.SAVE_PRESSURE_LOG: address = 0xAD0E; length = 1; break;
                case EFDCValue.ONE_TORR_AND_PURGE: address = 0xAD0F; length = 1; break;
                case EFDCValue.DELAY_AFTER_INJECT_FINISH: address = 0xAD0D; length = 1; break;
                case EFDCValue.ENABLE_INJECTION_PATH: address = 0xAD0E; length = 1; break;
                case EFDCValue.INJECT_AFTER_OPEN_ANGLE_VALVE: address = 0xAD0F; length = 1; break;
                case EFDCValue.CIM_ONOFF: address = 0xAD10; length = 1; break;
                case EFDCValue.READ_BARCODE: address = 0xAD0E; length = 1; break;
                case EFDCValue.ENABLE_IDLEPURGE: address = 0xAD0F; length = 1; break;
                case EFDCValue.N02_U_T_PW: address = 0xAF0C; length = 2; break;
                case EFDCValue.N02_U_I_PW: address = 0xAF0E; length = 2; break;
                case EFDCValue.N02_U_R_VOL: address = 0xAF10; length = 2; break;
                case EFDCValue.N02_U_R_CUR: address = 0xAF14; length = 2; break;
                case EFDCValue.N02_U_T_CUR: address = 0xAF16; length = 2; break;

                default: address = 0; length = 0; break;
            }
        }

        public static void WriteFDCValue(EFDCValue fDCValue, string value)
        {
            GetFDCInfo(fDCValue, out int address, out int length);

            CIMAddressMap.WriteWords(address, length, new short[length]);
            CIMAddressMap.WriteWords(address, length, CIMScenarioDispatcher.EncodeAsciiToWords(value));
        }

        public static void WriteFDCValue(EFDCValue fDCValue, int value)
        {
            GetFDCInfo(fDCValue, out int address, out int length);

            short[] data = new short[2];
            PlcDataConverter.SetInt32(data, 0, value);

            CIMAddressMap.WriteWords(address, length, data, false);
        }
    }
}
