using System;

namespace DEXR.Core.Model.Requests
{
    public class TransactionOrderMarketRequest
    {
        public TransactionOrderMarketRequestBody Body { get; set; }
        public string Signature { get; set; }
    }
    public class TransactionOrderMarketRequestBody
    {
        public long Nonce { get; set; }
        public string Fee { get; set; }

        public string PairSymbol { get; set; }
        public string Side { get; set; }
        public string Amount { get; set; }
        public string Owner { get; set; }
    }
}
