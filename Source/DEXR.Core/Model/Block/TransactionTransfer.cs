using System;

namespace DEXR.Core.Models
{
    public class TransactionTransfer : Transaction
    {
        public string TokenSymbol { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public long Amount { get; set; }
    }
}
