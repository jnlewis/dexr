using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.Core.Model
{
    public class TradeMatch
    {
        public string TransactionId { get; set; }
        public string Address { get; set; }
        public string TradingPair { get; set; }
        public string Side { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public string PartialOrFull { get; set; }
    }
}
