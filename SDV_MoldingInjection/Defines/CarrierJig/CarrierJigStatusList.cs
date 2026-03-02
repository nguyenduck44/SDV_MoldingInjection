using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines
{
    public class CarrierJigStatusList
    {
        public CarrierJigStatusList()
        {
            CarrierJigStatusH1 = new CarrierJigStatus { Name = "H1" };
            CarrierJigStatusH2 = new CarrierJigStatus { Name = "H2" };
            CarrierJigStatusH3 = new CarrierJigStatus { Name = "H3" };
            CarrierJigStatusH4 = new CarrierJigStatus { Name = "H4" };
        }

        public CarrierJigStatus CarrierJigStatusH1 { get; }
        public CarrierJigStatus CarrierJigStatusH2 { get; }
        public CarrierJigStatus CarrierJigStatusH3 { get; }
        public CarrierJigStatus CarrierJigStatusH4 { get; }
    }
}
