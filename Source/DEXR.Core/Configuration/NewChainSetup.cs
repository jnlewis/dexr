
using System;
using System.Collections.Generic;
using System.Text;

namespace DEXR.Core.Configuration
{
    internal class NewChainSetup
    {
        public string NativeTokenName { get; set; }
        public string NativeTokenSymbol { get; set; }
        public long InitialSupply { get; set; }
        public short Decimals { get; set; }
    }
}
