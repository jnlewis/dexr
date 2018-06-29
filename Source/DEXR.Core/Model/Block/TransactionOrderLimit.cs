using System;

namespace DEXR.Core.Models
{
    public class TransactionOrderLimit : Transaction
    {
        public string TradingPair { get; set; }
        public string Side { get; set; }
        public decimal Price { get; set; }
        public decimal Amount { get; set; }
        public int ExpiryBlockIndex { get; set; }
        public string Owner { get; set; }
    }
}
