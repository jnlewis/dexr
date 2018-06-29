using System;

namespace DEXR.Core.Model.Requests
{
    public class TransactionOrderLimitRequest
    {
        public TransactionOrderLimitRequestBody Body { get; set; }
        public string Signature { get; set; }
    }
    public class TransactionOrderLimitRequestBody
    {
        public long Nonce { get; set; }
        public string Fee { get; set; }

        public string PairSymbol { get; set; }
        public string Side { get; set; }
        public string Price { get; set; }
        public string Amount { get; set; }
        public int ExpiryBlocks { get; set; }
        public string Owner { get; set; }
    }
}
