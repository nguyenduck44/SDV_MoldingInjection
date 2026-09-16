using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_MoldingInjection.Defines
{
    public class Config
    {
		private bool isConnectEBoxMainPower;

        public bool IsConnectEBoxMainPower
        {
            get => isConnectEBoxMainPower;
            set
            {
                isConnectEBoxMainPower = value;
            }
        }

        public Config(bool isConnectEBoxMainPower)
        {
            IsConnectEBoxMainPower = isConnectEBoxMainPower;
        }
    }
}
