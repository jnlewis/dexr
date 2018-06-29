using System;

namespace DEXR.Core.Models
{
    public class TransactionOrderMarket : Transaction
    {
        public string TradingPair { get; set; }
        public string Side { get; set; }
        public decimal Amount { get; set; }
        public string Owner { get; set; }
    }
}
