using System.Diagnostics;
using TOPENG_Device;

namespace SDV_MoldingInjection.Test.CIM
{
    public class ConnectionTest
    {
        [Fact]
        public async Task AliveBitTest()
        {
            MCCLinkIE.mCCLinkIEUse = true;

            if (MCCLinkIE.mCCLinkIEUse == true)
            {
                int iCCLinkIEOpen = MCCLinkIE.Open();
                if (iCCLinkIEOpen == 0)  //OK
                {
                    MCCLinkIE.Initial();
                    MCCLinkIE.mRDeviceNo = MPLC.PLCReadBaseAddress_B;
                    MCCLinkIE.mRDeviceNoLength = MPLC.PLCReadBaseAddressLength_B;
                    MCCLinkIE.mSDeviceNo = MPLC.PLCWriteBaseAddressLength_B;
                    MCCLinkIE.mSDeviceNoLength = MPLC.PLCReadBaseAddressLength_B;
                }
                else
                {

                }
            }

            System.Timers.Timer rwTask = new System.Timers.Timer(200);
            rwTask.Elapsed += (s, e) =>
            {
                MPLC.PLCRead(false, 0, (int)MPLC.enumDeviceType.B, false);
                MPLC.PLCRead(false, 0, (int)MPLC.enumDeviceType.W, false);

                Task.Delay(50);
                bool bitOn = CIMCommandHandler.IsCIMBitOn(CIMCommand.TerminalDisplay);
                if (bitOn)
                {
                    int startWord = CIMAddressMap.GetReadWordIndexFromDAddress("D011");
                    var buf = new short[60];
                    CIMAddressMap.ReadWords(startWord, 60, buf);
                    CIMCommandHandler.SetLocalPLCBitOn(CIMCommand.TerminalDisplay);

                    string message = "";
                    for (int i = 0; i < buf.Length; i++)
                    {
                        message += System.Text.Encoding.ASCII.GetString(BitConverter.GetBytes(buf[i]));
                    }
                    Debug.WriteLine(message);
                }
                Debug.WriteLine(MPLC.PLCWriteValue_B[0]);

                MPLC.PLCWrite(false, 0, (int)MPLC.enumDeviceType.B, false);
                MPLC.PLCWrite(false, 0, (int)MPLC.enumDeviceType.W, false);

                Task.Delay(50);
            };
            rwTask.Start();

            CIMAliveTask task = new CIMAliveTask();
            task.Start();

            CIMScenarioDispatcher.ExecuteScenario(CIMScenario.Initialize);

            Task.Delay(30000).Wait();
        }
    }
}
