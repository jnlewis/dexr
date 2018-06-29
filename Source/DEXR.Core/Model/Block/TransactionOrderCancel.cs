using System;

namespace DEXR.Core.Models
{
    public class TransactionOrderCancel : Transaction
    {
        public string OrderTransactionId { get; set; }
        public string Owner { get; set; }
    }
}
