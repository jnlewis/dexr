using DEXR.Core.Configuration;
using DEXR.Core.Const;
using DEXR.Core.Model;
using DEXR.Core.Models;
using DEXR.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DEXR.Core.Services
{
    public class TradeServices : ServicesBase
    {
        IndexServices indexServices;

        public TradeServices()
        {
            indexServices = new IndexServices();
        }

        public void ValidateOrder(string tradingPair, string side, string owner, decimal amount, decimal price)
        {
            /*
            Example: BTC/NEO
            Buy:- Buying BTC selling NEO, must have available NEO to exchange for the BTC amount to buy multiplied by price
            Sell:- Selling BTC buying NEO, must have available BTC to exchange
             */
            
            string buyingToken = null;
            string sellingToken = null;

            if (side == TradeSides.Buy)
            {
                buyingToken = tradingPair.Split('/')[0];
                sellingToken = tradingPair.Split('/')[1];

                //Verify account balance
                if (!HasSufficientBalance(owner, sellingToken, amount * price))
                {
                    throw new ValidationException(ResponseCodes.InsufficientFunds, ResponseMessages.InsufficientFunds);
                }
            }
            else if (side == TradeSides.Sell)
            {
                buyingToken = tradingPair.Split('/')[1];
                sellingToken = tradingPair.Split('/')[0];

                //Verify account balance
                if (!HasSufficientBalance(owner, sellingToken, amount))
                {
                    throw new ValidationException(ResponseCodes.InsufficientFunds, ResponseMessages.InsufficientFunds);
                }
            }
        }

        public decimal? GetMarketPrice(string tradingPair, string side)
        {
            IndexServices indexServices = new IndexServices();
            var orders = indexServices.OrderIndex.GetByTradingPair(tradingPair);
            if (orders != null)
            {
                orders = orders.Where(x => x.Side == side).ToList();
                if (orders.Count == 0)
                    return null;

                if (side == TradeSides.Buy)
                {
                    return orders.Min(x => x.Price);
                }
                else if (side == TradeSides.Sell)
                {
                    return orders.Max(x => x.Price);
                }
                else
                {
                    throw new ArgumentException("GetMarketPrice: Parameter side not recognized: " + side);
                }
            }
            else
            {
                return null;
            }
        }

        public List<string> MakeTrade(string transactionId, string tradingPair, string side, decimal amount, string owner)
        {
            List<string> transactionIds = new List<string>();

            //FindTradeMatch
            var tradeMatches = FindMatchedOrdersByAvailableBalance(tradingPair, side, amount, owner);
            
            //Prepare to make transactions
            string ownerSendingToken = null;
            string ownerReceivingToken = null;
            if (side == TradeSides.Buy)
            {
                ownerReceivingToken = tradingPair.Split('/')[0];
                ownerSendingToken = tradingPair.Split('/')[1];

                //Perform transactions
                TransactionServices transactionService = new TransactionServices();
                foreach (var trade in tradeMatches)
                {
                    //Transfer from owner to matcher
                    string txId1 = transactionService.AddTransfer(
                        DateTimeUtility.CurrentUnixTimeUTC(),
                        0,
                        ownerSendingToken,
                        owner,
                        trade.Address,
                        trade.Amount * trade.Price);

                    //Transfer from matcher to owner
                    string txId2 = transactionService.AddTransfer(
                        DateTimeUtility.CurrentUnixTimeUTC(),
                        0,
                        ownerReceivingToken,
                        trade.Address,
                        owner,
                        trade.Amount);

                    //Add trade match record (For audit record purpose)
                    string txMatch = transactionService.AddTradeMatch(
                        DateTimeUtility.CurrentUnixTimeUTC(),
                        side,
                        tradingPair,
                        trade.Amount,
                        trade.Price,
                        trade.TransactionId,
                        trade.Address,
                        transactionId,
                        owner,
                        trade.PartialOrFull);

                    transactionIds.Add(txMatch);
                }

            }
            else if (side == TradeSides.Sell)
            {
                ownerReceivingToken = tradingPair.Split('/')[1];
                ownerSendingToken = tradingPair.Split('/')[0];

                //Perform transactions
                TransactionServices transactionService = new TransactionServices();
                foreach (var trade in tradeMatches)
                {
                    //Transfer from owner to matcher
                    string txId1 = transactionService.AddTransfer(
                        DateTimeUtility.CurrentUnixTimeUTC(),
                        0,
                        ownerSendingToken,
                        owner,
                        trade.Address,
                        trade.Amount);

                    //Transfer from matcher to owner
                    string txId2 = transactionService.AddTransfer(
                        DateTimeUtility.CurrentUnixTimeUTC(),
                        0,
                        ownerReceivingToken,
                        trade.Address,
                        owner,
                        trade.Amount * trade.Price);

                    //Add trade match record (For audit record purpose)
                    string txMatch = transactionService.AddTradeMatch(
                        DateTimeUtility.CurrentUnixTimeUTC(),
                        side,
                        tradingPair,
                        trade.Amount,
                        trade.Price,
                        trade.TransactionId,
                        trade.Address,
                        transactionId,
                        owner,
                        trade.PartialOrFull);

                    transactionIds.Add(txMatch);
                }

            }

            return transactionIds;
        }

        private List<TradeMatch> FindMatchedOrdersByAvailableBalance(string tradingPair, string side, decimal amount, string owner)
        {
            List<TradeMatch> result = new List<TradeMatch>();

            //Find all matches
            List<TradeMatch> matches = FindAllMatchedOrders(tradingPair, side, amount);

            //Filter out matches based on owner's available balance
            if (side == TradeSides.Buy)
            {
                //Get owner balance
                var token = tradingPair.Split('/')[1];
                IndexServices indexServices = new IndexServices();
                var balance = indexServices.BalanceIndex.Get(owner, token);

                decimal balanceValue = balance.Balance;

                int i = 0;
                bool lookForNextMatch = true;
                while (lookForNextMatch)
                {
                    //Exit if no more orders available
                    if (i >= matches.Count)
                    {
                        lookForNextMatch = false;
                        break;
                    }

                    var trade = matches[i];
                    var factoredPrice = ToFactoredPrice(trade.Price, token);
                    
                    if (balanceValue >= (trade.Amount * factoredPrice))
                    {
                        //Full
                        result.Add(trade);
                    }
                    else
                    {
                        //Partial
                        trade.Amount = Math.Floor(balanceValue / factoredPrice);
                        trade.PartialOrFull = TradePartialFull.Partial;
                        result.Add(trade);
                        lookForNextMatch = false;
                    }

                    balanceValue = balanceValue - (trade.Amount * factoredPrice);
                    i++;
                }
            }
            else if (side == TradeSides.Sell)
            {
                result = matches;
            }
            else
            {
                throw new ArgumentException();
            }

            return result;
        }

        //TODO: for IoC orders
        //FindMatchedOrdersByGivenBalance
        
        /// <summary>
        /// Find all trades that match for this order and returns the list of matched orders
        /// </summary>
        private List<TradeMatch> FindAllMatchedOrders(string tradingPair, string side, decimal amount)
        {
            List<TradeMatch> result = new List<TradeMatch>();

            decimal balanceAmount = amount;

            IndexServices indexServices = new IndexServices();
            var orders = indexServices.OrderIndex.GetByTradingPair(tradingPair);

            if (orders == null || orders.Count == 0)
                throw new ValidationException(ResponseCodes.NotEnoughOrdersAvailable, ResponseMessages.NotEnoughOrdersAvailable);

            if (side == TradeSides.Buy)
            {
                //Get sell orders sorted by lowest price
                var sellOrders = orders
                    .Where(x => x.Side == TradeSides.Sell)
                    .OrderBy(x => x.Price)
                    .ToList();

                int i = 0;
                bool lookForNextMatch = true;
                while (lookForNextMatch)
                {
                    //Exit if no more orders available
                    if (i >= sellOrders.Count ||
                        balanceAmount == 0)
                    {
                        lookForNextMatch = false;
                        break;
                    }

                    var currentOrder = sellOrders[i];

                    //Check full/partial
                    if (balanceAmount >= currentOrder.Amount)
                    {
                        result.Add(new TradeMatch()
                        {
                            TransactionId = currentOrder.TransactionId,
                            Address = currentOrder.Owner,
                            TradingPair = tradingPair,
                            Side = currentOrder.Side,
                            Amount = currentOrder.Amount,
                            Price = currentOrder.Price,
                            PartialOrFull = TradePartialFull.Full
                        });

                        balanceAmount = balanceAmount - currentOrder.Amount;
                        i++;
                    }
                    else
                    {
                        result.Add(new TradeMatch()
                        {
                            TransactionId = currentOrder.TransactionId,
                            Address = currentOrder.Owner,
                            TradingPair = tradingPair,
                            Side = currentOrder.Side,
                            Amount = balanceAmount,
                            Price = currentOrder.Price,
                            PartialOrFull = TradePartialFull.Partial
                        });

                        lookForNextMatch = false;
                        balanceAmount = 0;
                        i++;
                    }
                }
            }
            else if (side == TradeSides.Sell)
            {
                //Get buy orders sorted by highest price
                var buyOrders = orders
                    .Where(x => x.Side == TradeSides.Buy)
                    .OrderByDescending(x => x.Price)
                    .ToList();

                int i = 0;
                bool lookForNextMatch = true;
                while (lookForNextMatch)
                {
                    var currentOrder = buyOrders[i];

                    //Check if there is anymore order
                    if (i >= buyOrders.Count)
                    {
                        lookForNextMatch = false;
                        break;
                    }

                    //Check full/partial
                    if (balanceAmount > currentOrder.Amount)
                    {
                        result.Add(new TradeMatch()
                        {
                            TransactionId = currentOrder.TransactionId,
                            Address = currentOrder.Owner,
                            TradingPair = tradingPair,
                            Side = currentOrder.Side,
                            Amount = currentOrder.Amount,
                            Price = currentOrder.Price,
                            PartialOrFull = TradePartialFull.Full
                        });

                        balanceAmount = balanceAmount - currentOrder.Amount;
                        i++;
                    }
                    else
                    {
                        result.Add(new TradeMatch()
                        {
                            TransactionId = currentOrder.TransactionId,
                            Address = currentOrder.Owner,
                            TradingPair = tradingPair,
                            Side = currentOrder.Side,
                            Amount = balanceAmount,
                            Price = currentOrder.Price,
                            PartialOrFull = TradePartialFull.Partial
                        });

                        lookForNextMatch = false;
                        balanceAmount = 0;
                        i++;
                    }
                }
            }
            else
            {
                throw new ArgumentException();
            }

            return result;
        }

    }
}
