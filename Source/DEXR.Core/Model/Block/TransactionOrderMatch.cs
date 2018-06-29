using System;

namespace DEXR.Core.Models
{
    public class TransactionOrderMatch : Transaction
    {
        public string TradingPair { get; set; }
        public string Side { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public string PartialOrFull { get; set; }
        public string OrderTransactionId { get; set; }
        public string OrderAddress { get; set; }
        public string TakerTransactionId { get; set; }
        public string TakerAddress { get; set; }
    }
}
