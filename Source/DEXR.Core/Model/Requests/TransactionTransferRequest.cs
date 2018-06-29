using System;

namespace DEXR.Core.Model.Requests
{
    public class TransactionTransferRequest
    {
        public TransactionTransferRequestBody Body { get; set; }
        public string Signature { get; set; }
    }
    public class TransactionTransferRequestBody
    {
        public long Nonce { get; set; }
        public string Fee { get; set; }

        public string TokenSymbol { get; set; }
        public string Sender { get; set; }
        public string ToAddress { get; set; }
        public string Amount { get; set; }
    }
}
