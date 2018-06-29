using System;

namespace DEXR.CLI.Model
{
    public class Wallet
    {
        public string Address { get; set; }
        public string PrivateKey { get; set; }
        public string Signature { get; set; }
    }
}
