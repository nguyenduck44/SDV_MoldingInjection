using EQX.Core.InOut;
using EQX.UI.Controls;
using log4net;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines
{
    public class Information
    {
        public string Customer => "SDV";
        public string MachineName => "Resin Mold Inject";
        public string SoftwareVersion => "1.0.0.1";
        public string EQPID { get; set; }
        public Information()
        {
#if !SIMULATION
            try
            {
                object? eQPID = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Top Engineering", "EQPID", "B1BME01_DP01");
                EQPID = eQPID.ToString();
            }
            catch
            {
                LogManager.GetLogger("IO").Error("Get Value SensorStartBit Fail . Check Registry Value");
                EQPID = "B1BME01_DP01";
            }
#endif
        }
    }
}
