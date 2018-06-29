using System;

namespace DEXR.Core.Models
{
    public class TransactionToken : Transaction
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public short Decimals { get; set; }
    }
}
