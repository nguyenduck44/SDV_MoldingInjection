using EQX.Core.Communication.CIM;
using EQX.UI.Controls;
using EQX.UI.MVVM;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.IO;
using System.Text.RegularExpressions;
using TOPENG_Device;

namespace SDV_MoldingInjection.Defines.CIM
{
    public class CIMMCCActionInfo
    {
        private int fromAddress;

        public int FromAddress
        {
            get { return fromAddress; }
            set
            {
                fromAddress = value;
                ToAddress = FromAddress + 2;
                StartTimeAddress = FromAddress + 4;
                EndTimeAddress = FromAddress + 8;
            }
        }

        public int ToAddress { get; private set; }

        public int StartTimeAddress { get; private set; }

        public int EndTimeAddress { get; private set; }

        public string From { get; set; }
        public string To { get; set; }
    }

    public class CIMCollection
    {
        public List<MaterialPort> MaterialPorts { get; }
        public CIMFunctionViewModel CimFunction { get; }

        private readonly IConfiguration _configuration;
        private int MCCUnitStartWord { get; }
        private int MCCPPIDStartWord { get; }

        private string MccFromToMapFile => _configuration["Files:MCCFromToMapFile"];

        public CIMCollection(IEnumerable<MaterialPort> materialPorts,
            CIMFunctionViewModel cimFunction,
            IConfiguration configuration)
        {
            MaterialPorts = materialPorts.ToList();

            for (int i = 0; i < MaterialPorts.Count(); i++)
            {
                MaterialPorts[i].Id = i + 1;
                MaterialPorts[i].Name = $"Material Port Head {i + 1}";
                MaterialPorts[i].Type = "RESIN";
            }
            MaterialPorts.First().IsCurrentActived = true;
            CimFunction = cimFunction;
            _configuration = configuration;

            try
            {
                object? val = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Top Engineering", "MCCUnitStartWord", "");
                MCCUnitStartWord = int.Parse(val.ToString(), System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                LogManager.GetLogger("IO").Error("Get Value MCCUnitStartWord Fail . Check Registry Value");
                MessageBoxEx.ShowDialog("Get Value MCCUnitStartWord Fail . Check Registry Value");
            }

            try
            {
                object? val = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Top Engineering", "MCCPPIDStartWord", "");
                MCCPPIDStartWord = int.Parse(val.ToString(), System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
                LogManager.GetLogger("IO").Error("Get Value MCCPPIDStartWord Fail . Check Registry Value");
                MessageBoxEx.ShowDialog("Get Value MCCPPIDStartWord Fail . Check Registry Value");
            }
        }

        public bool WriteMCCFromToMap()
        {
            if (File.Exists(MccFromToMapFile) == false)
            {
                return false;
            }

            foreach (var line in File.ReadAllLines(MccFromToMapFile))
            {
                if (string.IsNullOrEmpty(line)) continue;

                string pattern = "^(?<Address>[0-9a-fA-F]{5})\t(?<From>[a-zA-Z]{2}[0-9]{2})\t(?<To>[a-zA-Z]{2}[0-9]{2})$";
                Match match = Regex.Match(line, pattern);

                if (match.Success)
                {
                    CIMMCCActionInfo mcc = new CIMMCCActionInfo()
                    {
                        FromAddress = int.Parse(match.Groups["Address"].ToString(), System.Globalization.NumberStyles.HexNumber),
                        From = match.Groups["From"].ToString(),
                        To = match.Groups["To"].ToString(),

                    };
                    MCCActionMap.Add(mcc);
                }
            }

            if (MCCActionMap.Count != (int)EMCCAction.MAX) return false;

            foreach (CIMMCCActionInfo cIMMCCActionInfo in MCCActionMap)
            {
                CCLinkIEHelper.WriteWords(cIMMCCActionInfo.FromAddress, 2, CIMScenarioDispatcher.EncodeAsciiToWords(cIMMCCActionInfo.From));
                CCLinkIEHelper.WriteWords(cIMMCCActionInfo.ToAddress, 2, CIMScenarioDispatcher.EncodeAsciiToWords(cIMMCCActionInfo.To));
            }

            return true;
        }

        public List<CIMMCCActionInfo> MCCActionMap = new List<CIMMCCActionInfo>();

        public void WriteMCC(EMCCAction lastAction, EMCCAction nextAction)
        {
            DateTime dt = DateTime.Now;

            WriteMCC_OnlyEnd(lastAction, dt);
            WriteMCC_OnlyStart(nextAction, dt);
        }

        public void WriteMCC_OnlyStart(EMCCAction mCCAction, DateTime? dt = null)
        {
            if ((int)mCCAction >= MCCActionMap.Count) return;

            CIMMCCActionInfo cIMMCCActionInfo = MCCActionMap[(int)mCCAction];

            short[] time =
            [
                (short)(dt == null ? DateTime.Now.Hour : dt.Value.Hour),
                (short)(dt == null ? DateTime.Now.Minute : dt.Value.Minute),
                (short)(dt == null ? DateTime.Now.Second : dt.Value.Second),
                (short)(dt == null ? DateTime.Now.Millisecond : dt.Value.Millisecond),
            ];
            CCLinkIEHelper.WriteWords(cIMMCCActionInfo.StartTimeAddress, 4, time);
        }

        public void WriteMCC_OnlyEnd(EMCCAction mCCAction, DateTime? dt = null)
        {
            if ((int)mCCAction >= MCCActionMap.Count) return;

            CIMMCCActionInfo cIMMCCActionInfo = MCCActionMap[(int)mCCAction];

            short[] time =
            [
                (short)(dt == null ? DateTime.Now.Hour : dt.Value.Hour),
                (short)(dt == null ? DateTime.Now.Minute : dt.Value.Minute),
                (short)(dt == null ? DateTime.Now.Second : dt.Value.Second),
                (short)(dt == null ? DateTime.Now.Millisecond : dt.Value.Millisecond),
            ];
            CCLinkIEHelper.WriteWords(cIMMCCActionInfo.EndTimeAddress, 4, time);
        }

        public void WriteMCC_CellID(EMCCUnit unit, string cellID)
        {
            CCLinkIEHelper.WriteWords(MCCUnitStartWord + (int)unit * 46, 20, CIMScenarioDispatcher.EncodeAsciiToWords(cellID));
        }

        public void WriteMCC_PPID(string ppid)
        {
            CCLinkIEHelper.WriteWords(MCCPPIDStartWord, 20, CIMScenarioDispatcher.EncodeAsciiToWords(ppid));
        }
    }
}
