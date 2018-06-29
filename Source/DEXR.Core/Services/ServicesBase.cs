using DEXR.Core.Models;
using DEXR.Core.Utility;
using System;
using Newtonsoft.Json;
using System.Linq;
using DEXR.Core.Configuration;
using DEXR.Core.Const;

namespace DEXR.Core.Services
{
    public class ServicesBase
    {
        protected string GenerateTransactionId(Transaction record)
        {
            var json = JsonConvert.SerializeObject(record); ;
            return HashUtility.SHA256(json);
        }
        protected string GenerateUUID()
        {
            return Guid.NewGuid().ToString();
        }

        protected bool IsTransactionInPending(string transactionId)
        {
            var count = ApplicationState.PendingRecords
                .Where(x => x.TransactionId == transactionId)
                .ToList()
                .Count();

            return (count > 0);
        }

        protected long ToFactoredPriceFee(decimal amount)
        {
            IndexServices indexServices = new IndexServices();
            return ToFactoredPrice(amount, indexServices.TokenIndex.GetNative().Decimals);
        }

        protected long ToFactoredPrice(decimal amount, string symbol)
        {
            IndexServices indexServices = new IndexServices();
            return ToFactoredPrice(amount, indexServices.TokenIndex.Get(symbol).Decimals);
        }

        protected long FromFactoredPrice(decimal amount, string symbol)
        {
            IndexServices indexServices = new IndexServices();
            return FromFactoredPrice(amount, indexServices.TokenIndex.Get(symbol).Decimals);
        }

        protected long ToFactoredPrice(decimal amount, short decimals)
        {
            var factors = (long)Math.Pow(10, decimals);
            return (long)(amount * factors);
        }

        protected long FromFactoredPrice(decimal amount, short decimals)
        {
            var factors = (long)Math.Pow(10, decimals);
            return (long)(amount / factors);
        }

        protected bool HasSufficientBalanceFee(string address, decimal amount)
        {
            IndexServices indexServices = new IndexServices();
            return HasSufficientBalance(address, indexServices.TokenIndex.GetNative().Symbol, amount);
        }

        protected bool HasSufficientBalance(string address, string symbol, decimal amount)
        {
            if (amount <= 0)
                return true;

            IndexServices indexServices = new IndexServices();
            var factoredPrice = ToFactoredPrice(amount, indexServices.TokenIndex.Get(symbol).Decimals);
            
            //Verify account balance
            var senderWallet = indexServices.BalanceIndex.Get(address, symbol);
            if (senderWallet == null || senderWallet.Balance < factoredPrice)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
