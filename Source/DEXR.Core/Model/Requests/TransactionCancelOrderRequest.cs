using System;

namespace DEXR.Core.Model.Requests
{
    public class TransactionCancelOrderRequest
    {
        public TransactionCancelOrderRequestBody Body { get; set; }
        public string Signature { get; set; }
    }
    public class TransactionCancelOrderRequestBody
    {
        public long Nonce { get; set; }
        public string Fee { get; set; }

        public string OrderTransactionId { get; set; }
        public string Owner { get; set; }
    }
}
