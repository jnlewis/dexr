using System;
using System.Collections.Generic;
using System.Linq;
using DEXR.Core.Const;
using DEXR.Core.Models;

namespace DEXR.Core.Data.Index
{
    public class IndexBalance
    {
        private Database database;
        private string nativeToken;

        public IndexBalance(Database db, string nativeTokenSymbol)
        {
            database = db;
            nativeToken = nativeTokenSymbol;
        }

        public void AddToIndex(TransactionTransfer record)
        {
            if (record == null)
                return;
            
            UpdateBalanceIndex(record.FromAddress, record.ToAddress, record.TokenSymbol, record.Amount);

            //Update fee holding balance
            if (record.Fee > 0)
            {
                UpdateBalanceIndex(record.FromAddress, ReservedAddresses.FeeWallet, nativeToken, record.Fee);
            }
        }

        public void AddToFeeIndex(TransactionBlockFee record)
        {
            UpdateBalanceIndex(ReservedAddresses.FeeWallet, record.ToAddress, nativeToken, record.TotalFee);
        }

        private void UpdateBalanceIndex(string fromAddress, string toAddress, string tokenSymbol, long amount)
        {
            if (tokenSymbol == null)
                return;

            //Create sender index
            CreateIndexIfNotExist(fromAddress, tokenSymbol);

            //Create recipient index
            CreateIndexIfNotExist(toAddress, tokenSymbol);

            //Update sender balance
            if (fromAddress != null)
            {
                BalanceIndexItem index = database.GetBalanceIndex(fromAddress, tokenSymbol);
                index.Balance = index.Balance - amount;
                database.SaveBalanceIndex(index);
                IndexBalanceCache.AddOrUpdate(index);
            }

            //Update recipient balance
            if (toAddress != null)
            {
                BalanceIndexItem index = database.GetBalanceIndex(toAddress, tokenSymbol);
                index.Balance = index.Balance + amount;
                database.SaveBalanceIndex(index);
                IndexBalanceCache.AddOrUpdate(index);
            }
        }
        
        private void CreateIndexIfNotExist(string address, string tokenSymbol)
        {
            if (address != null)
            {
                var index = database.GetBalanceIndex(address, tokenSymbol);
                if (index == null)
                {
                    index = new BalanceIndexItem();
                    index.TokenSymbol = tokenSymbol;
                    index.Balance = 0;
                    index.Address = address;

                    database.SaveBalanceIndex(index);
                    IndexBalanceCache.AddOrUpdate(index);
                }
            }
        }

        public void DeleteIndex(TransactionTransfer record)
        {
            database.DeleteBalanceIndex(record.FromAddress, record.TokenSymbol);
            database.DeleteBalanceIndex(record.ToAddress, record.TokenSymbol);

            IndexBalanceCache.Remove(record.FromAddress, record.TokenSymbol);
            IndexBalanceCache.Remove(record.ToAddress, record.TokenSymbol);
        }

        public BalanceIndexItem Get(string address, string token)
        {
            var cachedIndex = IndexBalanceCache.Get(address, token);
            if (cachedIndex != null)
                return cachedIndex;

            var index = database.GetBalanceIndex(address, token);
            if (index != null)
                IndexBalanceCache.AddOrUpdate(index);
            return index;
        }
    }

    public class BalanceIndexItem
    {
        public string Address { get; set; }
        public string TokenSymbol { get; set; }
        public long Balance { get; set; }
    }

    public static class IndexBalanceCache
    {
        private static List<BalanceIndexItem> cache;

        public static void AddOrUpdate(BalanceIndexItem item)
        {
            if (cache == null)
            {
                cache = new List<BalanceIndexItem>();
                cache.Add(item);
            }
            else
            {
                var find = cache.Where(x => 
                    x.Address == item.Address && 
                    x.TokenSymbol == item.TokenSymbol)
                    .FirstOrDefault();

                if(find == null)
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

        public static BalanceIndexItem Get(string address, string tokenSymbol)
        {
            if (cache == null)
                return null;

            return cache.Where(x => 
                x.Address == address && 
                x.TokenSymbol == tokenSymbol)
                .FirstOrDefault();
        }

        public static void Remove(string address, string tokenSymbol)
        {
            var find = cache.Where(x =>
                x.Address == address &&
                x.TokenSymbol == tokenSymbol)
                .FirstOrDefault();

            if (find != null)
            {
                cache.Remove(find);
            }
        }
    }
}
