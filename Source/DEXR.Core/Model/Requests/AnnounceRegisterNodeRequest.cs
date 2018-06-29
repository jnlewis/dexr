using System;

namespace DEXR.Core.Model.Requests
{
    public class AnnounceRegisterNodeRequest
    {
        public string ServerAddress { get; set; }

        public string WalletAddress { get; set; }

        public string Signature { get; set; }
    }
}
