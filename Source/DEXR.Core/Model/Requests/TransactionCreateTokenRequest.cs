using System;

namespace DEXR.Core.Model.Requests
{
    public class TransactionCreateTokenRequest
    {
        public TransactionCreateTokenRequestBody Body { get; set; }
        public string Signature { get; set; }
    }
    public class TransactionCreateTokenRequestBody
    {
        public long Nonce { get; set; }
        public string Fee { get; set; }

        public string Symbol { get; set; }
        public string Name { get; set; }
        public long TotalSupply { get; set; }
        public short Decimals { get; set; }
        public string Owner { get; set; }
    }
}
