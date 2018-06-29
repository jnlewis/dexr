using System;
using System.Collections.Generic;

namespace DEXR.Core.Models
{
    public class Block
    {
        public BlockHeader Header { get; set; }
        public List<Transaction> Transactions { get; set; }
    }

    public class BlockHeader
    {
        public int Index { get; set; }
        public int Timestamp { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public int TransactionCount { get; set; }
        public string LastTransactionId { get; set; }
    }

    public class Transaction
    {
        public long Nonce { get; set; }
        public string Signature { get; set; }
        public string TransactionId { get; set; }
        public long Fee { get; set; }
        public string Type { get; set; }
    }
}
