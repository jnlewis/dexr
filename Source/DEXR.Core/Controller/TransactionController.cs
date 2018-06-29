using DEXR.Core.Configuration;
using DEXR.Core.Const;
using DEXR.Core.Cryptography;
using DEXR.Core.Model;
using DEXR.Core.Model.Requests;
using DEXR.Core.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DEXR.Core.Controller
{
    public class TransactionController : ControllerBase
    {
        public GenericResponse CreateToken(TransactionCreateTokenRequest value)
        {
            try
            {
                VerifySignature(value.Body, value.Signature, value.Body.Owner);

                if(!ApplicationState.IsChainUpToDate)
                {
                    return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.NodeNotConsensusReady);
                }

                //Perform transaction
                TransactionServices transactionService = new TransactionServices();
                string txId = transactionService.AddToken(
                    value.Body.Nonce,
                    Convert.ToDecimal(value.Body.Fee),
                    value.Body.Symbol, 
                    value.Body.Name, 
                    value.Body.TotalSupply,
                    value.Body.Decimals,
                    value.Body.Owner);

                ApplicationLog.Info("Transaction added to queue for next block. TxId: " + txId);
                return new GenericResponse(txId, ResponseCodes.Success, "Transaction added to queue for next block.");
            }
            catch (ValidationException vex)
            {
                ApplicationLog.Warn("ValidationException [" + vex.Code + "]: " + vex.Message);
                return new GenericResponse(null, vex.Code, vex.Message);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse Transfer(TransactionTransferRequest value)
        {
            try
            {
                VerifySignature(value.Body, value.Signature, value.Body.Sender);

                if (!ApplicationState.IsChainUpToDate)
                {
                    return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.NodeNotConsensusReady);
                }

                //Perform transaction
                TransactionServices transactionService = new TransactionServices();
                string txId = transactionService.AddTransfer(
                    value.Body.Nonce,
                    Convert.ToDecimal(value.Body.Fee),
                    value.Body.TokenSymbol,
                    value.Body.Sender,
                    value.Body.ToAddress,
                    Convert.ToDecimal(value.Body.Amount));

                ApplicationLog.Info("Transaction added to queue for next block. TxId: " + txId);
                return new GenericResponse(txId, ResponseCodes.Success, "Transaction added to queue for next block.");
            }
            catch (ValidationException vex)
            {
                ApplicationLog.Warn("ValidationException [" + vex.Code + "]: " + vex.Message);
                return new GenericResponse(null, vex.Code, vex.Message);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse AddOrderLimit(TransactionOrderLimitRequest value)
        {
            try
            {
                VerifySignature(value.Body, value.Signature, value.Body.Owner);

                if (!ApplicationState.IsChainUpToDate)
                {
                    return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.NodeNotConsensusReady);
                }

                //Perform transaction
                TransactionServices transactionService = new TransactionServices();
                string txId = transactionService.AddLimitOrder(
                    value.Body.Nonce,
                    Convert.ToDecimal(value.Body.Fee),
                    value.Body.PairSymbol,
                    value.Body.Side,
                    Convert.ToDecimal(value.Body.Price),
                    Convert.ToDecimal(value.Body.Amount),
                    value.Body.ExpiryBlocks,
                    value.Body.Owner);
                
                ApplicationLog.Info("Transaction added to queue for next block. TxId: " + txId);
                return new GenericResponse(txId, ResponseCodes.Success, "Transaction added to queue for next block.");
            }
            catch (ValidationException vex)
            {
                ApplicationLog.Warn("ValidationException [" + vex.Code + "]: " + vex.Message);
                return new GenericResponse(null, vex.Code, vex.Message);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse AddOrderMarket(TransactionOrderMarketRequest value)
        {
            try
            {
                VerifySignature(value.Body, value.Signature, value.Body.Owner);

                if (!ApplicationState.IsChainUpToDate)
                {
                    return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.NodeNotConsensusReady);
                }

                //Perform transaction
                TransactionServices transactionService = new TransactionServices();
                List<string> txIds = transactionService.AddMarketOrder(
                    value.Body.Nonce,
                    Convert.ToDecimal(value.Body.Fee),
                    value.Body.PairSymbol,
                    value.Body.Side,
                    Convert.ToDecimal(value.Body.Amount),
                    value.Body.Owner);

                ApplicationLog.Info("Transaction added to queue for next block.");
                return new GenericResponse(txIds, ResponseCodes.Success, "Transaction added to queue for next block.");
            }
            catch (ValidationException vex)
            {
                ApplicationLog.Warn("ValidationException [" + vex.Code + "]: " + vex.Message);
                return new GenericResponse(null, vex.Code, vex.Message);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }

        public GenericResponse CancelOrder(TransactionCancelOrderRequest value)
        {
            try
            {
                VerifySignature(value.Body, value.Signature, value.Body.Owner);

                if (!ApplicationState.IsChainUpToDate)
                {
                    return new GenericResponse(null, ResponseCodes.Success, ResponseMessages.NodeNotConsensusReady);
                }

                //Perform transaction
                TransactionServices transactionService = new TransactionServices();
                string txId = transactionService.CancelOrder(
                    value.Body.Nonce,
                    Convert.ToDecimal(value.Body.Fee),
                    value.Body.OrderTransactionId,
                    value.Body.Owner);

                ApplicationLog.Info("Transaction added to queue for next block. TxId: " + txId);
                return new GenericResponse(txId, ResponseCodes.Success, "Transaction added to queue for next block.");
            }
            catch (ValidationException vex)
            {
                ApplicationLog.Warn("ValidationException [" + vex.Code + "]: " + vex.Message);
                return new GenericResponse(null, vex.Code, vex.Message);
            }
            catch (Exception ex)
            {
                ApplicationLog.Exception(ex);
                return new GenericResponse(null, ResponseCodes.Error, ResponseMessages.Error);
            }
        }
    }
}
