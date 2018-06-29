using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.UnitTest
{
    public class StressTestNode
    {
        public StressTestNode()
        {
            Status = "Stopped";
        }

        public string Status { get; set; }
        public string ApplicationFolder { get; set; }
        public int Port { get; set; }
        public System.Diagnostics.Process Process { get; set; }
    }
}
