using System;

namespace DEXR.Core.Models
{
    public class TransactionBlockFee : Transaction
    {
        public long TotalFee { get; set; }
        public string ToAddress { get; set; }
    }
}
