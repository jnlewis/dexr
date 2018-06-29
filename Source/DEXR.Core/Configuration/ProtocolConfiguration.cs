using System;
using System.Collections.Generic;
using System.Text;

namespace DEXR.Core.Configuration
{
    public class ProtocolConfiguration
    {
        public string ChainFolderName { get; set; }
        public string ServerHostType { get; set; }
        public int ServerPort { get; set; }
        public List<string> SeedList { get; set; }
    }
}
