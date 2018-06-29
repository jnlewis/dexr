using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.Core.Model
{
    public class Order
    {
        public string TransactionId { get; set; }
        public string Symbol { get; set; }
        public string Side { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }
}
