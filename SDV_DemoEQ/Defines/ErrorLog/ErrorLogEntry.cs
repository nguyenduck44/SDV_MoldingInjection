using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SDV_DemoEQ.Defines.ErrorLog
{
    public class ErrorLogEntry
    {
        public string Type { get; set; }
        public string Timestamp { get; set; }
        public int ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
