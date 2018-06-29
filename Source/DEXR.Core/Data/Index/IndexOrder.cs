using DEXR.Core.Const;
using DEXR.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DEXR.Core.Data.Index
{
    public class IndexOrder
    {
        private Database database;

        public IndexOrder(Database db)
        {
            database = db;
        }

        public void AddToIndex(TransactionOrderLimit record)
        {
            if (record == null)
                return;
            
            var orderIndex = new OrderIndexItem()
            {
                TransactionId = record.TransactionId,
                Owner = record.Owner,
                TradingPair = record.TradingPair,
                Side = record.Side,
                Amount = record.Amount,
                Price = record.Price
            };
            
            database.SaveOrderIndex(orderIndex);
            IndexOrderCache.AddOrUpdate(orderIndex);

            database.AddToKeyMap("orders_" + record.TradingPair, record.TransactionId);
            database.AddToKeyMap("orders_expiry_" + record.ExpiryBlockIndex, record.TransactionId);
        }

        public List<OrderIndexItem> GetByTradingPair(string tradingPair)
        {
            List<OrderIndexItem> result = new List<OrderIndexItem>();

            List<string> orderTransactionIds = database.GetKeyMap("orders_" + tradingPair);
            if(orderTransactionIds != null)
            {
                foreach (var orderTransactionId in orderTransactionIds)
                {
                    var item = Get(orderTransactionId);
                    if(item != null)
                        result.Add(item);
                }
            }

            return result;
        }
        
        public OrderIndexItem Get(string transactionId)
        {
            var cachedIndex = IndexOrderCache.Get(transactionId);
            if (cachedIndex != null)
                return cachedIndex;

            var index = database.GetOrderIndex(transactionId);
            if (index != null)
                IndexOrderCache.AddOrUpdate(index);
            return index;
        }

        public void UpdateIndexForMatchedOrder(TransactionOrderMatch record)
        {
            if (record.PartialOrFull == TradePartialFull.Full)
            {
                DeleteSingleTransaction(record.OrderTransactionId);
            }
            else if (record.PartialOrFull == TradePartialFull.Partial)
            {
                var orderIndex = new OrderIndexItem()
                {
                    TransactionId = record.OrderTransactionId,
                    Owner = record.OrderAddress,
                    TradingPair = record.TradingPair,
                    Side = record.Side,
                    Amount = record.Amount,
                    Price = record.Price
                };

                UpdateIndex(orderIndex);
            }
        }

        private void UpdateIndex(OrderIndexItem orderIndex)
        {
            database.SaveOrderIndex(orderIndex);
            IndexOrderCache.AddOrUpdate(orderIndex);
        }

        public void DeleteIndex(TransactionOrderLimit record)
        {
            database.DeleteOrderIndex(record.TransactionId);
            database.DeleteKeyMap("orders_" + record.TradingPair);
            IndexOrderCache.Remove(record.TransactionId);
        }

        public void DeleteSingleTransaction(string transactionId)
        {
            var item = Get(transactionId);

            database.DeleteOrderIndex(transactionId);
            IndexOrderCache.Remove(transactionId);

            database.UpdateToKeyMap("orders_" + item.TradingPair, transactionId);
        }

        public void DeleteExpiredOrders(int blockIndex)
        {
            var expiredOrdersTxIds = database.GetKeyMap("orders_expiry_" + blockIndex);
            if(expiredOrdersTxIds != null)
            {
                foreach (var txId in expiredOrdersTxIds)
                {
                    DeleteSingleTransaction(txId);
                }
            }
        }
    }

    public class OrderIndexItem
    {
        public string TransactionId { get; set; }
        public string Owner { get; set; }
        public string TradingPair { get; set; }
        public string Side { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
    }

    public static class IndexOrderCache
    {
        private static List<OrderIndexItem> cache;

        public static void AddOrUpdate(OrderIndexItem item)
        {
            if (cache == null)
            {
                cache = new List<OrderIndexItem>();
                cache.Add(item);
            }
            else
            {
                var find = cache.Where(x =>
                    x.TransactionId == item.TransactionId)
                    .FirstOrDefault();

                if (find == null)
                {
                    cache.Add(item);
                }
                else
                {
                    cache.Remove(find);
                    cache.Add(item);
                }
            }
        }

        public static OrderIndexItem Get(string transactionId)
        {
            if (cache == null)
                return null;

            return cache.Where(x =>
                    x.TransactionId == transactionId)
                    .FirstOrDefault();
        }

        public static void Remove(string transactionId)
        {
            var find = cache.Where(x =>
                    x.TransactionId == transactionId)
                    .FirstOrDefault();

            if (find != null)
            {
                cache.Remove(find);
            }
        }
    }
}
