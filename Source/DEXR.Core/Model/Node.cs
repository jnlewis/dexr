using System;

namespace DEXR.Core.Model
{
    public class Node
    {
        public string ServerAddress { get; set; }
        public string WalletAddress { get; set; }
        public string Signature { get; set; }
        public decimal Balance { get; set; }
    }
}
