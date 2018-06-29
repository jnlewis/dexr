using DEXR.Core.Configuration;
using DEXR.Core.Const;
using DEXR.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace DEXR.Core.Services
{
    public class TransactionServices : ServicesBase
    {
        IndexServices indexServices;

        public TransactionServices()
        {
            indexServices = new IndexServices();
        }

        public string AddToken(
            long nonce,
            decimal fee,
            string symbol,
            string name,
            long totalSupply,
            short decimals,
            string owner)
        {
            TransactionToken token = new TransactionToken();
            token.Nonce = nonce;
            token.Fee = ToFactoredPriceFee(fee);
            token.Symbol = symbol;
            token.Name = name;
            token.Decimals = decimals;
            token.TransactionId = GenerateTransactionId(token);

            //Check if same transaction has already been received
            if (IsTransactionInPending(token.TransactionId))
            {
                return token.TransactionId;
            }

            ////Verify if sender if owner of block and have permission to create tokens
            //var chainOriginator = indexServices.OwnerIndex.Get();
            //if(chainOriginator.Address != sender)
            //{
            //    throw new ValidationException(ResponseCodes.InsufficientPermission, ResponseMessages.InsufficientPermission);
            //}

            //Verify fee account balance
            if (!HasSufficientBalanceFee(owner, fee))
            {
                throw new ValidationException(ResponseCodes.InsufficientFundsForFee, ResponseMessages.InsufficientFundsForFee);
            }

            //Check if token already exist
            if (indexServices.TokenIndex.Get(symbol) != null)
            {
                throw new ValidationException(ResponseCodes.TokenAlreadyExists, ResponseMessages.TokenAlreadyExists);
            }

            TransactionTransfer transfer = new TransactionTransfer();
            transfer.Nonce = nonce;
            transfer.TokenSymbol = symbol;
            transfer.FromAddress = null;
            transfer.ToAddress = owner;
            transfer.Amount = ToFactoredPrice(totalSupply, decimals);
            transfer.TransactionId = GenerateTransactionId(transfer);

            //Check if same transaction has already been received
            if (IsTransactionInPending(transfer.TransactionId))
            {
                return token.TransactionId;
            }
            
            ApplicationState.PendingRecordsAdd(token);
            ApplicationState.PendingRecordsAdd(transfer);

            return token.TransactionId;
        }

        public string AddTransfer(
            long nonce,
            decimal fee,
            string tokenSymbol,
            string fromAddress,
            string toAddress,
            decimal amount)
        {
            var token = indexServices.TokenIndex.Get(tokenSymbol);
            if (token == null)
                throw new ValidationException(ResponseCodes.TokenNotRecognized, ResponseMessages.TokenNotRecognized);

            //Get the non-decimal amount
            var factoredAmount = ToFactoredPrice(amount, token.Decimals);

            TransactionTransfer transfer = new TransactionTransfer();
            transfer.Nonce = nonce;
            transfer.Fee = ToFactoredPriceFee(fee);
            transfer.TokenSymbol = tokenSymbol;
            transfer.FromAddress = fromAddress;
            transfer.ToAddress = toAddress;
            transfer.Amount = factoredAmount;
            transfer.TransactionId = GenerateTransactionId(transfer);

            //Check if same transaction has already been received
            if (IsTransactionInPending(transfer.TransactionId))
            {
                return transfer.TransactionId;
            }

            //Verify fee account balance
            if (!HasSufficientBalanceFee(fromAddress, fee))
            {
                throw new ValidationException(ResponseCodes.InsufficientFundsForFee, ResponseMessages.InsufficientFundsForFee);
            }

            //Verify account balance
            if (!HasSufficientBalance(fromAddress, tokenSymbol, amount))
            {
                throw new ValidationException(ResponseCodes.InsufficientFunds, ResponseMessages.InsufficientFunds);
            }

            ApplicationState.PendingRecordsAdd(transfer);

            return transfer.TransactionId;
        }

        public string AddLimitOrder(
            long nonce,
            decimal fee,
            string tradingPair,
            string side,
            decimal price,
            decimal amount,
            int expiryBlocks,
            string owner)
        {
            TradeServices tradeServices = new TradeServices();
            DataServices dataServices = new DataServices();

            tradeServices.ValidateOrder(tradingPair, side, owner, amount, price);

            TransactionOrderLimit order = new TransactionOrderLimit();
            order.Nonce = nonce;
            order.TradingPair = tradingPair;
            order.Side = side;
            order.Price = price;
            order.Amount = amount;
            order.ExpiryBlockIndex = dataServices.LastBlock.Header.Index + expiryBlocks;
            order.Owner = owner;
            order.TransactionId = GenerateTransactionId(order);

            //Check if same transaction has already been received
            if (IsTransactionInPending(order.TransactionId))
            {
                return order.TransactionId;
            }

            //Verify fee account balance
            if (!HasSufficientBalanceFee(owner, fee))
            {
                throw new ValidationException(ResponseCodes.InsufficientFundsForFee, ResponseMessages.InsufficientFundsForFee);
            }

            ApplicationState.PendingRecordsAdd(order);

            return order.TransactionId;
        }

        public List<string> AddMarketOrder(
            long nonce,
            decimal fee,
            string tradingPair,
            string side,
            decimal amount,
            string owner)
        {
            //Verify fee account balance
            if (!HasSufficientBalanceFee(owner, fee))
            {
                throw new ValidationException(ResponseCodes.InsufficientFundsForFee, ResponseMessages.InsufficientFundsForFee);
            }

            TradeServices tradeServices = new TradeServices();

            var marketLookupSide = side == TradeSides.Buy ? TradeSides.Sell : TradeSides.Buy;
            var marketPrice = tradeServices.GetMarketPrice(tradingPair, marketLookupSide);
            if(marketPrice == null)
            {
                throw new ValidationException(ResponseCodes.NotEnoughOrdersAvailable, ResponseMessages.NotEnoughOrdersAvailable);
            }

            tradeServices.ValidateOrder(tradingPair, side, owner, amount, marketPrice.Value);
            
            //Generate transaction Id
            TransactionOrderMarket order = new TransactionOrderMarket();
            order.Nonce = nonce;
            order.TradingPair = tradingPair;
            order.Side = side;
            order.Amount = amount;
            order.Owner = owner;
            order.TransactionId = GenerateTransactionId(order);
            
            //Make the trade
            var transactionIds = tradeServices.MakeTrade(order.TransactionId, tradingPair, side, amount, owner);

            return transactionIds;
        }

        public string CancelOrder(
            long nonce,
            decimal fee,
            string orderTransactionId,
            string owner)
        {
            //Verify fee account balance
            if (!HasSufficientBalanceFee(owner, fee))
            {
                throw new ValidationException(ResponseCodes.InsufficientFundsForFee, ResponseMessages.InsufficientFundsForFee);
            }

            //Check if the cancelling transaction belongs to this owner
            IndexServices indexServices = new IndexServices();
            var orderToCancel = indexServices.OrderIndex.Get(orderTransactionId);
            if(orderToCancel.Owner != owner)
            {
                throw new ValidationException(ResponseCodes.OrderDoesNotBelongToRequester, ResponseMessages.OrderDoesNotBelongToRequester);
            }

            TransactionOrderCancel order = new TransactionOrderCancel();
            order.Nonce = nonce;
            order.OrderTransactionId = orderTransactionId;
            order.Owner = owner;
            order.TransactionId = GenerateTransactionId(order);

            //Check if same transaction has already been received
            if (IsTransactionInPending(order.TransactionId))
            {
                return order.TransactionId;
            }
            
            //Delete from index so new orders does not match with this cancelled order
            indexServices.OrderIndex.DeleteSingleTransaction(orderTransactionId);

            //Note: Do not remove the cancelled order from pending records, otherwise the order would not go into next block
            //hence this transaction is unable to tie back to the originally cancelled order in the block


            ApplicationState.PendingRecordsAdd(order);

            return order.TransactionId;
        }

        public string AddTradeMatch(
            long nonce,
            string side,
            string tradingPair,
            decimal amount,
            decimal price,
            string orderTransactionId,
            string orderAddress,
            string takerTransactionId,
            string takerAddress,
            string partialOrFull)
        {
            TransactionOrderMatch order = new TransactionOrderMatch();
            order.Nonce = nonce;
            order.Side = side;
            order.TradingPair = tradingPair;
            order.Amount = amount;
            order.Price = price;
            order.PartialOrFull = partialOrFull;
            order.OrderTransactionId = orderTransactionId;
            order.OrderAddress = orderAddress;
            order.TakerTransactionId = takerTransactionId;
            order.TakerAddress = takerAddress;
            order.TransactionId = GenerateTransactionId(order);

            //Check if same transaction has already been received
            if (IsTransactionInPending(order.TransactionId))
            {
                return order.TransactionId;
            }

            ApplicationState.PendingRecordsAdd(order);

            //For matched orders, update index immediately instead of waiting for next block
            indexServices.OrderIndex.UpdateIndexForMatchedOrder(order);

            return order.TransactionId;
        }

    }
}
